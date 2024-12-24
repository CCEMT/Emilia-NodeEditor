using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    public class GraphPanelContainer : VisualElement
    {
        public GraphPanelContainer()
        {
            this.pickingMode = PickingMode.Ignore;
            this.style.flexGrow = 1;
            style.position = Position.Absolute;
            
            this.StretchToParentSize();
        }
    }
}