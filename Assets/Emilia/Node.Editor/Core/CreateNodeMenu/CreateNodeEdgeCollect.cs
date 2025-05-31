using System.Collections.Generic;

namespace Emilia.Node.Editor
{
    public class CreateNodeEdgeCollect : ICreateNodeCollect
    {
        private EditorGraphView graphView;
        private IEditorEdgeView edgeView;
        private IEditorPortView portView;

        public CreateNodeEdgeCollect(EditorGraphView graphView, IEditorEdgeView edgeView, IEditorPortView portView)
        {
            this.graphView = graphView;
            this.edgeView = edgeView;
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
                CreateNodeInfo createNodeInfo = new CreateNodeInfo(nodeInfo);
                createNodeInfo.createNodeConnector = new CreateNodeConnector();

                if (portView.info.canMultiConnect == false && portView.edges.Count > 0) createNodeInfo.createNodeConnector.edgeId = portView.edges[0].asset.id;
                createNodeInfo.createNodeConnector.originalNodeId = portView.master.asset.id;
                createNodeInfo.createNodeConnector.originalPortId = portView.info.id;
                createNodeInfo.createNodeConnector.targetPortId = portInfo.portId;

                createNodeInfos.Add(createNodeInfo);
            }

            return createNodeInfos;
        }
    }
}