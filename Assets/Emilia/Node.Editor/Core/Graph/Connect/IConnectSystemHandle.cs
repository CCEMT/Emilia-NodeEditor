using System;

namespace Emilia.Node.Editor
{
    public interface IConnectSystemHandle : IEditorHandle
    {
        Type connectorListenerType { get; }
        Type GetEdgeTypeByPort(IEditorPortView portView);
        bool CanConnect(IEditorPortView inputPort, IEditorPortView outputPort);
        
        bool BeforeConnect(IEditorPortView input, IEditorPortView output);
        void AfterConnect(IEditorEdgeView edgeView);
    }
}