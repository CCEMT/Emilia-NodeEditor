using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    public interface IGraphPanel
    {
        GraphPanelCapabilities panelCapabilities { get; }
        GraphTwoPaneSplitView parentView { get; set; }
        VisualElement rootView { get; }

        void Initialize(EditorGraphView graphView);

        void Dispose();
    }
}