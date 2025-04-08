using System;
using Emilia.Kit;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor
{
    [Serializable]
    public class NodeCopyPastePack : INodeCopyPastePack
    {
        [OdinSerialize, NonSerialized]
        private EditorNodeAsset _copyAsset;

        private EditorNodeAsset _pasteAsset;

        public EditorNodeAsset copyAsset => this._copyAsset;
        public EditorNodeAsset pasteAsset => _pasteAsset;

        public NodeCopyPastePack(EditorNodeAsset nodeAsset)
        {
            this._copyAsset = nodeAsset;
        }

        public virtual bool CanDependency(ICopyPastePack pack)
        {
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

            Rect rect = _pasteAsset.position;
            rect.position += new Vector2(20, 20);
            if (graphCopyPasteContext.createPosition != null) rect.position = graphCopyPasteContext.createPosition.Value;
            
            _pasteAsset.position = rect;

            GraphCopyPasteUtility.PasteChild(this._pasteAsset);

            graphView.RegisterCompleteObjectUndo("Graph Paste");
            graphView.AddNode(this._pasteAsset);

            Undo.RegisterCreatedObjectUndo(this._pasteAsset, "Graph Pause");
        }
    }
}