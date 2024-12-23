namespace Emilia.Node.Editor
{
    [GenericHandle]
    public abstract class GraphPanelHandle<T> : EditorHandle, IGraphPanelHandle where T : EditorGraphAsset
    {
        public EditorGraphView smartValue { get; private set; }

        public IGraphPanelHandle parentHandle { get; private set; }

        public override void Initialize(object weakSmartValue)
        {
            base.Initialize(weakSmartValue);
            smartValue = weakSmartValue as EditorGraphView;
            parentHandle = parent as IGraphPanelHandle;
        }

        public virtual void LoadPanel(GraphPanelSystem system)
        {
            parentHandle?.LoadPanel(system);
        }

        public override void Dispose()
        {
            base.Dispose();
            smartValue = null;
            parentHandle = null;
        }
    }
}