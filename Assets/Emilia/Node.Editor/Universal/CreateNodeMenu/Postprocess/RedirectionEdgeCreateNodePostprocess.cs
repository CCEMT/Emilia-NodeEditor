using System.Collections.Generic;

namespace Emilia.Node.Editor
{
    public class RedirectionEdgeCreateNodePostprocess : ICreateNodePostprocess
    {
        public string originalNodeId;
        public string targetPortId;
        public string edgeId;

        public RedirectionEdgeCreateNodePostprocess(string originalNodeId, string targetPortId, string edgeId)
        {
            this.originalNodeId = originalNodeId;
            this.targetPortId = targetPortId;
            this.edgeId = edgeId;
        }

        public void Postprocess(EditorGraphView graphView, IEditorNodeView nodeView, CreateNodeContext createNodeContext)
        {
            IEditorEdgeView edgeView = graphView.graphElementCache.edgeViewById.GetValueOrDefault(edgeId);
            if (edgeView == null) return;

            graphView.RegisterCompleteObjectUndo("Graph RedirectionEdge");

            EditorEdgeAsset edgeAsset = edgeView.asset;
            if (edgeAsset.inputNodeId == originalNodeId)
            {
                edgeAsset.outputNodeId = nodeView.asset.id;
                edgeAsset.outputPortId = targetPortId;
            }
            else
            {
                edgeAsset.inputNodeId = nodeView.asset.id;
                edgeAsset.inputPortId = targetPortId;
            }

            graphView.RemoveEdgeView(edgeView);
            graphView.AddEdgeView(edgeAsset);
        }
    }
}