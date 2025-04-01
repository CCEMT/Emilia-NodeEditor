using System;
using System.Collections.Generic;

namespace Emilia.Node.Editor
{
    [GenericHandle]
    public abstract class GraphHandle<T> : EditorHandle, IGraphHandle where T : EditorGraphAsset
    {
        public EditorGraphView smartValue { get; private set; }
        public IGraphHandle parentHandle { get; private set; }

        public override void Initialize(object weakSmartValue)
        {
            base.Initialize(weakSmartValue);
            smartValue = weakSmartValue as EditorGraphView;
            parentHandle = parent as IGraphHandle;
        }

        public virtual void InitializeCustomModule(Dictionary<Type, CustomGraphViewModule> modules)
        {
            parentHandle?.InitializeCustomModule(modules);
        }

        protected void AddModule<TModule>(Dictionary<Type, CustomGraphViewModule> modules) where TModule : CustomGraphViewModule, new()
        {
            modules.Add(typeof(TModule), new TModule());
        }

        public virtual void OnLoadBefore()
        {
            parentHandle?.OnLoadBefore();
        }

        public virtual void OnLoadAfter()
        {
            parentHandle?.OnLoadAfter();
        }

        public virtual void OnEnterFocus()
        {
            parentHandle?.OnEnterFocus();
        }

        public virtual void OnFocus()
        {
            parentHandle?.OnFocus();
        }

        public virtual void OnExitFocus()
        {
            parentHandle?.OnExitFocus();
        }

        public virtual void OnUpdate()
        {
            parentHandle?.OnUpdate();
        }

        public override void Dispose()
        {
            base.Dispose();
            smartValue = null;
        }
    }
}