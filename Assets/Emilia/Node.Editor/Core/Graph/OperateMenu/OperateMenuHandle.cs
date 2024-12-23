using System.Collections.Generic;

namespace Emilia.Node.Editor
{
    [GenericHandle]
    public abstract class OperateMenuHandle<T> : EditorHandle, IOperateMenuHandle where T : EditorGraphAsset
    {
        public EditorGraphView smartValue { get; private set; }
        public IOperateMenuHandle parentHandle { get; private set; }

        public override void Initialize(object weakSmartValue)
        {
            base.Initialize(weakSmartValue);
            smartValue = weakSmartValue as EditorGraphView;
            parentHandle = parent as IOperateMenuHandle;
        }

        public virtual void InitializeCache()
        {
            parentHandle?.InitializeCache();
        }

        public virtual void CollectMenuItems(List<OperateMenuItem> menuItems, OperateMenuContext context)
        {
            parentHandle?.CollectMenuItems(menuItems, context);
        }

        public override void Dispose()
        {
            base.Dispose();
            this.smartValue = default;
        }
    }
}