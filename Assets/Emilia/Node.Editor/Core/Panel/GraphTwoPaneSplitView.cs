using Emilia.Reflection.Editor;
using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    public class GraphTwoPaneSplitView : TwoPaneSplitView_Internals
    {
        public GraphTwoPaneSplitView(int fixedPaneIndex, float fixedPaneStartDimension, TwoPaneSplitViewOrientation orientation) : base(fixedPaneIndex, fixedPaneStartDimension, orientation) { }

        public bool canResizable
        {
            get => dragLineAnchor_Internal.style.width.value.value > 0f && dragLineAnchor_Internal.style.height.value.value > 0f;

            set
            {
                if (orientation == TwoPaneSplitViewOrientation.Horizontal)
                {
                    dragLineAnchor_Internal.style.width = value ? new StyleLength(1f) : new StyleLength(0f);
                    dragLineAnchor_Internal.style.height = value ? dragLineAnchor_Internal.parent.style.height : new StyleLength(0f);
                }
                else
                {
                    dragLineAnchor_Internal.style.height = value ? new StyleLength(1f) : new StyleLength(0f);
                    dragLineAnchor_Internal.style.width = value ? dragLineAnchor_Internal.parent.style.width : new StyleLength(0f);
                }
            }
        }
    }
}