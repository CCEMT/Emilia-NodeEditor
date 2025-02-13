namespace Emilia.Node.Editor
{
    [GenericHandle]
    public class GraphSaveHandle<T> : EditorHandle, IGraphSaveHandle where T : EditorGraphAsset
    {
        public virtual void OnSaveBefore() { }
        public virtual void OnSaveAfter() { }
    }
}