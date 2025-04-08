namespace Emilia.Node.Editor
{
    public interface IGraphUndoHandle : IEditorHandle
    {
        void OnUndoBefore(bool isSilent);
        void OnUndoAfter(bool isSilent);
    }
}