using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    public class GraphCompilationContainer : VisualElement
    {
        public GraphCompilationContainer()
        {
            name = nameof(GraphLoadingContainer);
            pickingMode = PickingMode.Ignore;
            style.display = DisplayStyle.Flex;
            
            this.StretchToParentSize();

            Label label = new Label("Compilation...");
            label.style.position = Position.Absolute;
            
            label.style.left = 10;
            label.style.bottom = 10;

            Add(label);
        }
    }
}