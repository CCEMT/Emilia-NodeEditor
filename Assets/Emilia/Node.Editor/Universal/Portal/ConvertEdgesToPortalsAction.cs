using System;
using System.Collections.Generic;
using System.Linq;
using Emilia.Node.Editor;
using UnityEditor;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    /// <summary>
    /// 将选中的边转换为Portal节点
    /// </summary>
    [Action("Convert/Convert to Portals", 7000, OperateMenuTagDefine.UniversalActionTag)]
    public class ConvertEdgesToPortalsAction : OperateMenuAction
    {
        private const float EntryPortalOffsetX = 50f;
        private const float ExitPortalOffsetX = -150f;
        private const float PortalHeight = 60f;

        public override OperateMenuActionValidity GetValidity(OperateMenuContext context)
        {
            // 检查是否有选中的边
            var selectedEdges = GetSelectedEdges(context.graphView);
            return selectedEdges.Count > 0 ? OperateMenuActionValidity.Valid : OperateMenuActionValidity.NotApplicable;
        }

        public override void Execute(OperateMenuActionContext context)
        {
            ConvertEdgesToPortals(context.graphView);
        }

        private List<IEditorEdgeView> GetSelectedEdges(EditorGraphView graphView)
        {
            return graphView.selection
                .OfType<IEditorEdgeView>()
                .Where(e => e.asset != null)
                .ToList();
        }

        private void ConvertEdgesToPortals(EditorGraphView graphView)
        {
            List<IEditorEdgeView> selectedEdges = GetSelectedEdges(graphView);
            if (selectedEdges.Count == 0) return;

            // 记录撤销
            graphView.RegisterCompleteObjectUndo("Convert Edges to Portals");

            // 用于存储创建的Exit Portal以便调整位置（多个Exit Portal连接到同一输入端口时需要调整位置）
            Dictionary<string, List<PortalNodeAsset>> exitPortalsByInputPort = new Dictionary<string, List<PortalNodeAsset>>();

            List<IEditorNodeView> createdNodeViews = new List<IEditorNodeView>();

            // 第一步：收集所有Edge信息，按输出端口分组
            Dictionary<string, List<(IEditorEdgeView edgeView, IEditorPortView outputPort, IEditorPortView inputPort)>> edgesByOutputPort =
                new Dictionary<string, List<(IEditorEdgeView, IEditorPortView, IEditorPortView)>>();

            foreach (IEditorEdgeView edgeView in selectedEdges)
            {
                if (edgeView.inputPortView == null || edgeView.outputPortView == null) continue;

                IEditorPortView outputPort = edgeView.outputPortView;
                IEditorPortView inputPort = edgeView.inputPortView;

                string outputPortKey = $"{outputPort.master.asset.id}_{outputPort.info.id}";

                if (!edgesByOutputPort.TryGetValue(outputPortKey, out var edgeList))
                {
                    edgeList = new List<(IEditorEdgeView, IEditorPortView, IEditorPortView)>();
                    edgesByOutputPort[outputPortKey] = edgeList;
                }
                edgeList.Add((edgeView, outputPort, inputPort));
            }

            // 第二步：按输出端口组处理
            foreach (var kvp in edgesByOutputPort)
            {
                var edgeList = kvp.Value;
                if (edgeList.Count == 0) continue;

                // 获取第一条边的输出端口信息来创建Entry Portal
                var firstEdgeInfo = edgeList[0];
                IEditorPortView outputPort = firstEdgeInfo.outputPort;

                // 获取输出节点位置（Entry Portal放在输出节点右侧）
                Vector2 outputNodePos = outputPort.master.asset.position.position;
                Vector2 outputNodeSize = outputPort.master.asset.position.size;

                // 生成新的Portal组ID
                string portalGroupId = Guid.NewGuid().ToString();

                // 创建Entry Portal（接收来自原始输出端口的连接）
                Vector2 entryPosition = new Vector2(outputNodePos.x + outputNodeSize.x + EntryPortalOffsetX, outputNodePos.y);

                IEditorNodeView entryNodeView = CreatePortalNode(
                    graphView,
                    PortalDirection.Entry,
                    entryPosition,
                    portalGroupId
                );

                PortalNodeAsset entryPortal = entryNodeView.asset as PortalNodeAsset;
                createdNodeViews.Add(entryNodeView);

                // 处理该输出端口的所有边
                int exitIndex = 0;
                foreach (var edgeInfo in edgeList)
                {
                    IEditorEdgeView edgeView = edgeInfo.edgeView;
                    IEditorPortView inputPort = edgeInfo.inputPort;

                    // 获取输入节点位置（Exit Portal放在输入节点左侧）
                    Vector2 inputNodePos = inputPort.master.asset.position.position;

                    string inputPortKey = $"{inputPort.master.asset.id}_{inputPort.info.id}";

                    // 检查是否需要调整位置（多个Exit Portal连接到同一输入端口）
                    if (!exitPortalsByInputPort.TryGetValue(inputPortKey, out var exitPortals))
                    {
                        exitPortals = new List<PortalNodeAsset>();
                        exitPortalsByInputPort[inputPortKey] = exitPortals;
                    }

                    // 创建Exit Portal（发送连接到原始输入端口）
                    Vector2 exitPosition = new Vector2(inputNodePos.x + ExitPortalOffsetX, inputNodePos.y);

                    // 根据已有的Exit Portal数量调整Y位置
                    float yOffset = exitPortals.Count * PortalHeight;
                    exitPosition.y += yOffset;

                    IEditorNodeView exitNodeView = CreatePortalNode(
                        graphView,
                        PortalDirection.Exit,
                        exitPosition,
                        portalGroupId
                    );

                    PortalNodeAsset exitPortal = exitNodeView.asset as PortalNodeAsset;
                    exitPortals.Add(exitPortal);
                    createdNodeViews.Add(exitNodeView);

                    // 设置Portal之间的链接关系
                    // 注意：Entry Portal的linkedPortalId只保存最后一个Exit Portal
                    // 但通过portalGroupId可以找到所有关联的Portal
                    entryPortal.linkedPortalId = exitPortal.id;
                    exitPortal.linkedPortalId = entryPortal.id;

                    // 设置显示名称
                    string portalName = $"Portal_{exitIndex + 1}";
                    if (exitIndex == 0)
                    {
                        entryPortal.displayName = portalName;
                    }
                    exitPortal.displayName = portalName;

                    // 删除原始边
                    graphView.connectSystem.Disconnect(edgeView);

                    // 创建从Exit Portal到原始输入端口的连接
                    IEditorPortView exitOutputPort = exitNodeView.GetPortView("portal_port");
                    if (exitOutputPort != null && inputPort != null)
                    {
                        graphView.connectSystem.Connect(inputPort, exitOutputPort);
                    }

                    exitIndex++;
                }

                // 创建从原始输出端口到Entry Portal的连接（只需要创建一次）
                IEditorPortView entryInputPort = entryNodeView.GetPortView("portal_port");
                if (entryInputPort != null && outputPort != null)
                {
                    graphView.connectSystem.Connect(entryInputPort, outputPort);
                }
            }

            // 清除选择并选中新创建的节点
            graphView.ClearSelection();
            foreach (var nodeView in createdNodeViews)
            {
                graphView.AddToSelection(nodeView.element);
            }

            // 保存更改
            graphView.graphSave.SetDirty();
        }

        private IEditorNodeView CreatePortalNode(
            EditorGraphView graphView,
            PortalDirection direction,
            Vector2 position,
            string portalGroupId)
        {
            // 创建Portal节点资产（端口类型和颜色从连接动态获取）
            PortalNodeAsset portalAsset = ScriptableObject.CreateInstance<PortalNodeAsset>();
            portalAsset.id = Guid.NewGuid().ToString();
            portalAsset.position = new Rect(position, new Vector2(100, 60));
            portalAsset.direction = direction;
            portalAsset.portalGroupId = portalGroupId;

            Undo.RegisterCreatedObjectUndo(portalAsset, "Create Portal Node");

            // 添加到图表
            IEditorNodeView nodeView = graphView.AddNode(portalAsset);

            return nodeView;
        }

    }
}
