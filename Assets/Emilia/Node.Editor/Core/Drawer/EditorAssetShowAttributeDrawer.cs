using System;
using Emilia.Kit.Editor;
using Emilia.Node.Attributes;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Emilia.Node.Editor
{
    public class EditorAssetShowAttributeDrawer : OdinAttributeDrawer<EditorAssetShowAttribute>, IDisposable
    {
        private EditorGraphRoot _graphRoot;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            EditorAssetShowAttribute attribute = Attribute;

            if (this._graphRoot == null) this._graphRoot = new EditorGraphRoot();

            if (this._graphRoot.window == null)
            {
                EditorWindow window = EditorImGUIKit.GetImGUIWindow();
                this._graphRoot.Initialize(window);
            }

            if (this._graphRoot.asset == null)
            {
                CallNextDrawer(label);
                EditorGraphAsset asset = Property.ValueEntry.WeakSmartValue as EditorGraphAsset;
                if (asset != null) this._graphRoot.SetAsset(asset);
                return;
            }
            
            if (_graphRoot == null || _graphRoot.window == null) return;

            this._graphRoot.OnImGUI(attribute.height, attribute.width);
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