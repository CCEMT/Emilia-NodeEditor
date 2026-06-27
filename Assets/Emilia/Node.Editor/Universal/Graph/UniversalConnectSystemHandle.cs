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
            UniversalConnectContext context = new(graphView, inputPort, outputPort);

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
            return HasAvailablePortCapacity(context.inputPort) &&
                   HasAvailablePortCapacity(context.outputPort);
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

        protected virtual bool HasAvailablePortCapacity(IEditorPortView portView)
        {
            if (portView.info.canMultiConnect) return true;
            return portView.edges.Count == 0;
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
