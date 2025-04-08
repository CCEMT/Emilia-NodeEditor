using System;
using Emilia.Kit;
using Sirenix.Serialization;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor
{
    [Serializable]
    public class ItemCopyPastePack : IItemCopyPastePack
    {
        [OdinSerialize, NonSerialized]
        protected EditorItemAsset _copyAsset;

        protected EditorItemAsset _pasteAsset;

        public EditorItemAsset copyAsset => this._copyAsset;
        public EditorItemAsset pasteAsset => this._pasteAsset;

        public ItemCopyPastePack(EditorItemAsset asset)
        {
            this._copyAsset = asset;
        }

        public virtual bool CanDependency(ICopyPastePack pack)
        {
            return false;
        }

        public virtual void Paste(CopyPasteContext copyPasteContext)
        {
            GraphCopyPasteContext graphCopyPasteContext = (GraphCopyPasteContext) copyPasteContext.userData;
            EditorGraphView graphView = graphCopyPasteContext.graphView;

            if (graphView == null) return;
            if (this._copyAsset == null) return;

            _pasteAsset = Object.Instantiate(_copyAsset);
            this._pasteAsset.id = Guid.NewGuid().ToString();

            GraphCopyPasteUtility.PasteChild(this._pasteAsset);
            PasteDependency(copyPasteContext);

            graphView.RegisterCompleteObjectUndo("Graph Paste");
            graphView.AddItem(_pasteAsset);

            Undo.RegisterCreatedObjectUndo(this._pasteAsset, "Graph Pause");
        }

        protected virtual void PasteDependency(CopyPasteContext copyPasteContext) { }
    }
}