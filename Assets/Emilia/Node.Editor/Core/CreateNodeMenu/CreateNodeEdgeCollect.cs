using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Emilia.Node.Editor
{
    public class CreateNodeEdgeCollect : ICreateNodeCollect
    {
        private EditorGraphView graphView;
        private IEditorEdgeView edgeView;

        public CreateNodeEdgeCollect(EditorGraphView graphView, IEditorEdgeView edgeView)
        {
            this.graphView = graphView;
            this.edgeView = edgeView;
        }

        public List<CreateNodeInfo> Collect(List<CreateNodeInfo> allNodeInfos)
        {
            List<CreateNodeInfo> createNodeInfos = new List<CreateNodeInfo>();

            Port port = this.edgeView.edgeElement.input ?? this.edgeView.edgeElement.output;
            IEditorPortView portView = port as IEditorPortView;
            if (portView == null) return createNodeInfos;

            List<PortInfo> portInfos = graphView.graphElementCache.GetPortInfoTypeByPort(portView);

            int portInfoAmount = portInfos.Count;
            for (int i = 0; i < portInfoAmount; i++)
            {
                PortInfo portInfo = portInfos[i];

                int amount = allNodeInfos.Count;
                for (int j = 0; j < amount; j++)
                {
                    CreateNodeInfo nodeInfo = allNodeInfos[j];

                    if (nodeInfo.nodeData == null)
                    {
                        if (nodeInfo.editorNodeAssetType != portInfo.nodeAssetType) continue;
                        nodeInfo.portId = portInfo.portId;

                        if (string.IsNullOrEmpty(portInfo.displayName) == false) nodeInfo.path += $"：{portInfo.displayName}";
                        createNodeInfos.Add(nodeInfo);
                    }
                    else
                    {
                        if (nodeInfo.nodeData.GetType() != portInfo.nodeData.GetType()) continue;
                        nodeInfo.portId = portInfo.portId;
                        if (string.IsNullOrEmpty(portInfo.displayName) == false) nodeInfo.path += $"：{portInfo.displayName}";
                        createNodeInfos.Add(nodeInfo);
                    }
                }
            }

            return createNodeInfos;
        }
    }
}