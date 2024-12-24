using System;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace Emilia.Node.Editor
{
    [Serializable]
    public class EditorGraphRoot
    {
        [SerializeField]
        private EditorWindow _window;

        [NonSerialized, OdinSerialize]
        private EditorGraphAsset _asset;

        private EditorGraphViewDrawer _drawer;

        public EditorWindow window => this._window;
        public EditorGraphAsset asset => this._asset;

        public EditorGraphView graphView { get; private set; }

        public void Initialize(EditorWindow window)
        {
            this._window = window;
        }

        public void SetAsset(EditorGraphAsset asset)
        {
            this._asset = asset;
            Reload();
        }

        public void OnImGUI(float height, float width = -1)
        {
            if (this._asset != null && this._drawer == null)
            {
                graphView = new EditorGraphView();
                graphView.window = window;
                graphView.Initialize();
                graphView.Reload(asset);


                this._drawer = new EditorGraphViewDrawer();
                this._drawer.Initialize(graphView);
                
                EditorApplication.update -= Update;
                EditorApplication.update += Update;
            }

            this._drawer?.Draw(height, width);
            graphView?.OnFocus();
        }

        public void Update()
        {
            graphView?.OnUpdate();
        }

        public void Reload()
        {
            graphView?.Dispose();
            graphView = null;

            this._drawer?.Dispose();
            this._drawer = null;
        }

        public void Dispose()
        {
            graphView?.Dispose();
            graphView = null;

            this._drawer?.Dispose();
            this._drawer = null;

            EditorApplication.update -= Update;
        }
    }
}