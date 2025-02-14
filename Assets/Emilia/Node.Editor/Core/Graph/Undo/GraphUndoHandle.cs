namespace Emilia.Node.Editor
{
    [GenericHandle]
    public class GraphUndoHandle<T> : EditorHandle, IGraphUndoHandle where T : EditorGraphAsset
    {
        public EditorGraphView smartValue { get; private set; }
        public IGraphUndoHandle parentHandle { get; private set; }

        public override void Initialize(object weakSmartValue)
        {
            base.Initialize(weakSmartValue);
            smartValue = weakSmartValue as EditorGraphView;
            parentHandle = parent as IGraphUndoHandle;
        }

        public virtual void OnUndoBefore()
        {
            parentHandle?.OnUndoBefore();
        }

        public virtual void OnUndoAfter()
        {
            parentHandle?.OnUndoAfter();
        }
        
        public override void Dispose()
        {
            base.Dispose();
            smartValue = null;
            parentHandle = null;
        }
    }
}