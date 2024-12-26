using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    public class NodeHorizontalContainer : VisualElement
    {
        public VisualElement inputContainer { get; private set; }
        public VisualElement outputContainer { get; private set; }

        public NodeHorizontalContainer()
        {
            VisualElement divider = new VisualElement();
            Add(divider);

            VisualElement top = new VisualElement();
            top.name = "top";
            top.style.flexDirection = FlexDirection.Row;

            Add(top);

            inputContainer = new VisualElement();
            inputContainer.name = "inputContainer";
            top.Add(inputContainer);

            VisualElement spacer = new VisualElement();
            spacer.name = "spacer";
            spacer.style.flexGrow = 1;
            top.Add(spacer);

            outputContainer = new VisualElement();
            outputContainer.name = "outputContainer";
            top.Add(outputContainer);

        }
    }
}