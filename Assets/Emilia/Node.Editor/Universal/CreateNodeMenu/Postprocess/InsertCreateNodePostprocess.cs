using System;
using System.Collections.Generic;
using System.Linq;
using Emilia.Kit.Editor;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor
{
    public class InsertCreateNodePostprocess : ICreateNodePostprocess
    {
        public string insertEdgeId;

        public string inputPortId;
        public string outputPortId;

        public InsertCreateNodePostprocess(string insertEdgeId, string inputPortId = null, string outputPortId = null)
        {
            this.insertEdgeId = insertEdgeId;
            this.inputPortId = inputPortId;
            this.outputPortId = outputPortId;
        }

        public void Postprocess(EditorGraphView graphView, IEditorNodeView nodeView, CreateNodeContext createNodeContext)
        {
            IEditorEdgeView edgeView = graphView.graphElementCache.GetEditorEdgeView(insertEdgeId);

            IEditorPortView inputPortView = null;
            if (string.IsNullOrEmpty(inputPortId) == false) inputPortView = nodeView.GetPortView(inputPortId);

            IEditorPortView outputPortView = null;
            if (string.IsNullOrEmpty(this.outputPortId) == false) outputPortView = nodeView.GetPortView(outputPortId);

            if (inputPortView == null || outputPortView == null)
            {
                if (nodeView.GetCanConnectPort(edgeView, out List<IEditorPortView> canConnectInput, out List<IEditorPortView> canConnectOutput))
                {
                    if (inputPortView == null) inputPortView = canConnectInput.FirstOrDefault();
                    if (outputPortView == null) outputPortView = canConnectOutput.FirstOrDefault();
                }
            }

            if (inputPortView == null || outputPortView == null) return;

            IEdgeCopyPastePack copyPastePack = edgeView.GetPack() as IEdgeCopyPastePack;

            EditorEdgeAsset inputPasteAsset = Object.Instantiate(copyPastePack.copyAsset);
            EditorEdgeAsset outputPasteAsset = Object.Instantiate(copyPastePack.copyAsset);

            inputPasteAsset.name = copyPastePack.copyAsset.name;
            inputPasteAsset.id = Guid.NewGuid().ToString();

            outputPasteAsset.name = copyPastePack.copyAsset.name;
            outputPasteAsset.id = Guid.NewGuid().ToString();

            inputPasteAsset.PasteChild();
            outputPasteAsset.PasteChild();

            inputPasteAsset.inputNodeId = inputPortView.master.asset.id;
            inputPasteAsset.inputPortId = inputPortView.info.id;

            outputPasteAsset.outputNodeId = outputPortView.master.asset.id;
            outputPasteAsset.outputPortId = outputPortView.info.id;

            nodeView.graphView.RegisterCompleteObjectUndo("Graph Insert");
            nodeView.graphView.AddEdge(inputPasteAsset);
            nodeView.graphView.AddEdge(outputPasteAsset);

            Undo.RegisterCreatedObjectUndo(inputPasteAsset, "Graph Insert");
            Undo.RegisterCreatedObjectUndo(outputPasteAsset, "Graph Insert");
            edgeView.Delete();
        }
    }
}