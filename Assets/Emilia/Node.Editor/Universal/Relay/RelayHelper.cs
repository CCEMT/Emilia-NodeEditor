using System;
using System.Collections.Generic;
using System.Linq;
using Emilia.Kit.Editor;
using Emilia.Node.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Emilia.Node.Universal.Editor
{
    public interface IRelayEdgeDataStrategy
    {
        bool CanMergeGroup(RelayInsertGroup group);

        void PrepareEdgeAssets(
            RelayInsertGroup group,
            IEditorNodeView relayNodeView,
            List<EditorEdgeAsset> inputEdges,
            List<EditorEdgeAsset> outputEdges);
    }

    public interface IRelayRestoreEdgeDataStrategy
    {
        bool CanRestoreGroup(RelayRestoreGroup group);

        void PrepareRestoredEdgeAssets(RelayRestoreGroup group);
    }

    public class DefaultRelayEdgeDataStrategy : IRelayEdgeDataStrategy, IRelayRestoreEdgeDataStrategy
    {
        public static readonly DefaultRelayEdgeDataStrategy Instance = new();

        public virtual bool CanMergeGroup(RelayInsertGroup group) => true;

        public virtual void PrepareEdgeAssets(
            RelayInsertGroup group,
            IEditorNodeView relayNodeView,
            List<EditorEdgeAsset> inputEdges,
            List<EditorEdgeAsset> outputEdges) { }

        public virtual bool CanRestoreGroup(RelayRestoreGroup group) => true;

        public virtual void PrepareRestoredEdgeAssets(RelayRestoreGroup group) { }
    }

    public class RelayInsertGroup
    {
        public List<IEditorEdgeView> edges = new();
        public List<IEditorPortView> outputPorts = new();
        public List<IEditorPortView> inputPorts = new();
    }

    public class RelayInsertResult
    {
        public List<IEditorNodeView> relayNodeViews = new();
        public List<IEditorEdgeView> createdEdgeViews = new();
        public List<IEditorEdgeView> skippedEdgeViews = new();
    }

    public class RelayRestoreConnection
    {
        public IEditorPortView outputPort;
        public IEditorPortView inputPort;
        public IEditorEdgeView inputSideEdge;
        public IEditorEdgeView outputSideEdge;
        public EditorEdgeAsset restoredEdgeAsset;
    }

    public class RelayRestoreGroup
    {
        public RelayEditorNodeView relayNodeView;
        public List<IEditorEdgeView> inputEdges = new();
        public List<IEditorEdgeView> outputEdges = new();
        public List<IEditorPortView> outputPorts = new();
        public List<IEditorPortView> inputPorts = new();
        public List<RelayRestoreConnection> connections = new();
    }

    public class RelayRestoreResult
    {
        public bool success;
        public RelayEditorNodeView relayNodeView;
        public List<IEditorEdgeView> createdEdgeViews = new();
        public List<IEditorEdgeView> removedEdgeViews = new();
    }

    public static class RelayHelper
    {
        public const string InputPortId = "relay_input";
        public const string OutputPortId = "relay_output";

        private static readonly Vector2 HorizontalRelaySize = new(50, 20);
        private static readonly Vector2 VerticalRelaySize = new(20, 50);
        private static readonly Vector2 RelaySpacing = new(0, 28);

        public static bool IsRelayPort(IEditorPortView portView)
        {
            return portView?.master is RelayEditorNodeView;
        }

        public static bool IsRelayInputPort(IEditorPortView portView)
        {
            return portView?.master is RelayEditorNodeView && portView.info.id == InputPortId;
        }

        public static bool IsRelayOutputPort(IEditorPortView portView)
        {
            return portView?.master is RelayEditorNodeView && portView.info.id == OutputPortId;
        }

        public static IEditorPortView GetRelayInputPort(IEditorNodeView relayNodeView)
        {
            return relayNodeView?.GetPortView(InputPortId);
        }

        public static IEditorPortView GetRelayOutputPort(IEditorNodeView relayNodeView)
        {
            return relayNodeView?.GetPortView(OutputPortId);
        }

        public static IEditorPortView ResolveSourceOutputPort(IEditorPortView outputPort)
        {
            List<IEditorPortView> sourceOutputs = ResolveSourceOutputPorts(outputPort);
            return sourceOutputs.Count == 1 ? sourceOutputs[0] : outputPort;
        }

        public static List<IEditorPortView> ResolveSourceOutputPorts(IEditorPortView outputPort)
        {
            List<IEditorPortView> result = new();
            if (outputPort == null) return result;

            if (IsRelayOutputPort(outputPort))
            {
                List<IEditorPortView> sourceOutputs = GetConnectedSourceOutputs(outputPort.master as RelayEditorNodeView);
                if (sourceOutputs.Count > 0) return sourceOutputs;
            }

            result.Add(outputPort);
            return result;
        }

        public static IEditorPortView ResolveTargetInputPort(IEditorPortView inputPort)
        {
            List<IEditorPortView> targetInputs = ResolveTargetInputPorts(inputPort);
            return targetInputs.Count == 1 ? targetInputs[0] : inputPort;
        }

        public static List<IEditorPortView> ResolveTargetInputPorts(IEditorPortView inputPort)
        {
            List<IEditorPortView> result = new();
            if (inputPort == null) return result;

            if (IsRelayInputPort(inputPort))
            {
                List<IEditorPortView> targetInputs = GetConnectedTargetInputs(inputPort.master as RelayEditorNodeView);
                if (targetInputs.Count > 0) return targetInputs;
            }

            result.Add(inputPort);
            return result;
        }

        public static IEditorPortView GetConnectedSourceOutput(RelayEditorNodeView relayView, HashSet<string> visited = null)
        {
            List<IEditorPortView> sourceOutputs = GetConnectedSourceOutputs(relayView, visited);
            return sourceOutputs.Count > 0 ? sourceOutputs[0] : null;
        }

        public static List<IEditorPortView> GetConnectedSourceOutputs(RelayEditorNodeView relayView, HashSet<string> visited = null)
        {
            List<IEditorPortView> result = new();
            CollectConnectedSourceOutputs(relayView, result, visited);
            return result;
        }

        public static IEditorPortView GetConnectedTargetInput(RelayEditorNodeView relayView, HashSet<string> visited = null)
        {
            List<IEditorPortView> targetInputs = GetConnectedTargetInputs(relayView, visited);
            return targetInputs.Count > 0 ? targetInputs[0] : null;
        }

        public static List<IEditorPortView> GetConnectedTargetInputs(RelayEditorNodeView relayView, HashSet<string> visited = null)
        {
            List<IEditorPortView> result = new();
            CollectConnectedTargetInputs(relayView, result, visited);
            return result;
        }

        private static void CollectConnectedSourceOutputs(
            RelayEditorNodeView relayView,
            List<IEditorPortView> result,
            HashSet<string> visited = null)
        {
            if (relayView == null) return;

            visited ??= new HashSet<string>();
            if (visited.Add(relayView.asset.id) == false) return;

            IEditorPortView inputPort = GetRelayInputPort(relayView);
            if (inputPort == null) return;

            IReadOnlyList<IEditorEdgeView> edges = inputPort.edges;
            int edgeCount = edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                IEditorPortView sourceOutput = edges[i].outputPortView;
                if (IsRelayOutputPort(sourceOutput))
                {
                    CollectConnectedSourceOutputs(sourceOutput.master as RelayEditorNodeView, result, new HashSet<string>(visited));
                    continue;
                }

                AddDistinctPort(result, sourceOutput);
            }
        }

        private static void CollectConnectedTargetInputs(
            RelayEditorNodeView relayView,
            List<IEditorPortView> result,
            HashSet<string> visited = null)
        {
            if (relayView == null) return;

            visited ??= new HashSet<string>();
            if (visited.Add(relayView.asset.id) == false) return;

            IEditorPortView outputPort = GetRelayOutputPort(relayView);
            if (outputPort == null) return;

            IReadOnlyList<IEditorEdgeView> edges = outputPort.edges;
            int edgeCount = edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                IEditorPortView targetInput = edges[i].inputPortView;
                if (IsRelayInputPort(targetInput))
                {
                    CollectConnectedTargetInputs(targetInput.master as RelayEditorNodeView, result, new HashSet<string>(visited));
                    continue;
                }

                AddDistinctPort(result, targetInput);
            }
        }

        public static IEditorNodeView CreateRelayNode<TRelayNodeAsset>(
            EditorGraphView graphView,
            Vector2 position,
            bool registerUndo = true,
            EditorOrientation portOrientation = EditorOrientation.Horizontal)
            where TRelayNodeAsset : RelayNodeAsset
        {
            TRelayNodeAsset relayAsset = ScriptableObject.CreateInstance<TRelayNodeAsset>();
            relayAsset.id = Guid.NewGuid().ToString();
            relayAsset.portOrientation = portOrientation;
            relayAsset.position = new Rect(position, GetRelaySize(portOrientation));

            if (registerUndo)
            {
                Undo.RegisterCreatedObjectUndo(relayAsset, "Create Relay Node");
                graphView.RegisterCompleteObjectUndo("Create Relay Node");
            }

            return graphView.AddNode(relayAsset);
        }

        public static bool CanInsertRelay<TRelayNodeAsset>(
            EditorGraphView graphView,
            IEnumerable<IEditorEdgeView> edgeViews,
            IRelayEdgeDataStrategy edgeDataStrategy = null)
            where TRelayNodeAsset : RelayNodeAsset
        {
            List<IEditorEdgeView> edges = CollectInsertableEdges<TRelayNodeAsset>(graphView, edgeViews);
            if (edges.Count == 0) return false;

            List<RelayInsertGroup> groups = BuildInsertGroups(edges, edgeDataStrategy);
            return groups.Count > 0;
        }

        public static RelayInsertResult InsertRelay<TRelayNodeAsset>(
            EditorGraphView graphView,
            IEnumerable<IEditorEdgeView> edgeViews,
            Vector2 fallbackPosition,
            IRelayEdgeDataStrategy edgeDataStrategy = null)
            where TRelayNodeAsset : RelayNodeAsset
        {
            RelayInsertResult result = new();
            if (graphView == null || edgeViews == null) return result;

            edgeDataStrategy ??= DefaultRelayEdgeDataStrategy.Instance;
            List<IEditorEdgeView> selectedEdges = edgeViews.Where(edge => edge?.asset != null).ToList();
            List<IEditorEdgeView> insertableEdges = CollectInsertableEdges<TRelayNodeAsset>(graphView, selectedEdges);
            result.skippedEdgeViews.AddRange(selectedEdges.Where(edge => insertableEdges.Contains(edge) == false));

            List<RelayInsertGroup> groups = BuildInsertGroups(insertableEdges, edgeDataStrategy);
            if (groups.Count == 0) return result;

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            graphView.RegisterCompleteObjectUndo("Insert Relay Node");

            int groupCount = groups.Count;
            for (int i = 0; i < groupCount; i++)
            {
                Vector2 position = CalculateRelayPosition(groups[i], fallbackPosition, i, groupCount);
                InsertRelayGroup<TRelayNodeAsset>(graphView, groups[i], position, edgeDataStrategy, result);
            }

            RefreshRelayViews(graphView);
            SelectCreatedRelays(graphView, result.relayNodeViews);
            graphView.graphSave.SetDirty();

            Undo.CollapseUndoOperations(undoGroup);
            Undo.SetCurrentGroupName("Insert Relay Node");

            return result;
        }

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
                "Relay/还原为原 Edge 连接",
                _ => RestoreRelay(relayNodeView.graphView, relayNodeView, edgeDataStrategy),
                status);
        }

        public static List<IEditorEdgeView> GetSelectedEdges(EditorGraphView graphView)
        {
            if (graphView == null) return new List<IEditorEdgeView>();
            return graphView.selection.OfType<IEditorEdgeView>().Where(edge => edge.asset != null).ToList();
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

        public static List<RelayInsertGroup> BuildInsertGroups(
            IReadOnlyList<IEditorEdgeView> edgeViews,
            IRelayEdgeDataStrategy edgeDataStrategy = null)
        {
            edgeDataStrategy ??= DefaultRelayEdgeDataStrategy.Instance;
            List<IEditorEdgeView> edges = edgeViews?.Where(IsValidEdge).Distinct().ToList() ?? new List<IEditorEdgeView>();
            if (edges.Count == 0) return new List<RelayInsertGroup>();

            RelayInsertGroup wholeGroup = CreateGroup(edges);
            List<RelayInsertGroup> wholeGroups = new() {wholeGroup};
            if (IsCompleteBipartite(wholeGroup) &&
                edgeDataStrategy.CanMergeGroup(wholeGroup) &&
                IsGroupListCapacitySafe(wholeGroups))
            {
                return new List<RelayInsertGroup> {wholeGroup};
            }

            List<RelayInsertGroup> byInputs = BuildGroupsByEndpointSet(edges, GroupByInputTargetSet, edgeDataStrategy);
            List<RelayInsertGroup> byOutputs = BuildGroupsByEndpointSet(edges, GroupByOutputSourceSet, edgeDataStrategy);
            bool byInputsCapacitySafe = IsGroupListCapacitySafe(byInputs);
            bool byOutputsCapacitySafe = IsGroupListCapacitySafe(byOutputs);

            if (byInputsCapacitySafe && byOutputsCapacitySafe)
            {
                if (byInputs.Count < byOutputs.Count) return byInputs;
                if (byOutputs.Count < byInputs.Count) return byOutputs;
                return byInputs;
            }

            if (byInputsCapacitySafe) return byInputs;
            if (byOutputsCapacitySafe) return byOutputs;

            List<RelayInsertGroup> singleEdgeGroups = BuildSingleEdgeGroups(edges, edgeDataStrategy);
            return IsGroupListCapacitySafe(singleEdgeGroups) ? singleEdgeGroups : new List<RelayInsertGroup>();
        }

        private static List<IEditorEdgeView> CollectInsertableEdges<TRelayNodeAsset>(
            EditorGraphView graphView,
            IEnumerable<IEditorEdgeView> edgeViews)
            where TRelayNodeAsset : RelayNodeAsset
        {
            List<IEditorEdgeView> result = new();
            if (graphView == null || edgeViews == null) return result;

            List<IEditorEdgeView> candidates = new();
            foreach (IEditorEdgeView edgeView in edgeViews)
            {
                if (IsValidEdge(edgeView) == false) continue;
                IEditorNodeView relayNodeView = CreateRelayPreview<TRelayNodeAsset>(graphView, edgeView);
                if (relayNodeView == null) continue;

                if (CanInsertRelayForEdgeIgnoringCapacity(graphView, relayNodeView, edgeView))
                    candidates.Add(edgeView);

                DestroyRelayPreview(relayNodeView);
            }

            foreach (IEditorEdgeView edgeView in candidates)
            {
                if (HasCapacityAfterReplacing(edgeView.outputPortView, candidates) == false) continue;
                if (HasCapacityAfterReplacing(edgeView.inputPortView, candidates) == false) continue;
                result.Add(edgeView);
            }

            return result;
        }

        private static bool CanInsertRelayForEdgeIgnoringCapacity(
            EditorGraphView graphView,
            IEditorNodeView relayNodeView,
            IEditorEdgeView edgeView)
        {
            if (relayNodeView is not RelayEditorNodeView relayView) return false;

            IEditorPortView relayInput = GetRelayInputPort(relayNodeView);
            IEditorPortView relayOutput = GetRelayOutputPort(relayNodeView);
            if (relayInput == null || relayOutput == null) return false;

            if (CanConnectIgnoringCapacity(graphView, relayInput, edgeView.outputPortView) == false) return false;
            if (CanConnectIgnoringCapacity(graphView, edgeView.inputPortView, relayOutput) == false) return false;

            UniversalConnectContext throughContext = new(graphView, edgeView.inputPortView, edgeView.outputPortView);
            return relayView.CanConnectRelayThrough(throughContext, edgeView.outputPortView, edgeView.inputPortView);
        }

        private static bool CanConnectIgnoringCapacity(
            EditorGraphView graphView,
            IEditorPortView inputPort,
            IEditorPortView outputPort)
        {
            UniversalConnectContext context = new(graphView, inputPort, outputPort);
            if (context.CanConnectByDirectionAndType() == false) return false;
            return CanConnectByNodeConstraints(context);
        }

        private static bool CanConnectByNodeConstraints(UniversalConnectContext context)
        {
            IEditorNodeView inputNodeView = context.inputPort.master;
            IEditorNodeView outputNodeView = context.outputPort.master;

            if (inputNodeView is IUniversalConnectConstraintNodeView inputConstraint &&
                inputConstraint.CanConnect(context) == false)
            {
                return false;
            }

            if (outputNodeView != inputNodeView &&
                outputNodeView is IUniversalConnectConstraintNodeView outputConstraint &&
                outputConstraint.CanConnect(context) == false)
            {
                return false;
            }

            return true;
        }

        private static IEditorNodeView CreateRelayPreview<TRelayNodeAsset>(EditorGraphView graphView, IEditorEdgeView edgeView)
            where TRelayNodeAsset : RelayNodeAsset
        {
            TRelayNodeAsset relayAsset = ScriptableObject.CreateInstance<TRelayNodeAsset>();
            EditorOrientation portOrientation = ResolveRelayOrientation(edgeView);

            relayAsset.id = Guid.NewGuid().ToString();
            relayAsset.portOrientation = portOrientation;
            relayAsset.position = new Rect(Vector2.zero, GetRelaySize(portOrientation));

            Type nodeViewType = GraphTypeCache.GetNodeViewType(typeof(TRelayNodeAsset));
            if (nodeViewType == null)
            {
                Object.DestroyImmediate(relayAsset);
                return null;
            }

            IEditorNodeView nodeView = Activator.CreateInstance(nodeViewType) as IEditorNodeView;
            if (nodeView == null)
            {
                Object.DestroyImmediate(relayAsset);
                return null;
            }

            nodeView.Initialize(graphView, relayAsset);
            return nodeView;
        }

        private static void DestroyRelayPreview(IEditorNodeView relayNodeView)
        {
            if (relayNodeView == null) return;
            EditorNodeAsset relayAsset = relayNodeView.asset;
            relayNodeView.Dispose();
            if (relayAsset != null) Object.DestroyImmediate(relayAsset);
        }

        private static List<RelayInsertGroup> BuildGroupsByEndpointSet(
            List<IEditorEdgeView> edges,
            Func<List<IEditorEdgeView>, List<List<IEditorEdgeView>>> groupBuilder,
            IRelayEdgeDataStrategy edgeDataStrategy)
        {
            List<RelayInsertGroup> result = new();
            List<List<IEditorEdgeView>> edgeGroups = groupBuilder(edges);

            int groupCount = edgeGroups.Count;
            for (int i = 0; i < groupCount; i++)
            {
                AppendStrategySafeGroups(CreateGroup(edgeGroups[i]), edgeDataStrategy, result);
            }

            return result;
        }

        private static List<RelayInsertGroup> BuildSingleEdgeGroups(
            List<IEditorEdgeView> edges,
            IRelayEdgeDataStrategy edgeDataStrategy)
        {
            List<RelayInsertGroup> result = new();
            int edgeCount = edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                RelayInsertGroup singleEdgeGroup = CreateGroup(new List<IEditorEdgeView> {edges[i]});
                if (edgeDataStrategy.CanMergeGroup(singleEdgeGroup))
                    result.Add(singleEdgeGroup);
            }

            return result;
        }

        private static List<List<IEditorEdgeView>> GroupByInputTargetSet(List<IEditorEdgeView> edges)
        {
            return edges
                .GroupBy(edge => GetPortKey(edge.outputPortView))
                .GroupBy(group => CreatePortSetKey(group.Select(edge => edge.inputPortView)))
                .OrderBy(group => group.Key)
                .Select(group => group.SelectMany(item => item).ToList())
                .ToList();
        }

        private static List<List<IEditorEdgeView>> GroupByOutputSourceSet(List<IEditorEdgeView> edges)
        {
            return edges
                .GroupBy(edge => GetPortKey(edge.inputPortView))
                .GroupBy(group => CreatePortSetKey(group.Select(edge => edge.outputPortView)))
                .OrderBy(group => group.Key)
                .Select(group => group.SelectMany(item => item).ToList())
                .ToList();
        }

        private static void AppendStrategySafeGroups(
            RelayInsertGroup group,
            IRelayEdgeDataStrategy edgeDataStrategy,
            List<RelayInsertGroup> result)
        {
            if (group.edges.Count == 0) return;

            if (IsCompleteBipartite(group) && edgeDataStrategy.CanMergeGroup(group))
            {
                result.Add(group);
                return;
            }

            int edgeCount = group.edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                RelayInsertGroup singleEdgeGroup = CreateGroup(new List<IEditorEdgeView> {group.edges[i]});
                if (edgeDataStrategy.CanMergeGroup(singleEdgeGroup))
                    result.Add(singleEdgeGroup);
            }
        }

        private static RelayInsertGroup CreateGroup(List<IEditorEdgeView> edges)
        {
            RelayInsertGroup group = new();
            group.edges.AddRange(edges);

            int edgeCount = edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                AddDistinctPort(group.outputPorts, edges[i].outputPortView);
                AddDistinctPort(group.inputPorts, edges[i].inputPortView);
            }

            group.outputPorts.Sort(ComparePortKeys);
            group.inputPorts.Sort(ComparePortKeys);
            group.edges.Sort(CompareEdges);
            return group;
        }

        private static bool IsCompleteBipartite(RelayInsertGroup group)
        {
            if (group.outputPorts.Count == 0 || group.inputPorts.Count == 0) return false;
            if (group.edges.Count != group.outputPorts.Count * group.inputPorts.Count) return false;

            HashSet<string> edgeKeys = new();
            int edgeCount = group.edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                edgeKeys.Add(GetEdgeKey(group.edges[i].outputPortView, group.edges[i].inputPortView));
            }

            int outputCount = group.outputPorts.Count;
            for (int outputIndex = 0; outputIndex < outputCount; outputIndex++)
            {
                int inputCount = group.inputPorts.Count;
                for (int inputIndex = 0; inputIndex < inputCount; inputIndex++)
                {
                    if (edgeKeys.Contains(GetEdgeKey(group.outputPorts[outputIndex], group.inputPorts[inputIndex])) == false)
                        return false;
                }
            }

            return true;
        }

        private static bool IsGroupListCapacitySafe(List<RelayInsertGroup> groups)
        {
            if (groups == null || groups.Count == 0) return false;

            Dictionary<string, (IEditorPortView port, int count)> newConnectionCountByPort = new();
            List<IEditorEdgeView> replacedEdges = groups.SelectMany(group => group.edges).Distinct().ToList();

            int groupCount = groups.Count;
            for (int groupIndex = 0; groupIndex < groupCount; groupIndex++)
            {
                RelayInsertGroup group = groups[groupIndex];
                AddNewConnectionCounts(newConnectionCountByPort, group.outputPorts);
                AddNewConnectionCounts(newConnectionCountByPort, group.inputPorts);
            }

            foreach ((IEditorPortView port, int count) in newConnectionCountByPort.Values)
            {
                if (port.info.canMultiConnect) continue;

                int remainingConnectionCount = CountRemainingConnections(port, replacedEdges);
                if (remainingConnectionCount + count > 1) return false;
            }

            return true;
        }

        private static void AddNewConnectionCounts(
            Dictionary<string, (IEditorPortView port, int count)> newConnectionCountByPort,
            List<IEditorPortView> ports)
        {
            int portCount = ports.Count;
            for (int i = 0; i < portCount; i++)
            {
                IEditorPortView port = ports[i];
                string portKey = GetPortKey(port);
                if (newConnectionCountByPort.TryGetValue(portKey, out (IEditorPortView port, int count) value))
                    newConnectionCountByPort[portKey] = (value.port, value.count + 1);
                else
                    newConnectionCountByPort[portKey] = (port, 1);
            }
        }

        private static bool HasCapacityAfterReplacing(
            IEditorPortView portView,
            IReadOnlyList<IEditorEdgeView> replacedEdges)
        {
            if (portView == null) return false;
            if (portView.info.canMultiConnect) return true;
            return CountRemainingConnections(portView, replacedEdges) == 0;
        }

        private static int CountRemainingConnections(
            IEditorPortView portView,
            IReadOnlyList<IEditorEdgeView> replacedEdges)
        {
            int remainingCount = 0;
            IReadOnlyList<IEditorEdgeView> edges = portView.edges;
            int edgeCount = edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                if (ContainsEdge(replacedEdges, edges[i])) continue;
                remainingCount++;
            }

            return remainingCount;
        }

        private static bool ContainsEdge(
            IReadOnlyList<IEditorEdgeView> edges,
            IEditorEdgeView targetEdge)
        {
            if (targetEdge == null) return false;

            int edgeCount = edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                if (edges[i] == targetEdge) return true;
                if (string.IsNullOrEmpty(targetEdge.asset?.id) == false &&
                    edges[i]?.asset?.id == targetEdge.asset.id)
                {
                    return true;
                }
            }

            return false;
        }

        private static void InsertRelayGroup<TRelayNodeAsset>(
            EditorGraphView graphView,
            RelayInsertGroup group,
            Vector2 position,
            IRelayEdgeDataStrategy edgeDataStrategy,
            RelayInsertResult result)
            where TRelayNodeAsset : RelayNodeAsset
        {
            EditorOrientation portOrientation = ResolveRelayOrientation(group);
            IEditorNodeView relayNodeView = CreateRelayNode<TRelayNodeAsset>(graphView, position, false, portOrientation);
            if (relayNodeView == null) return;

            Undo.RegisterCreatedObjectUndo(relayNodeView.asset, "Insert Relay Node");

            IEditorPortView relayInput = GetRelayInputPort(relayNodeView);
            IEditorPortView relayOutput = GetRelayOutputPort(relayNodeView);
            if (relayInput == null || relayOutput == null) return;

            List<EditorEdgeAsset> inputEdges = CreateInputEdgeAssets(group, relayInput);
            List<EditorEdgeAsset> outputEdges = CreateOutputEdgeAssets(group, relayOutput);
            edgeDataStrategy.PrepareEdgeAssets(group, relayNodeView, inputEdges, outputEdges);

            AddCreatedEdges(graphView, inputEdges, result);
            AddCreatedEdges(graphView, outputEdges, result);

            int edgeCount = group.edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                group.edges[i].Delete();
            }

            result.relayNodeViews.Add(relayNodeView);
        }

        private static List<EditorEdgeAsset> CreateInputEdgeAssets(RelayInsertGroup group, IEditorPortView relayInput)
        {
            List<EditorEdgeAsset> result = new();
            int outputCount = group.outputPorts.Count;
            for (int i = 0; i < outputCount; i++)
            {
                IEditorPortView outputPort = group.outputPorts[i];
                IEditorEdgeView templateEdge = group.edges.First(edge => edge.outputPortView == outputPort);
                EditorEdgeAsset edgeAsset = CopyEdgeAsset(templateEdge);
                edgeAsset.outputNodeId = outputPort.master.asset.id;
                edgeAsset.outputPortId = outputPort.info.id;
                edgeAsset.inputNodeId = relayInput.master.asset.id;
                edgeAsset.inputPortId = relayInput.info.id;
                result.Add(edgeAsset);
            }

            return result;
        }

        private static List<EditorEdgeAsset> CreateOutputEdgeAssets(RelayInsertGroup group, IEditorPortView relayOutput)
        {
            List<EditorEdgeAsset> result = new();
            int inputCount = group.inputPorts.Count;
            for (int i = 0; i < inputCount; i++)
            {
                IEditorPortView inputPort = group.inputPorts[i];
                IEditorEdgeView templateEdge = group.edges.First(edge => edge.inputPortView == inputPort);
                EditorEdgeAsset edgeAsset = CopyEdgeAsset(templateEdge);
                edgeAsset.outputNodeId = relayOutput.master.asset.id;
                edgeAsset.outputPortId = relayOutput.info.id;
                edgeAsset.inputNodeId = inputPort.master.asset.id;
                edgeAsset.inputPortId = inputPort.info.id;
                result.Add(edgeAsset);
            }

            return result;
        }

        private static EditorEdgeAsset CopyEdgeAsset(IEditorEdgeView edgeView)
        {
            IEdgeCopyPastePack copyPastePack = edgeView.GetPack() as IEdgeCopyPastePack;
            EditorEdgeAsset edgeAsset = Object.Instantiate(copyPastePack.copyAsset);
            edgeAsset.name = copyPastePack.copyAsset.name;
            edgeAsset.id = Guid.NewGuid().ToString();
            edgeAsset.PasteChild();
            return edgeAsset;
        }

        private static void AddCreatedEdges(
            EditorGraphView graphView,
            List<EditorEdgeAsset> edgeAssets,
            RelayInsertResult result)
        {
            AddCreatedEdges(graphView, edgeAssets, result.createdEdgeViews, "Insert Relay Node");
        }

        private static void AddCreatedEdges(
            EditorGraphView graphView,
            List<EditorEdgeAsset> edgeAssets,
            List<IEditorEdgeView> createdEdgeViews,
            string undoName)
        {
            int edgeCount = edgeAssets.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                IEditorEdgeView edgeView = graphView.AddEdge(edgeAssets[i]);
                if (edgeView != null)
                {
                    createdEdgeViews.Add(edgeView);
                    edgeView.ForceUpdateView();
                }

                Undo.RegisterCreatedObjectUndo(edgeAssets[i], undoName);
            }
        }

        private static Vector2 CalculateRelayPosition(
            RelayInsertGroup group,
            Vector2 fallbackPosition,
            int groupIndex,
            int groupCount)
        {
            Vector2 position = Vector2.zero;
            int edgeCount = group.edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                position += GetEdgeCenterInContent(group.edges[i]);
            }

            if (edgeCount == 0) position = fallbackPosition;
            else position /= edgeCount;

            if (groupCount > 1)
                position += RelaySpacing * (groupIndex - (groupCount - 1) * 0.5f);

            return position;
        }

        private static EditorOrientation ResolveRelayOrientation(IEditorEdgeView edgeView)
        {
            if (edgeView == null) return EditorOrientation.Horizontal;

            if (edgeView.outputPortView?.info.orientation == EditorOrientation.Custom ||
                edgeView.inputPortView?.info.orientation == EditorOrientation.Custom)
            {
                return EditorOrientation.Custom;
            }

            if (edgeView.outputPortView?.info.orientation == EditorOrientation.Vertical ||
                edgeView.inputPortView?.info.orientation == EditorOrientation.Vertical)
            {
                return EditorOrientation.Vertical;
            }

            return EditorOrientation.Horizontal;
        }

        private static EditorOrientation ResolveRelayOrientation(RelayInsertGroup group)
        {
            if (group == null) return EditorOrientation.Horizontal;

            if (ContainsOrientation(group.outputPorts, EditorOrientation.Custom) ||
                ContainsOrientation(group.inputPorts, EditorOrientation.Custom))
            {
                return EditorOrientation.Custom;
            }

            if (ContainsOrientation(group.outputPorts, EditorOrientation.Vertical) ||
                ContainsOrientation(group.inputPorts, EditorOrientation.Vertical))
            {
                return EditorOrientation.Vertical;
            }

            return EditorOrientation.Horizontal;
        }

        private static bool ContainsOrientation(List<IEditorPortView> ports, EditorOrientation orientation)
        {
            if (ports == null) return false;

            int portCount = ports.Count;
            for (int i = 0; i < portCount; i++)
            {
                if (ports[i]?.info.orientation == orientation) return true;
            }

            return false;
        }

        private static Vector2 GetRelaySize(EditorOrientation orientation)
        {
            return orientation == EditorOrientation.Vertical ? VerticalRelaySize : HorizontalRelaySize;
        }

        private static bool IsValidEdge(IEditorEdgeView edgeView)
        {
            return edgeView?.asset != null &&
                   edgeView.inputPortView != null &&
                   edgeView.outputPortView != null &&
                   edgeView.inputPortView.master != null &&
                   edgeView.outputPortView.master != null;
        }

        private static Vector2 GetEdgeCenterInContent(IEditorEdgeView edgeView)
        {
            if (edgeView is EditorEdgeView editorEdgeView)
            {
                Vector2 edgePoint = editorEdgeView.GetPointByRate(0.5f);
                return editorEdgeView.ChangeCoordinatesTo(edgeView.graphView.contentViewContainer, edgePoint);
            }

            Vector2 outputCenter = edgeView.outputPortView.portElement.worldBound.center;
            Vector2 inputCenter = edgeView.inputPortView.portElement.worldBound.center;
            return edgeView.graphView.contentViewContainer.WorldToLocal((outputCenter + inputCenter) * 0.5f);
        }

        private static void SelectCreatedRelays(EditorGraphView graphView, List<IEditorNodeView> relayNodeViews)
        {
            graphView.ClearSelection();
            int relayCount = relayNodeViews.Count;
            for (int i = 0; i < relayCount; i++)
            {
                graphView.AddToSelection(relayNodeViews[i].element);
            }

            graphView.UpdateSelected();
        }

        private static void AddDistinctPort(List<IEditorPortView> ports, IEditorPortView port)
        {
            if (port == null) return;
            if (ports.Any(item => GetPortKey(item) == GetPortKey(port))) return;
            ports.Add(port);
        }

        private static int ComparePortKeys(IEditorPortView a, IEditorPortView b)
        {
            return string.Compare(GetPortKey(a), GetPortKey(b), StringComparison.Ordinal);
        }

        private static int CompareEdges(IEditorEdgeView a, IEditorEdgeView b)
        {
            return string.Compare(GetEdgeKey(a.outputPortView, a.inputPortView), GetEdgeKey(b.outputPortView, b.inputPortView), StringComparison.Ordinal);
        }

        public static string GetPortKey(IEditorPortView portView)
        {
            if (portView?.master?.asset == null) return string.Empty;
            return $"{portView.master.asset.id}_{portView.info.id}";
        }

        private static string CreatePortSetKey(IEnumerable<IEditorPortView> portViews)
        {
            return string.Join("|", portViews.Select(GetPortKey).OrderBy(key => key));
        }

        private static string GetEdgeKey(IEditorPortView outputPort, IEditorPortView inputPort)
        {
            return $"{GetPortKey(outputPort)}->{GetPortKey(inputPort)}";
        }

        public static void RefreshRelayView(IEditorNodeView relayNodeView)
        {
            if (relayNodeView is RelayEditorNodeView relayView)
                relayView.RefreshPortFromConnections();
        }

        public static void RefreshRelayViews(EditorGraphView graphView)
        {
            if (graphView == null) return;

            foreach (IEditorNodeView nodeView in graphView.graphElementCache.nodeViewById.Values)
            {
                RefreshRelayView(nodeView);
            }
        }
    }
}
