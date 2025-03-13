using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    public class GraphPanelContainer : VisualElement
    {
        public GraphPanelContainer()
        {
            pickingMode = PickingMode.Ignore;
            style.flexGrow = 1;
            style.position = Position.Absolute;

            this.StretchToParentSize();
        }
    }
}