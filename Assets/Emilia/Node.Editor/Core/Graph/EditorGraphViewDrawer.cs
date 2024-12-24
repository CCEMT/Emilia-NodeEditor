using Sirenix.OdinInspector.Editor.Internal.UIToolkitIntegration;
using UnityEngine;

namespace Emilia.Node.Editor
{
    public class EditorGraphViewDrawer
    {
        private EditorGraphView _graphView;
        public OdinImGuiElement guiElement;

        public void Initialize(EditorGraphView graphView)
        {
            _graphView = graphView;
            this.guiElement = new OdinImGuiElement(graphView);
        }

        public void Draw(float height, float width = -1)
        {
            if (guiElement == null) return;

            Rect rect = ImguiElementUtils.EmbedVisualElementAndDrawItHere(this.guiElement);

            float targetWidth = width <= 0 ? rect.width : width;
            if (targetWidth > 0) _graphView.style.width = targetWidth;

            float targetHeight = height;
            if (targetHeight > 0) _graphView.style.height = targetHeight;
        }

        public void Dispose()
        {
            guiElement = null;
        }
    }
}