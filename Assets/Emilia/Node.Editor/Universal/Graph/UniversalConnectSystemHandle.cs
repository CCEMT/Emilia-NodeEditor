using System;
using Emilia.Kit;
using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    [EditorHandle(typeof(EditorUniversalGraphAsset))]
    public class UniversalConnectSystemHandle : ConnectSystemHandle
    {
        public override Type GetConnectorListenerType(EditorGraphView graphView) => typeof(UniversalEdgeConnectorListener);
        
        public override Type GetEdgeAssetTypeByPort(EditorGraphView graphView, IEditorPortView portView) => typeof(UniversalEditorEdgeAsset);

        public override bool CanConnect(EditorGraphView graphView, IEditorPortView inputPort, IEditorPortView outputPort)
        {
            if (inputPort.portElement.portType != outputPort.portElement.portType) return false;

            if (inputPort.portDirection == EditorPortDirection.Any || outputPort.portDirection == EditorPortDirection.Any) return true;
            if (inputPort.portDirection == EditorPortDirection.Input && outputPort.portDirection == EditorPortDirection.Output) return true;
            if (inputPort.portDirection == EditorPortDirection.Output && outputPort.portDirection == EditorPortDirection.Input) return true;

            return false;
        }
    }
}