using System;
using System.Collections.Generic;
using Emilia.Kit;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public abstract class GraphHandle
    {
        public virtual void Initialize(EditorGraphView graphView) { }
        public virtual void InitializeCustomModule(EditorGraphView graphView, Dictionary<Type, CustomGraphViewModule> modules) { }
        public virtual void OnLoadBefore(EditorGraphView graphView) { }
        public virtual void OnLoadAfter(EditorGraphView graphView) { }
        public virtual void OnEnterFocus(EditorGraphView graphView) { }
        public virtual void OnFocus(EditorGraphView graphView) { }
        public virtual void OnExitFocus(EditorGraphView graphView) { }
        public virtual void OnUpdate(EditorGraphView graphView) { }
        public virtual void Dispose(EditorGraphView graphView) { }

        protected void AddModule<TModule>(Dictionary<Type, CustomGraphViewModule> modules) where TModule : CustomGraphViewModule, new()
        {
            modules.Add(typeof(TModule), new TModule());
        }
    }
}