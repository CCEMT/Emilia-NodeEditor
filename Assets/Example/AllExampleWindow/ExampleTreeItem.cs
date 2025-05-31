using System;
using Emilia.Kit.Editor;
using Emilia.Node.Editor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Example
{
    [Serializable]
    public class ExampleTreeItem
    {
        public ExampleAsset asset;

        public ExampleTreeItem(ExampleAsset asset)
        {
            this.asset = asset;
        }
    }

    public class ExampleTreeItemDrawer : OdinValueDrawer<ExampleTreeItem>, IDisposable
    {
        private EditorGraphRoot _graphRoot;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            ExampleTreeItem item = this.ValueEntry.SmartValue;

            if (this._graphRoot == null) this._graphRoot = new EditorGraphRoot();

            if (this._graphRoot.window == null)
            {
                EditorWindow window = EditorImGUIKit.GetImGUIWindow();
                this._graphRoot.Initialize(window);
            }

            if (this._graphRoot.asset == null)
            {
                ExampleAsset asset = item.asset;
                if (asset != null) this._graphRoot.SetAsset(asset);
                return;
            }

            if (_graphRoot == null || _graphRoot.window == null) return;

            Rect rect = GUIHelper.GetCurrentLayoutRect();
            float height = this._graphRoot.window.position.height - 35;
            this._graphRoot.OnImGUI(height, rect.width);
        }

        public void Dispose()
        {
            if (_graphRoot != null)
            {
                if (_graphRoot.asset != null) this._graphRoot.asset.SaveAll();
                this._graphRoot.Dispose();
            }

            this._graphRoot = null;
        }
    }
}