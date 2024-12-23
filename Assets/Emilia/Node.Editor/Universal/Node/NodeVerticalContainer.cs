using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    public class NodeVerticalContainer : VisualElement
    {
        public NodeVerticalContainer()
        {
            style.flexDirection = FlexDirection.Row;
            style.flexWrap = Wrap.Wrap;
            style.justifyContent = Justify.SpaceBetween;

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            foreach (var child in Children())
            {
                child.style.flexGrow = 1;
            }
        }
    }
}