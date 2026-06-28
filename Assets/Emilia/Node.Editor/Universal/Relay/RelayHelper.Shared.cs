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
    public static partial class RelayHelper
    {
        private static readonly Vector2 HorizontalRelaySize = new(50, 20);
        private static readonly Vector2 VerticalRelaySize = new(20, 50);
        private static readonly Vector2 RelaySpacing = new(0, 28);

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
                AddNewConnectionCount(newConnectionCountByPort, ports[i]);
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

    }
}
