using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    public class FlexibleSpaceToolbarViewControl : ToolbarViewControl
    {
        public override void OnDraw()
        {
            GUILayout.FlexibleSpace();
        }
    }
}