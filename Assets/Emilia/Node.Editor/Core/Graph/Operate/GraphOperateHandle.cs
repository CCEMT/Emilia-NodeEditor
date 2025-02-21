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
            smartValue = weakSmartValue as EditorGraphView;
            parentHandle = parent as IGraphOperateHandle;
        }

        public virtual void OpenCreateNodeMenu(Vector2 mousePosition, CreateNodeContext createNodeContext = default)
        {
            parentHandle?.OpenCreateNodeMenu(mousePosition, createNodeContext);
        }

        public virtual void Cut()
        {
            parentHandle?.Cut();
        }

        public virtual void Copy()
        {
            parentHandle?.Copy();
        }

        public virtual void Paste()
        {
            parentHandle?.Paste();
        }

        public virtual void Delete()
        {
            parentHandle?.Delete();
        }

        public virtual void Duplicate()
        {
            parentHandle?.Duplicate();
        }

        public virtual void Save()
        {
            parentHandle?.Save();
        }

        public override void Dispose()
        {
            base.Dispose();
            smartValue = null;
            parentHandle = null;
        }
    }
}