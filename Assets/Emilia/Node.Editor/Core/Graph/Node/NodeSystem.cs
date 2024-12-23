using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor
{
    public class NodeSystem
    {
        private EditorGraphView graphView;
        private INodeSystemHandle handle;

        public void Reset(EditorGraphView graphView)
        {
            this.graphView = graphView;

            if (handle != null) EditorHandleUtility.ReleaseHandle(handle);
            handle = EditorHandleUtility.BuildHandle<INodeSystemHandle>(graphView.graphAsset.GetType(), graphView);
        }

        public IEditorNodeView CreateNode(Type nodeType, Vector2 position, object userData)
        {
            EditorNodeAsset nodeAsset = CreateNode(nodeType, position);
            if (nodeAsset == null) return null;
            Undo.RegisterCreatedObjectUndo(nodeAsset, "Graph CreateNode");

            if (userData != null) nodeAsset.userData = graphView.graphCopyPaste.CreateCopy(userData);

            graphView.RegisterCompleteObjectUndo("Graph CreateNode");
            IEditorNodeView nodeView = graphView.AddNode(nodeAsset);
            handle?.OnCreateNode(nodeView);

            return nodeView;
        }

        public EditorNodeAsset CreateNode(Type nodeType, Vector2 position)
        {
            if (typeof(EditorNodeAsset).IsAssignableFrom(nodeType) == false) return null;

            EditorNodeAsset node = ScriptableObject.CreateInstance(nodeType) as EditorNodeAsset;
            node.id = Guid.NewGuid().ToString();
            node.position = new Rect(position, new Vector2(100, 100));

            return node;
        }

        public void DeleteNode(IEditorNodeView nodeView)
        {
            nodeView.RemoveView();

            graphView.RegisterCompleteObjectUndo("Graph RemoveNode");

            graphView.graphAsset.RemoveNode(nodeView.asset);

            List<Object> assets = new List<Object>();
            nodeView.asset.CollectAsset(assets);

            int amount = assets.Count;
            for (int i = 0; i < amount; i++)
            {
                Object asset = assets[i];
                if (asset == null) continue;
                Undo.DestroyObjectImmediate(asset);
            }
        }

        public void DeleteNodeNoUndo(IEditorNodeView nodeView)
        {
            nodeView.RemoveView();

            graphView.graphAsset.RemoveNode(nodeView.asset);

            List<Object> assets = new List<Object>();
            nodeView.asset.CollectAsset(assets);

            int amount = assets.Count;
            for (int i = 0; i < amount; i++)
            {
                Object asset = assets[i];
                if (asset == null) continue;
                Object.DestroyImmediate(asset, true);
            }
        }

        public void Dispose()
        {
            if (this.handle != null)
            {
                EditorHandleUtility.ReleaseHandle(handle);
                this.handle = null;
            }

            this.graphView = null;
        }
    }
}