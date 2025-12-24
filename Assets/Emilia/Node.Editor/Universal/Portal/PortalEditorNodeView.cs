using System;
using System.Collections.Generic;
using Emilia.Node.Attributes;
using Emilia.Node.Editor;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    /// <summary>
    /// Portal节点视图
    /// </summary>
    [EditorNode(typeof(PortalNodeAsset))]
    public class PortalEditorNodeView : UniversalEditorNodeView
    {
        private static readonly Color HighlightColor = new Color(1f, 0.8f, 0.2f, 1f);
        private static readonly Color EntryDefaultColor = new Color(0.2f, 0.6f, 0.3f);
        private static readonly Color ExitDefaultColor = new Color(0.3f, 0.4f, 0.7f);

        private PortalNodeAsset _portalAsset;

        public override bool canExpanded => false;

        public override void Initialize(EditorGraphView graphView, EditorNodeAsset asset)
        {
            _portalAsset = asset as PortalNodeAsset;
            base.Initialize(graphView, asset);

            // 设置Portal节点的默认颜色
            SetColor(GetDefaultPortalColor());
        }

        private Color GetDefaultPortalColor()
        {
            return _portalAsset.direction == PortalDirection.Entry ? EntryDefaultColor : ExitDefaultColor;
        }

        /// <summary>
        /// 从连接的边动态获取端口信息
        /// </summary>
        private (Type portType, Color portColor) GetPortInfoFromConnections()
        {
            Type portType = typeof(object);
            Color portColor = Color.white;

            if (_portalAsset == null || graphView?.graphAsset == null) return (portType, portColor);

            // 获取连接到此Portal的边
            List<EditorEdgeAsset> edges = graphView.graphAsset.GetEdges(_portalAsset.id, "portal_port");

            if (edges.Count > 0)
            {
                EditorEdgeAsset firstEdge = edges[0];

                // 根据Portal方向获取对应的端口信息
                string connectedNodeId;
                string connectedPortId;

                if (_portalAsset.direction == PortalDirection.Entry)
                {
                    // Entry Portal的输入端口连接到其他节点的输出端口
                    connectedNodeId = firstEdge.outputNodeId;
                    connectedPortId = firstEdge.outputPortId;
                }
                else
                {
                    // Exit Portal的输出端口连接到其他节点的输入端口
                    connectedNodeId = firstEdge.inputNodeId;
                    connectedPortId = firstEdge.inputPortId;
                }

                // 获取连接节点的端口视图
                IEditorNodeView connectedNodeView = graphView.graphElementCache.nodeViewById.GetValueOrDefault(connectedNodeId);
                if (connectedNodeView != null)
                {
                    IEditorPortView connectedPortView = connectedNodeView.GetPortView(connectedPortId);
                    if (connectedPortView != null)
                    {
                        portType = connectedPortView.info.portType ?? typeof(object);
                        portColor = connectedPortView.info.color;
                    }
                }
            }

            return (portType, portColor);
        }

        public override List<EditorPortInfo> CollectStaticPortAssets()
        {
            List<EditorPortInfo> portInfos = new List<EditorPortInfo>();

            if (_portalAsset == null) return portInfos;

            // 动态获取端口类型和颜色
            var (portType, portColor) = GetPortInfoFromConnections();

            EditorPortInfo portInfo = new EditorPortInfo
            {
                id = "portal_port",
                displayName = string.Empty,
                portType = portType,
                direction = _portalAsset.direction == PortalDirection.Entry
                    ? EditorPortDirection.Input
                    : EditorPortDirection.Output,
                orientation = EditorOrientation.Horizontal,
                canMultiConnect = true,
                color = portColor
            };

            portInfos.Add(portInfo);

            return portInfos;
        }

        public override void UpdateTitle()
        {
            if (_portalAsset != null)
            {
                title = _portalAsset.title;
            }
        }

        public override void Select()
        {
            base.Select();
            HighlightLinkedPortal(true);
        }

        public override void Unselect()
        {
            base.Unselect();
            HighlightLinkedPortal(false);
        }

        /// <summary>
        /// 高亮关联的Portal节点（Entry高亮Exit，Exit高亮Entry）
        /// </summary>
        private void HighlightLinkedPortal(bool highlight)
        {
            if (_portalAsset == null || string.IsNullOrEmpty(_portalAsset.portalGroupId)) return;

            // 确定需要高亮的方向（Entry找Exit，Exit找Entry）
            PortalDirection targetDirection = _portalAsset.direction == PortalDirection.Entry
                ? PortalDirection.Exit
                : PortalDirection.Entry;

            // 通过portalGroupId查找同组的目标方向Portal节点
            foreach (var kvp in graphView.graphElementCache.nodeViewById)
            {
                if (kvp.Value.asset is PortalNodeAsset otherPortal &&
                    otherPortal.portalGroupId == _portalAsset.portalGroupId &&
                    otherPortal.direction == targetDirection)
                {
                    if (kvp.Value is PortalEditorNodeView linkedPortalView)
                    {
                        if (highlight)
                        {
                            linkedPortalView.SetFocus(HighlightColor);
                        }
                        else
                        {
                            linkedPortalView.ClearFocus();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 刷新端口视图（当连接改变时调用）
        /// </summary>
        public void RefreshPortFromConnections()
        {
            // 重新构建端口视图以获取最新的端口信息
            schedule.Execute(() =>
            {
                // 更新节点颜色
                var (_, portColor) = GetPortInfoFromConnections();
                if (portColor != Color.white)
                {
                    SetColor(portColor);
                }
                else
                {
                    SetColor(GetDefaultPortalColor());
                }
            }).ExecuteLater(1);
        }
    }
}
