namespace Emilia.Node.Editor
{
    [GenericHandle]
    public class GraphSaveHandle<T> : EditorHandle, IGraphSaveHandle where T : EditorGraphAsset
    {
        public EditorGraphView smartValue { get; private set; }
        public IGraphSaveHandle parentHandle { get; private set; }

        public override void Initialize(object weakSmartValue)
        {
            base.Initialize(weakSmartValue);
            smartValue = weakSmartValue as EditorGraphView;
            parentHandle = parent as IGraphSaveHandle;
        }

        public virtual void OnSaveBefore()
        {
            parentHandle?.OnSaveBefore();
        }

        public virtual void OnSaveAfter()
        {
            parentHandle?.OnSaveAfter();
        }

        public override void Dispose()
        {
            base.Dispose();
            smartValue = null;
            parentHandle = null;
        }
    }
}