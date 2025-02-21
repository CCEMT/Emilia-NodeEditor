using System;
using System.Collections.Generic;

namespace Emilia.Node.Editor
{
    public interface IGraphHandle : IEditorHandle
    {
        void InitializeCustomModule(Dictionary<Type, GraphViewModule> modules);

        void OnLoadBefore();
        void OnLoadAfter();

        void OnFocus();
        void OnUnFocus();
        void OnUpdate();
    }
}