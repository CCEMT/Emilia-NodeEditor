namespace Emilia.Node.Editor
{
    [GenericHandle]
    public class GraphUndoHandle<T> : EditorHandle, IGraphUndoHandle where T : EditorGraphAsset
    {
        public virtual void OnUndoBefore() { }
        public virtual void OnUndoAfter() { }
    }
}