using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor
{
    public class ItemSystem : GraphViewModule
    {
        private IItemSystemHandle handle;

        public override void Reset(EditorGraphView graphView)
        {
            base.Reset(graphView);

            if (handle != null) EditorHandleUtility.ReleaseHandle(handle);
            handle = EditorHandleUtility.BuildHandle<IItemSystemHandle>(graphView.graphAsset.GetType(), graphView);
        }

        /// <summary>
        /// 创建Item
        /// </summary>
        public IEditorItemView CreateItem(Type type, Vector2 position)
        {
            if (typeof(EditorItemAsset).IsAssignableFrom(type) == false) return null;

            EditorItemAsset itemAsset = ScriptableObject.CreateInstance(type) as EditorItemAsset;
            itemAsset.id = Guid.NewGuid().ToString();
            itemAsset.position = new Rect(position, new Vector2(100, 100));

            Undo.IncrementCurrentGroup();
            Undo.RegisterCreatedObjectUndo(itemAsset, "Graph CreateItem");

            graphView.RegisterCompleteObjectUndo("Graph CreateItem");
            IEditorItemView editorItemView = this.graphView.AddItem(itemAsset);
            handle?.OnCreateItem(editorItemView);

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            Undo.IncrementCurrentGroup();

            return editorItemView;
        }

        /// <summary>
        /// 删除Item
        /// </summary>
        public void DeleteItem(IEditorItemView itemView)
        {
            itemView.RemoveView();

            if (itemView.asset == null) return;
            graphView.RegisterCompleteObjectUndo("Graph RemoveItem");
            graphView.graphAsset.RemoveItem(itemView.asset);

            List<Object> assets = new List<Object>();
            itemView.asset.CollectAsset(assets);

            int amount = assets.Count;
            for (int i = 0; i < amount; i++)
            {
                Object asset = assets[i];
                if (asset == null) continue;
                Undo.DestroyObjectImmediate(asset);
            }
        }

        /// <summary>
        /// 删除Item，不记录Undo
        /// </summary>
        public void DeleteItemNoUndo(IEditorItemView itemView)
        {
            itemView.RemoveView();
            graphView.graphAsset.RemoveItem(itemView.asset);

            List<Object> assets = new List<Object>();
            itemView.asset.CollectAsset(assets);

            int amount = assets.Count;
            for (int i = 0; i < amount; i++)
            {
                Object asset = assets[i];
                if (asset == null) continue;
                Object.DestroyImmediate(asset, true);
            }
        }

        public override void Dispose()
        {
            if (this.handle != null)
            {
                EditorHandleUtility.ReleaseHandle(handle);
                this.handle = null;
            }

            base.Dispose();
        }
    }
}