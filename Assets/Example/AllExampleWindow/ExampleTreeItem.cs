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
        private EditorGraphImGUIRoot _graphImGUIRoot;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            ExampleTreeItem item = this.ValueEntry.SmartValue;

            if (this._graphImGUIRoot == null) this._graphImGUIRoot = new EditorGraphImGUIRoot();

            if (this._graphImGUIRoot.window == null)
            {
                EditorWindow window = EditorImGUIKit.GetImGUIWindow();
                this._graphImGUIRoot.Initialize(window);
            }

            if (this._graphImGUIRoot.asset == null)
            {
                ExampleAsset asset = item.asset;
                if (asset != null) this._graphImGUIRoot.SetAsset(asset);
                return;
            }

            if (this._graphImGUIRoot == null || this._graphImGUIRoot.window == null) return;

            Rect rect = GUIHelper.GetCurrentLayoutRect();
            float height = this._graphImGUIRoot.window.position.height - 35;
            this._graphImGUIRoot.OnImGUI(height, rect.width);
        }

        public void Dispose()
        {
            if (this._graphImGUIRoot != null)
            {
                if (this._graphImGUIRoot.asset != null) this._graphImGUIRoot.asset.SaveAll();
                this._graphImGUIRoot.Dispose();
            }

            this._graphImGUIRoot = null;
        }
    }
}