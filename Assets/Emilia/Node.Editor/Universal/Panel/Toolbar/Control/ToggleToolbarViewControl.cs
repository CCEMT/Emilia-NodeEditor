using System;
using UnityEditor;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    public class ToggleToolbarViewControl : ToolbarViewControl
    {
        private GUIContent content;

        private Func<bool> getter;
        private Action<bool> setter;

        public ToggleToolbarViewControl(string displayName, Func<bool> getter, Action<bool> setter)
        {
            this.content = new GUIContent(displayName);
            this.getter = getter;
            this.setter = setter;
        }

        public ToggleToolbarViewControl(GUIContent guiContent, Func<bool> getter, Action<bool> setter)
        {
            this.content = guiContent;
            this.getter = getter;
            this.setter = setter;
        }

        public override void OnDraw()
        {
            bool value = this.getter?.Invoke() ?? false;
            value = GUILayout.Toggle(getter.Invoke(), content, EditorStyles.toolbarButton);
            setter?.Invoke(value);
        }
    }
}