using Emilia.Kit;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public abstract class NodeSystemHandle
    {
        public virtual void OnCreateNode(EditorGraphView graphView,IEditorNodeView editorNodeView) { }
    }
}