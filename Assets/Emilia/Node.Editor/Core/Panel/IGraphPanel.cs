using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    public interface IGraphPanel
    {
        string id { get; set; }
        GraphPanelCapabilities panelCapabilities { get; }
        GraphTwoPaneSplitView parentView { get; set; }
        VisualElement rootView { get; }

        void Initialize(EditorGraphView graphView);

        void Dispose();
    }
}