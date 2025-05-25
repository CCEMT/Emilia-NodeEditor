using System;
using Emilia.Kit;
using Emilia.Kit.Editor;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor
{
    /// <summary>
    /// Node拷贝粘贴Pack
    /// </summary>
    [Serializable]
    public class NodeCopyPastePack : INodeCopyPastePack
    {
        [OdinSerialize, NonSerialized]
        private UnityAssetSerializationPack assetPack;

        private EditorNodeAsset _copyAsset;
        private EditorNodeAsset _pasteAsset;

        public EditorNodeAsset copyAsset
        {
            get
            {
                if (this._copyAsset == null) this._copyAsset = UnityAssetSerializationUtility.DeserializeUnityAsset<EditorNodeAsset>(this.assetPack);
                return this._copyAsset;
            }
        }

        public EditorNodeAsset pasteAsset => _pasteAsset;

        public NodeCopyPastePack(EditorNodeAsset nodeAsset)
        {
            assetPack = UnityAssetSerializationUtility.SerializeUnityAsset(nodeAsset);
        }

        public virtual bool CanDependency(ICopyPastePack pack) => false;

        public void Paste(CopyPasteContext copyPasteContext)
        {
            GraphCopyPasteContext graphCopyPasteContext = (GraphCopyPasteContext) copyPasteContext.userData;
            EditorGraphView graphView = graphCopyPasteContext.graphView;

            if (graphView == null) return;

            EditorNodeAsset copy = copyAsset;
            if (copy == null) return;

            _pasteAsset = Object.Instantiate(copy);
            _pasteAsset.name = copy.name;
            _pasteAsset.id = Guid.NewGuid().ToString();

            Rect rect = _pasteAsset.position;
            rect.position += new Vector2(20, 20);
            if (graphCopyPasteContext.createPosition != null) rect.position = graphCopyPasteContext.createPosition.Value;

            _pasteAsset.position = rect;

            this._pasteAsset.PasteChild();

            graphView.RegisterCompleteObjectUndo("Graph Paste");
            IEditorNodeView nodeView = graphView.AddNode(this._pasteAsset);

            copyPasteContext.pasteContent.Add(nodeView);

            Undo.RegisterCreatedObjectUndo(this._pasteAsset, "Graph Pause");
        }
    }
}