namespace Emilia.Node.Editor
{
    [GenericHandle]
    public abstract class ItemSystemHandle<T> : EditorHandle, IItemSystemHandle where T : EditorGraphAsset
    {
        public EditorGraphView smartValue { get; private set; }
        public IItemSystemHandle parentHandle { get; private set; }

        public override void Initialize(object weakSmartValue)
        {
            base.Initialize(weakSmartValue);
            smartValue = weakSmartValue as EditorGraphView;
            parentHandle = parent as IItemSystemHandle;
        }

        public virtual void OnCreateItem(IEditorItemView itemView)
        {
            parentHandle?.OnCreateItem(itemView);
        }

        public override void Dispose()
        {
            base.Dispose();
            smartValue = null;
            parentHandle = null;
        }
    }
}