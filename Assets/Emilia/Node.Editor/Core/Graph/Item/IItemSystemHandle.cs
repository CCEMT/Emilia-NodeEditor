namespace Emilia.Node.Editor
{
    public interface IItemSystemHandle : IEditorHandle
    {
        void OnCreateItem(IEditorItemView itemView);
    }
}