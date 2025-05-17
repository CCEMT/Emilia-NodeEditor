using Emilia.Kit;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public class GraphSaveHandle
    {
        public virtual void OnSaveBefore(EditorGraphView graphView) { }
        public virtual void OnSaveAfter(EditorGraphView graphView) { }
    }
}