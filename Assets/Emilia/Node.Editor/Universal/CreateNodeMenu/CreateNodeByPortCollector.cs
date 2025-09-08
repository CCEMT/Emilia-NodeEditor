using System.Collections.Generic;

namespace Emilia.Node.Editor
{
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
            List<CreateNodeInfo> createNodeInfos = new List<CreateNodeInfo>();

            if (portView == null) return createNodeInfos;

            List<PortInfo> portInfos = graphView.graphElementCache.GetPortInfoTypeByPort(portView);

            int portInfoAmount = portInfos.Count;
            for (int i = 0; i < portInfoAmount; i++)
            {
                PortInfo portInfo = portInfos[i];

                int amount = allNodeInfos.Count;
                for (int j = 0; j < amount; j++)
                {
                    MenuNodeInfo nodeInfo = allNodeInfos[j];

                    if (nodeInfo.nodeData == null)
                    {
                        if (nodeInfo.editorNodeAssetType != portInfo.nodeAssetType) continue;
                        AddCreateNodeInfo(portInfo, nodeInfo);
                    }
                    else
                    {
                        if (nodeInfo.nodeData.GetType() != portInfo.nodeData.GetType()) continue;
                        AddCreateNodeInfo(portInfo, nodeInfo);
                    }
                }
            }

            void AddCreateNodeInfo(PortInfo portInfo, MenuNodeInfo nodeInfo)
            {
                if (string.IsNullOrEmpty(portInfo.displayName) == false) nodeInfo.path += $"：{portInfo.displayName}";

                string originalNodeId = portView.master.asset.id;
                string originalPortId = portView.info.id;
                string targetPortId = portInfo.portId;

                ICreateNodePostprocess createNodePostprocess;

                if (portView.info.canMultiConnect == false && portView.edges.Count > 0)
                {
                    string edgeId = portView.edges[0].asset.id;
                    createNodePostprocess = new RedirectionEdgeCreateNodePostprocess(originalNodeId, targetPortId, edgeId);
                }
                else
                {
                    createNodePostprocess = new ConnectCreateNodePostprocess(originalNodeId, originalPortId, targetPortId);
                }

                CreateNodeInfo createNodeInfo = new CreateNodeInfo(nodeInfo, createNodePostprocess);

                createNodeInfos.Add(createNodeInfo);
            }

            return createNodeInfos;
        }
    }
}