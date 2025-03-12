using System.Collections.Generic;

namespace Emilia.Node.Editor
{
    [GenericHandle]
    public abstract class CreateItemMenuHandle<T> : EditorHandle, ICreateItemMenuHandle where T : EditorGraphAsset
    {
        public EditorGraphView smartValue { get; private set; }
        public ICreateItemMenuHandle parentHandle { get; private set; }

        public override void Initialize(object weakSmartValue)
        {
            base.Initialize(weakSmartValue);
            smartValue = weakSmartValue as EditorGraphView;
            parentHandle = parent as ICreateItemMenuHandle;
        }

        public virtual void CollectItemMenus(List<CreateItemMenuInfo> itemTypes) { }

        public override void Dispose()
        {
            base.Dispose();
            smartValue = null;
            parentHandle = null;
        }
    }
}