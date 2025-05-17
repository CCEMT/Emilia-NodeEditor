using Emilia.Kit;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public class GraphUndoHandle
    {
        public virtual void OnUndoBefore(EditorGraphView graphView, bool isSilent) { }
        public virtual void OnUndoAfter(EditorGraphView graphView, bool isSilent) { }
    }
}