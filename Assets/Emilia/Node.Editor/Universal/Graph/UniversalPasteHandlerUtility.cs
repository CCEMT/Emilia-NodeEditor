using System.Linq;
using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    public static class UniversalPasteHandlerUtility
    {
        public static bool CanPasteSelected(EditorGraphView graphView)
        {
            if (graphView?.graphSelected?.selected == null) return false;

            return graphView.graphSelected.selected
                .OfType<IUniversalPasteHandler>()
                .Any(handler => handler.CanPaste());
        }

        public static bool TryPasteSelected(EditorGraphView graphView)
        {
            if (graphView?.graphSelected?.selected == null) return false;

            foreach (IUniversalPasteHandler handler in graphView.graphSelected.selected.OfType<IUniversalPasteHandler>())
            {
                if (handler.TryPaste()) return true;
            }

            return false;
        }
    }
}
