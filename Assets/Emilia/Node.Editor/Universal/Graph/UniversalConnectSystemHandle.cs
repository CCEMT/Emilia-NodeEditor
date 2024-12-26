using System;
using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    public class UniversalConnectSystemHandle : ConnectSystemHandle<EditorUniversalGraphAsset>
    {
        public override Type GetEdgeTypeByPort(IEditorPortView portView)
        {
            return typeof(UniversalEditorEdgeAsset);
        }

        public override bool CanConnect(IEditorPortView inputPort, IEditorPortView outputPort)
        {
            if (inputPort.portElement.portType != outputPort.portElement.portType) return false;

            if (inputPort.portDirection == EditorPortDirection.Any || outputPort.portDirection == EditorPortDirection.Any) return true;
            if (inputPort.portDirection == EditorPortDirection.Input && outputPort.portDirection == EditorPortDirection.Output) return true;
            if (inputPort.portDirection == EditorPortDirection.Output && outputPort.portDirection == EditorPortDirection.Input) return true;

            return false;
        }
    }
}