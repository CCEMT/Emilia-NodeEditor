using System;
using Emilia.Kit.Editor;
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
            titleContent.text = graphAsset.name;

            WindowSettingsAttribute settings = graphAsset.GetType().GetAttribute<WindowSettingsAttribute>();
            if (settings != null)
            {
                if (string.IsNullOrEmpty(settings.titleExpression))
                {
                    titleContent.text = settings.title;
                    return;
                }

                ValueResolver<string> valueResolver = ValueResolver.Get<string>(graphAsset.propertyTree.RootProperty, settings.titleExpression);
                if (valueResolver.HasError)
                {
                    Debug.LogError($"UpdateTitle Error: {valueResolver.ErrorMessage}");
                    return;
                }

                string getTitle = valueResolver.GetValue();
                titleContent.text = getTitle;
            }

            if (_graphRoot?.graphView?.graphSave?.dirty ?? false) titleContent.text += "*";
        }

        /// <summary>
        /// 设置资源
        /// </summary>
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