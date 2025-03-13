namespace Emilia.Node.Editor
{
    public interface IGraphUndoHandle : IEditorHandle
    {
        void OnUndoBefore();
        void OnUndoAfter();
    }
}