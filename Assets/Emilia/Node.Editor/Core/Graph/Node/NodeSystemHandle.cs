namespace Emilia.Node.Editor
{
    [GenericHandle]
    public abstract class NodeSystemHandle<T> : EditorHandle, INodeSystemHandle where T : EditorGraphAsset
    {
        public EditorGraphView smartValue { get; private set; }

        public INodeSystemHandle parentHandle { get; private set; }

        public override void Initialize(object weakSmartValue)
        {
            base.Initialize(weakSmartValue);
            this.smartValue = weakSmartValue as EditorGraphView;
            parentHandle = parent as INodeSystemHandle;
        }

        public virtual void OnCreateNode(IEditorNodeView editorNodeView)
        {
            parentHandle?.OnCreateNode(editorNodeView);
        }

        public override void Dispose()
        {
            base.Dispose();
            smartValue = default;
        }
    }
}