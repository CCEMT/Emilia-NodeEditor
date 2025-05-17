using Emilia.Kit;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public abstract class ItemSystemHandle
    {
        public virtual void OnCreateItem(EditorGraphView graphView, IEditorItemView itemView) { }
    }
}