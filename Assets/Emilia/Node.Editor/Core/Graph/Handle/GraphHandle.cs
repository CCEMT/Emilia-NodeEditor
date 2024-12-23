namespace Emilia.Node.Editor
{
    [GenericHandle]
    public abstract class GraphHandle<T> : EditorHandle, IGraphHandle where T : EditorGraphAsset
    {
        public EditorGraphView smartValue { get; private set; }
        public IGraphHandle parentHandle { get; private set; }

        public override void Initialize(object weakSmartValue)
        {
            base.Initialize(weakSmartValue);
            this.smartValue = weakSmartValue as EditorGraphView;
            parentHandle = parent as IGraphHandle;
        }

        public virtual void OnLoadBefore()
        {
            parentHandle?.OnLoadBefore();
        }

        public virtual void OnLoadAfter()
        {
            parentHandle?.OnLoadAfter();
        }

        public virtual void OnFocus()
        {
            parentHandle?.OnFocus();
        }

        public virtual void OnUpdate()
        {
            parentHandle?.OnUpdate();
        }

        public override void Dispose()
        {
            base.Dispose();
            this.smartValue = default;
        }
    }
}