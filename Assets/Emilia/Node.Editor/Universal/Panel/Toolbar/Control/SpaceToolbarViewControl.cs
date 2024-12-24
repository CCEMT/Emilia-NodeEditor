using UnityEditor;

namespace Emilia.Node.Universal.Editor
{
    public class SpaceToolbarViewControl : ToolbarViewControl
    {
        private float size;
        private bool leftSeparator;
        private bool rightSeparator;

        public SpaceToolbarViewControl(float size, bool leftSeparator = false, bool rightSeparator = false)
        {
            this.size = size;
            this.leftSeparator = leftSeparator;
            this.rightSeparator = rightSeparator;
        }

        public override void OnDraw()
        {
            if (leftSeparator) EditorGUILayout.Separator();
            EditorGUILayout.Space(size);
            if (rightSeparator) EditorGUILayout.Separator();
        }
    }
}