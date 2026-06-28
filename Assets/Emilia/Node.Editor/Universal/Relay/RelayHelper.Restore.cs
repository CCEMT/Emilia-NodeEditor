using System.Collections.Generic;
using Emilia.Node.Editor;
using UnityEditor;
using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    public static partial class RelayHelper
    {
        public static bool CanRestoreRelay(
            EditorGraphView graphView,
            RelayEditorNodeView relayNodeView,
            IRelayRestoreEdgeDataStrategy edgeDataStrategy = null)
        {
            edgeDataStrategy ??= DefaultRelayEdgeDataStrategy.Instance;
            return TryBuildRestoreGroup(graphView, relayNodeView, edgeDataStrategy, out _);
        }

        public static bool CanRestoreRelay(
            EditorGraphView graphView,
            IEditorNodeView relayNodeView,
            IRelayRestoreEdgeDataStrategy edgeDataStrategy = null)
        {
            return relayNodeView is RelayEditorNodeView relayView &&
                   CanRestoreRelay(graphView, relayView, edgeDataStrategy);
        }

        public static RelayRestoreResult RestoreRelay(
            EditorGraphView graphView,
            RelayEditorNodeView relayNodeView,
            IRelayRestoreEdgeDataStrategy edgeDataStrategy = null)
        {
            RelayRestoreResult result = new() {relayNodeView = relayNodeView};
            edgeDataStrategy ??= DefaultRelayEdgeDataStrategy.Instance;

            if (TryBuildRestoreGroup(graphView, relayNodeView, edgeDataStrategy, out RelayRestoreGroup group) == false)
                return result;

            List<EditorEdgeAsset> restoredEdgeAssets = CreateRestoredEdgeAssets(group);
            edgeDataStrategy.PrepareRestoredEdgeAssets(group);

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            graphView.RegisterCompleteObjectUndo("Restore Relay to Edges");

            AddCreatedEdges(graphView, restoredEdgeAssets, result.createdEdgeViews, "Restore Relay to Edges");
            DeleteRelayGroup(graphView, group, result);

            RefreshRelayViews(graphView);
            graphView.ClearSelection();
            int createdCount = result.createdEdgeViews.Count;
            for (int i = 0; i < createdCount; i++)
            {
                graphView.AddToSelection(result.createdEdgeViews[i].edgeElement);
            }

            graphView.UpdateSelected();
            graphView.graphSave.SetDirty();

            Undo.CollapseUndoOperations(undoGroup);
            Undo.SetCurrentGroupName("Restore Relay to Edges");

            result.success = true;
            return result;
        }

        public static RelayRestoreResult RestoreRelay(
            EditorGraphView graphView,
            IEditorNodeView relayNodeView,
            IRelayRestoreEdgeDataStrategy edgeDataStrategy = null)
        {
            return relayNodeView is RelayEditorNodeView relayView
                ? RestoreRelay(graphView, relayView, edgeDataStrategy)
                : new RelayRestoreResult();
        }

        public static void AppendRestoreRelayContextMenu(
            ContextualMenuPopulateEvent evt,
            RelayEditorNodeView relayNodeView,
            IRelayRestoreEdgeDataStrategy edgeDataStrategy = null)
        {
            if (evt == null || relayNodeView == null) return;

            DropdownMenuAction.Status status = CanRestoreRelay(relayNodeView.graphView, relayNodeView, edgeDataStrategy)
                ? DropdownMenuAction.Status.Normal
                : DropdownMenuAction.Status.Disabled;

            evt.menu.AppendAction(
                "Relay/Revert Relays to Edges",
                _ => RestoreRelay(relayNodeView.graphView, relayNodeView, edgeDataStrategy),
                status);
        }

        private static bool TryBuildRestoreGroup(
            EditorGraphView graphView,
            RelayEditorNodeView relayNodeView,
            IRelayRestoreEdgeDataStrategy edgeDataStrategy,
            out RelayRestoreGroup group)
        {
            group = null;
            if (graphView == null || relayNodeView?.asset == null) return false;

            IEditorPortView relayInput = GetRelayInputPort(relayNodeView);
            IEditorPortView relayOutput = GetRelayOutputPort(relayNodeView);
            if (relayInput == null || relayOutput == null) return false;

            group = new RelayRestoreGroup {relayNodeView = relayNodeView};
            CollectRestoreInputEdges(relayInput, group);
            CollectRestoreOutputEdges(relayOutput, group);

            if (group.inputEdges.Count == 0 || group.outputEdges.Count == 0) return false;
            if (HasAdjacentRelay(group)) return false;

            BuildRestoreConnections(group);
            if (group.connections.Count == 0) return false;
            if (edgeDataStrategy.CanRestoreGroup(group) == false) return false;
            if (CanRestoreConnections(graphView, group) == false) return false;
            if (IsRestoreCapacitySafe(group) == false) return false;

            return true;
        }

        private static void CollectRestoreInputEdges(IEditorPortView relayInput, RelayRestoreGroup group)
        {
            IReadOnlyList<IEditorEdgeView> edges = relayInput.edges;
            int edgeCount = edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                IEditorEdgeView edge = edges[i];
                if (IsValidEdge(edge) == false) continue;
                if (edge.inputPortView != relayInput) continue;

                group.inputEdges.Add(edge);
                AddDistinctPort(group.outputPorts, edge.outputPortView);
            }

            group.inputEdges.Sort(CompareEdges);
            group.outputPorts.Sort(ComparePortKeys);
        }

        private static void CollectRestoreOutputEdges(IEditorPortView relayOutput, RelayRestoreGroup group)
        {
            IReadOnlyList<IEditorEdgeView> edges = relayOutput.edges;
            int edgeCount = edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                IEditorEdgeView edge = edges[i];
                if (IsValidEdge(edge) == false) continue;
                if (edge.outputPortView != relayOutput) continue;

                group.outputEdges.Add(edge);
                AddDistinctPort(group.inputPorts, edge.inputPortView);
            }

            group.outputEdges.Sort(CompareEdges);
            group.inputPorts.Sort(ComparePortKeys);
        }

        private static bool HasAdjacentRelay(RelayRestoreGroup group)
        {
            int inputEdgeCount = group.inputEdges.Count;
            for (int i = 0; i < inputEdgeCount; i++)
            {
                if (IsRelayPort(group.inputEdges[i].outputPortView)) return true;
            }

            int outputEdgeCount = group.outputEdges.Count;
            for (int i = 0; i < outputEdgeCount; i++)
            {
                if (IsRelayPort(group.outputEdges[i].inputPortView)) return true;
            }

            return false;
        }

        private static void BuildRestoreConnections(RelayRestoreGroup group)
        {
            group.connections.Clear();

            int inputEdgeCount = group.inputEdges.Count;
            for (int inputEdgeIndex = 0; inputEdgeIndex < inputEdgeCount; inputEdgeIndex++)
            {
                IEditorEdgeView inputSideEdge = group.inputEdges[inputEdgeIndex];

                int outputEdgeCount = group.outputEdges.Count;
                for (int outputEdgeIndex = 0; outputEdgeIndex < outputEdgeCount; outputEdgeIndex++)
                {
                    IEditorEdgeView outputSideEdge = group.outputEdges[outputEdgeIndex];
                    group.connections.Add(new RelayRestoreConnection
                    {
                        outputPort = inputSideEdge.outputPortView,
                        inputPort = outputSideEdge.inputPortView,
                        inputSideEdge = inputSideEdge,
                        outputSideEdge = outputSideEdge
                    });
                }
            }
        }

        private static bool CanRestoreConnections(EditorGraphView graphView, RelayRestoreGroup group)
        {
            List<IEditorEdgeView> replacedEdges = GetRestoreReplacedEdges(group);
            HashSet<string> newEdgeKeys = new();

            int connectionCount = group.connections.Count;
            for (int i = 0; i < connectionCount; i++)
            {
                RelayRestoreConnection connection = group.connections[i];
                if (CanConnectIgnoringCapacity(graphView, connection.inputPort, connection.outputPort) == false)
                    return false;

                string edgeKey = GetEdgeKey(connection.outputPort, connection.inputPort);
                if (newEdgeKeys.Add(edgeKey) == false) return false;
                if (HasExistingDirectEdge(graphView, connection.outputPort, connection.inputPort, replacedEdges))
                    return false;
            }

            return true;
        }

        private static bool HasExistingDirectEdge(
            EditorGraphView graphView,
            IEditorPortView outputPort,
            IEditorPortView inputPort,
            IReadOnlyList<IEditorEdgeView> replacedEdges)
        {
            int edgeCount = graphView.edgeViews.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                IEditorEdgeView edge = graphView.edgeViews[i];
                if (ContainsEdge(replacedEdges, edge)) continue;
                if (edge.outputPortView != outputPort) continue;
                if (edge.inputPortView != inputPort) continue;
                return true;
            }

            return false;
        }

        private static bool IsRestoreCapacitySafe(RelayRestoreGroup group)
        {
            Dictionary<string, (IEditorPortView port, int count)> newConnectionCountByPort = new();
            List<IEditorEdgeView> replacedEdges = GetRestoreReplacedEdges(group);

            int connectionCount = group.connections.Count;
            for (int i = 0; i < connectionCount; i++)
            {
                AddNewConnectionCount(newConnectionCountByPort, group.connections[i].outputPort);
                AddNewConnectionCount(newConnectionCountByPort, group.connections[i].inputPort);
            }

            foreach ((IEditorPortView port, int count) in newConnectionCountByPort.Values)
            {
                if (port.info.canMultiConnect) continue;

                int remainingConnectionCount = CountRemainingConnections(port, replacedEdges);
                if (remainingConnectionCount + count > 1) return false;
            }

            return true;
        }

        private static void AddNewConnectionCount(
            Dictionary<string, (IEditorPortView port, int count)> newConnectionCountByPort,
            IEditorPortView port)
        {
            if (port == null) return;

            string portKey = GetPortKey(port);
            if (newConnectionCountByPort.TryGetValue(portKey, out (IEditorPortView port, int count) value))
                newConnectionCountByPort[portKey] = (value.port, value.count + 1);
            else
                newConnectionCountByPort[portKey] = (port, 1);
        }

        private static List<IEditorEdgeView> GetRestoreReplacedEdges(RelayRestoreGroup group)
        {
            List<IEditorEdgeView> result = new();
            result.AddRange(group.inputEdges);

            int outputEdgeCount = group.outputEdges.Count;
            for (int i = 0; i < outputEdgeCount; i++)
            {
                if (ContainsEdge(result, group.outputEdges[i])) continue;
                result.Add(group.outputEdges[i]);
            }

            return result;
        }

        private static List<EditorEdgeAsset> CreateRestoredEdgeAssets(RelayRestoreGroup group)
        {
            List<EditorEdgeAsset> result = new();

            int connectionCount = group.connections.Count;
            for (int i = 0; i < connectionCount; i++)
            {
                RelayRestoreConnection connection = group.connections[i];
                IEditorEdgeView templateEdge = connection.outputSideEdge ?? connection.inputSideEdge;
                EditorEdgeAsset edgeAsset = CopyEdgeAsset(templateEdge);

                edgeAsset.outputNodeId = connection.outputPort.master.asset.id;
                edgeAsset.outputPortId = connection.outputPort.info.id;
                edgeAsset.inputNodeId = connection.inputPort.master.asset.id;
                edgeAsset.inputPortId = connection.inputPort.info.id;

                connection.restoredEdgeAsset = edgeAsset;
                result.Add(edgeAsset);
            }

            return result;
        }

        private static void DeleteRelayGroup(
            EditorGraphView graphView,
            RelayRestoreGroup group,
            RelayRestoreResult result)
        {
            List<IEditorEdgeView> replacedEdges = GetRestoreReplacedEdges(group);
            int edgeCount = replacedEdges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                IEditorEdgeView edge = replacedEdges[i];
                if (edge?.asset == null) continue;

                result.removedEdgeViews.Add(edge);
                edge.Delete();
            }

            graphView.nodeSystem.DeleteNode(group.relayNodeView);
        }

    }
}