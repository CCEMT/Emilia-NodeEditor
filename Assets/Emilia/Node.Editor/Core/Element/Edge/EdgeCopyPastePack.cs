using System;
using Emilia.Kit;
using Sirenix.Serialization;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor
{
    [Serializable]
    public class EdgeCopyPastePack : IEdgeCopyPastePack
    {
        [OdinSerialize, NonSerialized]
        private EditorEdgeAsset _copyAsset;

        private EditorEdgeAsset _pasteAsset;

        public EditorEdgeAsset copyAsset => this._copyAsset;
        public EditorEdgeAsset pasteAsset => this._pasteAsset;

        public EdgeCopyPastePack(EditorEdgeAsset edgeAsset)
        {
            this._copyAsset = edgeAsset;
        }

        public virtual bool CanDependency(ICopyPastePack pack)
        {
            INodeCopyPastePack nodeCopyPastePack = pack as INodeCopyPastePack;
            if (nodeCopyPastePack == null) return false;
            bool isInput = this._copyAsset.inputNodeId == nodeCopyPastePack.copyAsset.id;
            bool isOutput = this._copyAsset.outputNodeId == nodeCopyPastePack.copyAsset.id;
            if (isInput || isOutput) return true;
            return false;
        }

        public void Paste(CopyPasteContext copyPasteContext)
        {
            GraphCopyPasteContext graphCopyPasteContext = (GraphCopyPasteContext) copyPasteContext.userData;
            EditorGraphView graphView = graphCopyPasteContext.graphView;

            if (graphView == null) return;
            if (this._copyAsset == null) return;

            _pasteAsset = Object.Instantiate(_copyAsset);
            _pasteAsset.name = this._copyAsset.name;
            _pasteAsset.id = Guid.NewGuid().ToString();

            GraphCopyPasteUtility.PasteChild(this._pasteAsset);

            int amount = copyPasteContext.dependency.Count;
            for (int i = 0; i < amount; i++)
            {
                ICopyPastePack pack = copyPasteContext.dependency[i];
                INodeCopyPastePack nodeCopyPastePack = pack as INodeCopyPastePack;
                if (nodeCopyPastePack == null) continue;

                bool isInput = this._copyAsset.inputNodeId == nodeCopyPastePack.copyAsset.id;
                if (isInput)
                {
                    _pasteAsset.inputNodeId = nodeCopyPastePack.pasteAsset.id;
                    continue;
                }

                bool isOutput = this._copyAsset.outputNodeId == nodeCopyPastePack.copyAsset.id;
                if (isOutput)
                {
                    _pasteAsset.outputNodeId = nodeCopyPastePack.pasteAsset.id;
                    continue;
                }
            }

            graphView.RegisterCompleteObjectUndo("Graph Paste");
            graphView.AddEdge(_pasteAsset);
            Undo.RegisterCreatedObjectUndo(this._pasteAsset, "Graph Pause");
        }
    }
}