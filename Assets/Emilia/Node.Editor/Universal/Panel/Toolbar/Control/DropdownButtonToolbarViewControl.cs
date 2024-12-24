using System;
using Emilia.Kit;
using UnityEditor;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    public class DropdownButtonToolbarViewControl : ToolbarViewControl
    {
        private GUIContent content;
        private Func<OdinMenu> odinMenuFunc;

        public DropdownButtonToolbarViewControl(string displayName, Func<OdinMenu> odinMenuFunc)
        {
            this.content = new GUIContent(displayName);
            this.odinMenuFunc = odinMenuFunc;
        }

        public DropdownButtonToolbarViewControl(GUIContent guiContent, Func<OdinMenu> odinMenuFunc)
        {
            this.content = guiContent;
            this.odinMenuFunc = odinMenuFunc;
        }

        public override void OnDraw()
        {
            if (GUILayout.Button(content, EditorStyles.toolbarDropDown))
            {
                OdinMenu menu = this.odinMenuFunc.Invoke();
                menu?.ShowInPopup();
            }
        }
    }
}