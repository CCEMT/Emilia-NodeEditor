using System;
using System.Collections.Generic;
using System.Linq;
using Emilia.Kit.Editor;
using Emilia.Node.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Node.Universal.Editor
{
    public static partial class RelayHelper
    {
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

        public static List<IEditorEdgeView> GetSelectedEdges(EditorGraphView graphView)
        {
            if (graphView == null) return new List<IEditorEdgeView>();
            return graphView.selection.OfType<IEditorEdgeView>().Where(edge => edge.asset != null).ToList();
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

    }
}