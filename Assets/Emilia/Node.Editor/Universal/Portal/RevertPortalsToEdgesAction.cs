using System.Collections.Generic;
using System.Linq;
using Emilia.Node.Editor;
using UnityEditor.Experimental.GraphView;

namespace Emilia.Node.Universal.Editor
{
    /// <summary>
    /// 将选中的Portal节点还原为普通边
    /// </summary>
    [Action("Convert/Revert Portals to Edges", 7001, OperateMenuTagDefine.UniversalActionTag)]
    public class RevertPortalsToEdgesAction : OperateMenuAction
    {
        public override OperateMenuActionValidity GetValidity(OperateMenuContext context)
        {
            var selectedPortals = GetSelectedPortalNodes(context.graphView);
            return selectedPortals.Count > 0 ? OperateMenuActionValidity.Valid : OperateMenuActionValidity.NotApplicable;
        }

        public override void Execute(OperateMenuActionContext context)
        {
            RevertPortalsToEdges(context.graphView);
        }

        private List<IEditorNodeView> GetSelectedPortalNodes(EditorGraphView graphView)
        {
            return graphView.selection
                .OfType<IEditorNodeView>()
                .Where(n => n.asset is PortalNodeAsset)
                .ToList();
        }

        private void RevertPortalsToEdges(EditorGraphView graphView)
        {
            List<IEditorNodeView> selectedPortals = GetSelectedPortalNodes(graphView);
            if (selectedPortals.Count == 0) return;

            // 记录撤销
            graphView.RegisterCompleteObjectUndo("Revert Portals to Edges");

            // 收集所有需要处理的Portal组
            HashSet<string> processedGroups = new HashSet<string>();
            List<(IEditorPortView fromPort, IEditorPortView toPort)> connectionsToCreate = new List<(IEditorPortView, IEditorPortView)>();
            List<IEditorNodeView> portalsToDelete = new List<IEditorNodeView>();

            foreach (IEditorNodeView nodeView in selectedPortals)
            {
                PortalNodeAsset portalAsset = nodeView.asset as PortalNodeAsset;
                if (portalAsset == null) continue;

                // 检查是否已处理过该组
                if (processedGroups.Contains(portalAsset.portalGroupId)) continue;
                processedGroups.Add(portalAsset.portalGroupId);

                // 通过portalGroupId查找同组的所有Portal（而不是只通过linkedPortalId）
                List<IEditorNodeView> entryPortals = new List<IEditorNodeView>();
                List<IEditorNodeView> exitPortals = new List<IEditorNodeView>();

                foreach (var kvp in graphView.graphElementCache.nodeViewById)
                {
                    if (kvp.Value.asset is PortalNodeAsset otherPortal && otherPortal.portalGroupId == portalAsset.portalGroupId)
                    {
                        if (otherPortal.direction == PortalDirection.Entry)
                        {
                            entryPortals.Add(kvp.Value);
                        }
                        else
                        {
                            exitPortals.Add(kvp.Value);
                        }
                    }
                }

                // 收集原始连接信息
                foreach (IEditorNodeView entryPortalView in entryPortals)
                {
                    IEditorPortView entryPort = entryPortalView.GetPortView("portal_port");
                    if (entryPort == null) continue;

                    // Entry Portal的输入边 - 获取连接到它的输出端口
                    foreach (IEditorEdgeView edge in entryPort.edges)
                    {
                        IEditorPortView sourceOutputPort = edge.outputPortView;
                        if (sourceOutputPort == null) continue;

                        // 遍历所有Exit Portal，获取它们连接到的输入端口
                        foreach (IEditorNodeView exitPortalView in exitPortals)
                        {
                            IEditorPortView exitPort = exitPortalView.GetPortView("portal_port");
                            if (exitPort == null) continue;

                            // Exit Portal的输出边 - 获取连接到它的输入端口
                            foreach (IEditorEdgeView exitEdge in exitPort.edges)
                            {
                                IEditorPortView targetInputPort = exitEdge.inputPortView;
                                if (targetInputPort != null)
                                {
                                    connectionsToCreate.Add((sourceOutputPort, targetInputPort));
                                }
                            }
                        }
                    }

                    if (!portalsToDelete.Contains(entryPortalView))
                    {
                        portalsToDelete.Add(entryPortalView);
                    }
                }

                // 添加所有Exit Portal到删除列表
                foreach (IEditorNodeView exitPortalView in exitPortals)
                {
                    if (!portalsToDelete.Contains(exitPortalView))
                    {
                        portalsToDelete.Add(exitPortalView);
                    }
                }
            }

            // 选中要删除的Portal节点，然后走默认删除逻辑
            graphView.ClearSelection();
            foreach (IEditorNodeView portalView in portalsToDelete)
            {
                graphView.AddToSelection(portalView.element);
            }
            graphView.graphOperate.Delete();

            // 创建新的直接连接
            foreach (var connection in connectionsToCreate)
            {
                // 检查连接是否有效
                if (graphView.connectSystem.CanConnect(connection.toPort, connection.fromPort))
                {
                    graphView.connectSystem.Connect(connection.toPort, connection.fromPort);
                }
            }

            // 保存更改
            graphView.graphSave.SetDirty();
        }
    }
}
