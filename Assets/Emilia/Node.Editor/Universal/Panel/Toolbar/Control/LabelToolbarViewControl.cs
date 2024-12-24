using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    public class LabelToolbarViewControl : ToolbarViewControl
    {
        public GUIContent content;

        public LabelToolbarViewControl(string text)
        {
            this.content = new GUIContent(text);
        }

        public LabelToolbarViewControl(GUIContent guiContent)
        {
            this.content = guiContent;
        }

        public override void OnDraw()
        {
            GUILayout.Label(content);
        }
    }
}