using System;
using Emilia.Kit.Editor;
using Emilia.Node.Attributes;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    public class EditorGraphWindow : OdinProEditorWindow, IEditorGraphWindow
    {
        [NonSerialized, OdinSerialize]
        private EditorGraphRoot _graphRoot;

        public EditorGraphAsset graphAsset => this._graphRoot?.asset;

        private void OnEnable()
        {
            IMGUIContainer container = new IMGUIContainer(Draw);
            container.style.flexGrow = 1;
            rootVisualElement.Add(container);
        }

        private void OnInspectorUpdate()
        {
            UpdateTitle();
        }

        private void Draw()
        {
            this._graphRoot?.OnImGUI(position.height);
        }

        private void UpdateTitle()
        {
            WindowSettingsAttribute settings = graphAsset.GetType().GetAttribute<WindowSettingsAttribute>();
            if (settings == null) return;

            if (string.IsNullOrEmpty(settings.titleExpression))
            {
                titleContent = new GUIContent(settings.title);
                return;
            }

            ValueResolver<string> valueResolver = ValueResolver.Get<string>(graphAsset.propertyTree.RootProperty, settings.titleExpression);
            if (valueResolver.HasError)
            {
                Debug.LogError($"UpdateTitle Error: {valueResolver.ErrorMessage}");
                return;
            }

            string getTitle = valueResolver.GetValue();
            titleContent = new GUIContent(getTitle);
        }

        public void SetGraphAsset(EditorGraphAsset graphAsset)
        {
            if (this._graphRoot == null)
            {
                this._graphRoot = new EditorGraphRoot();
                this._graphRoot.Initialize(this);
            }

            this._graphRoot.SetAsset(graphAsset);
        }

        private void OnDisable()
        {
            this._graphRoot?.Dispose();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            this._graphRoot = default;
        }
    }
}