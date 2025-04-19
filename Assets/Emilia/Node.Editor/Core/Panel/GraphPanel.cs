using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    public class GraphPanel : GraphElement, IGraphPanel
    {
        protected GraphPanelCapabilities _panelCapabilities;

        public virtual string id { get; set; }

        public virtual GraphPanelCapabilities panelCapabilities
        {
            get => _panelCapabilities;
            set => _panelCapabilities = value;
        }

        public GraphTwoPaneSplitView parentView { get; set; }
        public VisualElement rootView => this;
        protected EditorGraphView graphView { get; private set; }

        public virtual void Initialize(EditorGraphView graphView)
        {
            this.graphView = graphView;
        }

        public virtual void Dispose() { }
    }
}