using System;
using System.Linq;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

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

        private GUIStyle tipsStyle;

        /// <summary>
        /// 窗口
        /// </summary>
        public EditorWindow window => this._window;

        /// <summary>
        /// 资产
        /// </summary>
        public EditorGraphAsset asset => this._asset;

        public EditorGraphView graphView { get; private set; }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(EditorWindow window)
        {
            this._window = window;
        }

        /// <summary>
        /// 设置资源
        /// </summary>
        public void SetAsset(EditorGraphAsset asset)
        {
            this._asset = asset;
            Reload();
        }

        /// <summary>
        /// ImGUI绘制
        /// </summary>
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

            if (this._drawer != null && asset != null) this._drawer.Draw(height, width);
            else
            {
                InitTipsStyle();
                GUILayout.Label("当前编辑的对象为空", tipsStyle, GUILayout.Height(height));
            }
        }

        private void InitTipsStyle()
        {
            if (this.tipsStyle != null) return;
            tipsStyle = new GUIStyle(GUI.skin.label);
            tipsStyle.alignment = TextAnchor.MiddleCenter;
            tipsStyle.fontSize = 20;
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void Update()
        {
            if (this._asset == null) Reload();
            graphView?.OnUpdate();
        }

        /// <summary>
        /// 重新加载
        /// </summary>
        public void Reload()
        {
            graphView?.Dispose();
            graphView = null;

            this._drawer?.Dispose();
            this._drawer = null;
        }

        /// <summary>
        /// 释放
        /// </summary>
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