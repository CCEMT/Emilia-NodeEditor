using UnityEngine;

namespace Emilia.Node.Editor
{
    [GenericHandle]
    public abstract class GraphOperateHandle<T> : EditorHandle, IGraphOperateHandle where T : EditorGraphAsset
    {
        public EditorGraphView smartValue { get; private set; }
        public IGraphOperateHandle parentHandle { get; private set; }

        public override void Initialize(object weakSmartValue)
        {
            base.Initialize(weakSmartValue);
            this.smartValue = weakSmartValue as EditorGraphView;
            parentHandle = parent as IGraphOperateHandle;
        }

        public virtual void OpenCreateNodeMenu(Vector2 mousePosition, CreateNodeContext createNodeContext = default)
        {
            this.parentHandle?.OpenCreateNodeMenu(mousePosition, createNodeContext);
        }

        public virtual void Cut()
        {
            this.parentHandle?.Cut();
        }

        public virtual void Copy()
        {
            this.parentHandle?.Copy();
        }

        public virtual void Paste()
        {
            this.parentHandle?.Paste();
        }

        public virtual void Delete()
        {
            this.parentHandle?.Delete();
        }

        public virtual void Duplicate()
        {
            this.parentHandle?.Duplicate();
        }

        public virtual void Save()
        {
            this.parentHandle?.Save();
        }

        public override void Dispose()
        {
            base.Dispose();
            this.smartValue = default;
        }
    }
}