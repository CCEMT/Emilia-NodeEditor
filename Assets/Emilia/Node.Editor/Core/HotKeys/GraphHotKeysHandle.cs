using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    [GenericHandle]
    public abstract class GraphHotKeysHandle<T> : EditorHandle, IGraphHotKeysHandle where T : EditorGraphAsset
    {
        protected EditorGraphView smartValue { get; private set; }
        public IGraphHotKeysHandle parentHandle { get; private set; }

        public override void Initialize(object weakSmartValue)
        {
            base.Initialize(weakSmartValue);
            smartValue = weakSmartValue as EditorGraphView;
            parentHandle = parent as IGraphHotKeysHandle;
        }

        public virtual void OnKeyDown(KeyDownEvent evt)
        {
            parentHandle?.OnKeyDown(evt);
        }

        public override void Dispose()
        {
            base.Dispose();
            smartValue = null;
            parentHandle = null;
        }
    }
}