namespace Emilia.Node.Editor
{
    public interface INodeSystemHandle : IEditorHandle
    {
        void OnCreateNode(IEditorNodeView editorNodeView);
    }
}