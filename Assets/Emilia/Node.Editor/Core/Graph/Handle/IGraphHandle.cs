namespace Emilia.Node.Editor
{
    public interface IGraphHandle : IEditorHandle
    {
        void OnLoadBefore();
        void OnLoadAfter();
        
        void OnFocus();
        void OnUpdate();
    }
}