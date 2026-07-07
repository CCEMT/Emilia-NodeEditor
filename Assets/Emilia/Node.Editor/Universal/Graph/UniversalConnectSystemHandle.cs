using System;
using System.Collections.Generic;
using Emilia.Kit;
using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    /// <summary>
    /// 通用连接处理器
    /// </summary>
    [EditorHandle(typeof(EditorUniversalGraphAsset))]
    public class UniversalConnectSystemHandle : ConnectSystemHandle
    {
        public override Type GetConnectorListenerType(EditorGraphView graphView) => typeof(UniversalEdgeConnectorListener);

        public override Type GetEdgeAssetTypeByPort(EditorGraphView graphView, IEditorPortView portView) => typeof(UniversalEditorEdgeAsset);

        public override bool CanConnect(EditorGraphView graphView, IEditorPortView inputPort, IEditorPortView outputPort)
        {
            return CanConnect(graphView, inputPort, outputPort, ConnectValidationOptions.Default);
        }

        public override bool CanConnect(EditorGraphView graphView, IEditorPortView inputPort, IEditorPortView outputPort,
            ConnectValidationOptions options)
        {
            UniversalConnectContext context = new(graphView, inputPort, outputPort, options);
            return CanConnect(context);
        }

        protected virtual bool CanConnect(UniversalConnectContext context)
        {
            if (CanConnectByDirectionAndType(context) == false) return false;
            if (CanConnectByPortCapacity(context) == false) return false;
            if (CanConnectByNodeConstraints(context) == false) return false;

            return true;
        }

        protected virtual bool CanConnectByDirectionAndType(UniversalConnectContext context)
        {
            return context.CanConnectByDirectionAndType();
        }

        protected virtual bool CanConnectByPortCapacity(UniversalConnectContext context)
        {
            if (context.options.validateCapacity == false) return true;

            return HasAvailablePortCapacity(context, context.inputPort) &&
                   HasAvailablePortCapacity(context, context.outputPort);
        }

        protected virtual bool CanConnectByNodeConstraints(UniversalConnectContext context)
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

        protected virtual bool HasAvailablePortCapacity(UniversalConnectContext context, IEditorPortView portView)
        {
            if (portView == null) return false;
            if (portView.info.canMultiConnect) return true;

            IReadOnlyList<IEditorEdgeView> edges = portView.edges;
            int edgeCount = edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                if (IsIgnoredCapacityEdge(context, edges[i])) continue;
                return false;
            }

            return true;
        }

        protected virtual bool IsIgnoredCapacityEdge(UniversalConnectContext context, IEditorEdgeView edgeView)
        {
            if (edgeView == null) return false;
            ConnectValidationOptions options = context.options;
            string edgeAssetId = GetEdgeAssetId(edgeView);

            IReadOnlyList<IEditorEdgeView> ignoredEdges = options.ignoredCapacityEdges;
            int ignoredEdgeCount = ignoredEdges != null ? ignoredEdges.Count : 0;
            for (int i = 0; i < ignoredEdgeCount; i++)
            {
                IEditorEdgeView ignoredEdge = ignoredEdges[i];
                if (ignoredEdge == edgeView) return true;
                if (string.IsNullOrEmpty(edgeAssetId) == false &&
                    GetEdgeAssetId(ignoredEdge) == edgeAssetId)
                {
                    return true;
                }
            }

            IReadOnlyList<EditorEdgeAsset> ignoredEdgeAssets = options.ignoredCapacityEdgeAssets;
            int ignoredEdgeAssetCount = ignoredEdgeAssets != null ? ignoredEdgeAssets.Count : 0;
            for (int i = 0; i < ignoredEdgeAssetCount; i++)
            {
                EditorEdgeAsset ignoredAsset = ignoredEdgeAssets[i];
                if (ignoredAsset == edgeView.asset) return true;
                if (string.IsNullOrEmpty(edgeAssetId) == false &&
                    GetEdgeAssetId(ignoredAsset) == edgeAssetId)
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetEdgeAssetId(IEditorEdgeView edgeView)
        {
            if (edgeView == null || edgeView.asset == null) return null;
            return edgeView.asset.id;
        }

        private static string GetEdgeAssetId(EditorEdgeAsset edgeAsset)
        {
            if (edgeAsset == null) return null;
            return edgeAsset.id;
        }

        public override void AfterConnect(EditorGraphView graphView, IEditorEdgeView edgeView)
        {
            UniversalConnectContext context = new(graphView, edgeView.inputPortView, edgeView.outputPortView);
            NotifyAfterConnect(context, edgeView);
        }

        public override void AfterDisconnect(EditorGraphView graphView, EditorEdgeAsset edgeAsset)
        {
            NotifyAfterDisconnect(graphView, edgeAsset);
        }

        protected virtual void NotifyAfterConnect(UniversalConnectContext context, IEditorEdgeView edgeView)
        {
            IEditorNodeView inputNodeView = context.inputPort.master;
            IEditorNodeView outputNodeView = context.outputPort.master;

            if (inputNodeView is IUniversalConnectionChangedNodeView inputChanged)
                inputChanged.AfterConnect(context, edgeView);

            if (outputNodeView != inputNodeView &&
                outputNodeView is IUniversalConnectionChangedNodeView outputChanged)
            {
                outputChanged.AfterConnect(context, edgeView);
            }
        }

        protected virtual void NotifyAfterDisconnect(EditorGraphView graphView, EditorEdgeAsset edgeAsset)
        {
            if (edgeAsset == null) return;

            IEditorNodeView inputNodeView = GetNodeView(graphView, edgeAsset.inputNodeId);
            IEditorNodeView outputNodeView = GetNodeView(graphView, edgeAsset.outputNodeId);

            if (inputNodeView is IUniversalConnectionChangedNodeView inputChanged)
                inputChanged.AfterDisconnect(graphView, edgeAsset);

            if (outputNodeView != inputNodeView &&
                outputNodeView is IUniversalConnectionChangedNodeView outputChanged)
            {
                outputChanged.AfterDisconnect(graphView, edgeAsset);
            }
        }

        private IEditorNodeView GetNodeView(EditorGraphView graphView, string nodeId)
        {
            if (graphView == null || string.IsNullOrEmpty(nodeId)) return null;
            return graphView.graphElementCache.nodeViewById.GetValueOrDefault(nodeId);
        }
    }
}
