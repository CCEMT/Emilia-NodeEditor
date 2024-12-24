using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

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
            this.smartValue = weakSmartValue as EditorGraphView;
            parentHandle = parent as IGraphSelectedHandle;
        }

        public virtual void UpdateSelectedInspector(List<ISelectable> selection)
        {
            parentHandle?.UpdateSelectedInspector(selection);
        }

        public virtual void CollectSelectedDrawer(List<IGraphSelectedDrawer> drawers)
        {
            parentHandle?.CollectSelectedDrawer(drawers);
        }

        public virtual void BeforeUpdateSelected(List<ISelectable> selection)
        {
            parentHandle?.BeforeUpdateSelected(selection);
        }

        public virtual void AfterUpdateSelected(List<ISelectable> selection)
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