using System;
using Emilia.Kit;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public abstract class ConnectSystemHandle
    {
        public virtual Type GetConnectorListenerType(EditorGraphView graphView) => typeof(EditorEdgeConnectorListener);

        public virtual Type GetEdgeTypeByPort(EditorGraphView graphView, IEditorPortView portView) => null;

        public virtual bool CanConnect(EditorGraphView graphView, IEditorPortView inputPort, IEditorPortView outputPort) => false;

        public virtual bool BeforeConnect(EditorGraphView graphView, IEditorPortView input, IEditorPortView output) => false;

        public virtual void AfterConnect(EditorGraphView graphView, IEditorEdgeView edgeView) { }
    }
}