using Emilia.Kit;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public abstract class GraphPanelHandle
    {
        public virtual void LoadPanel(EditorGraphView graphView, GraphPanelSystem system) { }
    }
}