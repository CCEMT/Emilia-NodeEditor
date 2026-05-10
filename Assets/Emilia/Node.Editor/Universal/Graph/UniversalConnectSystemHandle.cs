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
            if (inputPort.portDirection == EditorPortDirection.Any || outputPort.portDirection == EditorPortDirection.Any) return true;

            bool isDirectionValid = (inputPort.portDirection == EditorPortDirection.Input && outputPort.portDirection == EditorPortDirection.Output) ||
                                    (inputPort.portDirection == EditorPortDirection.Output && outputPort.portDirection == EditorPortDirection.Input);

            if (isDirectionValid == false) return false;

            Type inputType = inputPort.portElement.portType;
            Type outputType = outputPort.portElement.portType;

            if (inputType == outputType) return true;

            if (inputType == typeof(object) || outputType == typeof(object)) return true;

            if (inputType != null && outputType != null && inputType.IsAssignableFrom(outputType)) return true;

            return false;
        }

        public override void AfterConnect(EditorGraphView graphView, IEditorEdgeView edgeView)
        {
            RefreshPortalIfNeeded(graphView, edgeView.asset?.inputNodeId);
            RefreshPortalIfNeeded(graphView, edgeView.asset?.outputNodeId);
        }

        public override void AfterDisconnect(EditorGraphView graphView, EditorEdgeAsset edgeAsset)
        {
            RefreshPortalIfNeeded(graphView, edgeAsset?.inputNodeId);
            RefreshPortalIfNeeded(graphView, edgeAsset?.outputNodeId);
        }

        private void RefreshPortalIfNeeded(EditorGraphView graphView, string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId)) return;

            var nodeView = graphView.graphElementCache.nodeViewById.GetValueOrDefault(nodeId);
            if (nodeView?.asset is PortalNodeAsset portalAsset)
            {
                // 刷新当前 Portal
                PortalHelper.RefreshPortalView(nodeView);

                // 刷新同组反方向 Portal
                PortalDirection targetDirection = portalAsset.direction == PortalDirection.Entry
                    ? PortalDirection.Exit
                    : PortalDirection.Entry;
                List<IEditorNodeView> linkedPortals = PortalHelper.FindLinkedPortals(graphView, portalAsset, targetDirection);
                foreach (IEditorNodeView linkedPortal in linkedPortals)
                {
                    PortalHelper.RefreshPortalView(linkedPortal);
                }
            }
        }
    }
}
