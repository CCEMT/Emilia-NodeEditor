namespace Emilia.Node.Editor
{
    public interface IGraphSaveHandle : IEditorHandle
    {
        void OnSaveBefore();

        void OnSaveAfter();
    }
}