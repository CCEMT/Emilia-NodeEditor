using System.Collections.Generic;
using System.Linq;

namespace Emilia.Node.Editor
{
    public class ConnectCreateNodePostprocess : ICreateNodePostprocess
    {
        public string originalNodeId;
        public string originalPortId;
        public string targetPortId;

        public ConnectCreateNodePostprocess(string originalNodeId, string originalPortId, string targetPortId = null)
        {
            this.originalNodeId = originalNodeId;
            this.originalPortId = originalPortId;
            this.targetPortId = targetPortId;
        }

        public void Postprocess(EditorGraphView graphView, IEditorNodeView nodeView, CreateNodeContext createNodeContext)
        {
            IEditorNodeView originalNodeView = graphView.graphElementCache.GetEditorNodeView(originalNodeId);
            IEditorPortView originalPortView = originalNodeView.GetPortView(originalPortId);
            IEditorPortView targetPortView;

            if (string.IsNullOrEmpty(targetPortId) == false) targetPortView = nodeView.GetPortView(targetPortId);
            else
            {
                List<IEditorPortView> canConnectList = nodeView.GetCanConnectPort(originalPortView);
                targetPortView = canConnectList.FirstOrDefault();
            }

            if (targetPortView == null) return;

            if (originalPortView.portDirection == EditorPortDirection.Input) graphView.connectSystem.Connect(originalPortView, targetPortView);
            else graphView.connectSystem.Connect(targetPortView, originalPortView);
        }
    }
}