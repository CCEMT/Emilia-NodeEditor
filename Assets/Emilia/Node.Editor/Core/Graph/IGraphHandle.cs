using System;
using System.Collections.Generic;

namespace Emilia.Node.Editor
{
    public interface IGraphHandle : IEditorHandle
    {
        void InitializeCustomModule(Dictionary<Type, CustomGraphViewModule> modules);

        void OnLoadBefore();
        void OnLoadAfter();

        void OnEnterFocus();
        void OnFocus();
        void OnExitFocus();
        void OnUpdate();
    }
}