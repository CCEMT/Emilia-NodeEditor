using System.Collections.Generic;

namespace Emilia.Node.Editor
{
    public static class IEditorPortViewExtension
    {
        /// <summary>
        /// 获取端口的所有连接
        /// </summary>
        public static List<IEditorEdgeView> GetEdges(this IEditorPortView editorPortView)
        {
            List<IEditorEdgeView> edgeViews = new List<IEditorEdgeView>();

            int amount = editorPortView.master.graphView.edgeViews.Count;
            for (int i = 0; i < amount; i++)
            {
                IEditorEdgeView edgeView = editorPortView.master.graphView.edgeViews[i];

                if (edgeView.asset.inputNodeId == editorPortView.master.asset.id && edgeView.asset.inputPortId == editorPortView.info.id)
                {
                    edgeViews.Add(edgeView);
                    continue;
                }

                if (edgeView.asset.outputNodeId == editorPortView.master.asset.id && edgeView.asset.outputPortId == editorPortView.info.id)
                {
                    edgeViews.Add(edgeView);
                    continue;
                }
            }

            return edgeViews;
        }
    }
}