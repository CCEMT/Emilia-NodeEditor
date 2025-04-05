using System.Collections.Generic;
using Emilia.Kit;

namespace Emilia.Node.Editor
{
    [GenericHandle]
    public abstract class GraphSelectedHandle<T> : EditorHandle, IGraphSelectedHandle where T : EditorGraphAsset
    {
        public EditorGraphView smartValue { get; private set; }

        public IGraphSelectedHandle parentHandle { get; private set; }

        public override void Initialize(object weakSmartValue)
        {
            base.Initialize(weakSmartValue);
            smartValue = weakSmartValue as EditorGraphView;
            parentHandle = parent as IGraphSelectedHandle;
        }

        public virtual void UpdateSelectedInspector(List<ISelectedHandle> selection)
        {
            parentHandle?.UpdateSelectedInspector(selection);
        }

        public virtual void CollectSelectedDrawer(List<IGraphSelectedDrawer> drawers)
        {
            parentHandle?.CollectSelectedDrawer(drawers);
        }

        public virtual void BeforeUpdateSelected(List<ISelectedHandle> selection)
        {
            parentHandle?.BeforeUpdateSelected(selection);
        }

        public virtual void AfterUpdateSelected(List<ISelectedHandle> selection)
        {
            parentHandle?.AfterUpdateSelected(selection);
        }

        public override void Dispose()
        {
            base.Dispose();
            smartValue = null;
            parentHandle = null;
        }
    }
}