using System.Collections.Generic;

namespace Emilia.Node.Editor
{
    /// <summary>
    /// 根据端口收集可创建的节点
    /// </summary>
    public class CreateNodeByPortCollector : ICreateNodeCollector
    {
        private EditorGraphView graphView;
        private IEditorPortView portView;

        public CreateNodeByPortCollector(EditorGraphView graphView, IEditorPortView portView)
        {
            this.graphView = graphView;
            this.portView = portView;
        }

        public List<CreateNodeInfo> Collect(List<MenuNodeInfo> allNodeInfos)
        {
            List<CreateNodeInfo> createNodeInfos = new();

            if (portView == null) return createNodeInfos;

            // 从图视图缓存中获取与当前端口兼容的所有端口信息
            List<PortInfo> portInfos = graphView.graphElementCache.GetPortInfoTypeByPort(portView);

            int portInfoAmount = portInfos.Count;
            for (int i = 0; i < portInfoAmount; i++)
            {
                PortInfo portInfo = portInfos[i];

                int amount = allNodeInfos.Count;
                for (int j = 0; j < amount; j++)
                {
                    MenuNodeInfo nodeInfo = allNodeInfos[j];

                    if (MatchNodeInfo(nodeInfo, portInfo) == false) continue;
                    AddCreateNodeInfo(portInfo, nodeInfo);
                }
            }

            void AddCreateNodeInfo(PortInfo portInfo, MenuNodeInfo nodeInfo)
            {
                if (string.IsNullOrEmpty(portInfo.displayName) == false) nodeInfo.path += $"：{portInfo.displayName}";

                string originalNodeId = portView.master.asset.id;
                string originalPortId = portView.info.id;
                string targetPortId = portInfo.portId;

                ICreateNodePostprocess createNodePostprocess;

                // 根据端口的连接能力决定创建节点后的处理方式
                if (portView.info.canMultiConnect == false && portView.edges.Count > 0)
                {
                    // 如果端口不支持多连接且已有连接，则需要重定向现有边
                    string edgeId = portView.edges[0].asset.id;
                    createNodePostprocess = new RedirectionEdgeCreateNodePostprocess(originalNodeId, targetPortId, edgeId);
                }
                else
                {
                    // 如果端口支持多连接或没有连接，直接创建新的连接
                    createNodePostprocess = new ConnectCreateNodePostprocess(originalNodeId, originalPortId, targetPortId);
                }

                CreateNodeInfo createNodeInfo = new(nodeInfo, createNodePostprocess);
                createNodeInfos.Add(createNodeInfo);
            }

            return createNodeInfos;
        }

        private static bool MatchNodeInfo(MenuNodeInfo nodeInfo, PortInfo portInfo)
        {
            // 没有关联数据的节点通过编辑器节点资产类型匹配。
            if (nodeInfo.nodeData == null) return nodeInfo.editorNodeAssetType == portInfo.nodeAssetType;

            // 有关联数据的节点必须匹配缓存中的数据类型。
            if (portInfo.nodeData == null) return false;
            return nodeInfo.nodeData.GetType() == portInfo.nodeData.GetType();
        }
    }
}
