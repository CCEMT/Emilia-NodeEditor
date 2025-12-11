using System;
using Emilia.Kit;
using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    /// <summary>
    /// 通用连接处理器
    /// </summary>
    [EditorHandle(typeof(EditorUniversalGraphAsset))]
    public class UniversalConnectSystemHandle : ConnectSystemHandle
    {
        public override Type GetConnectorListenerType(EditorGraphView graphView) => typeof(UniversalEdgeConnectorListener);

        public override Type GetEdgeAssetTypeByPort(EditorGraphView graphView, IEditorPortView portView) => typeof(UniversalEditorEdgeAsset);

        public override bool CanConnect(EditorGraphView graphView, IEditorPortView inputPort, IEditorPortView outputPort)
        {
            if (inputPort.portDirection == EditorPortDirection.Any || outputPort.portDirection == EditorPortDirection.Any) return true;

            bool isDirectionValid = (inputPort.portDirection == EditorPortDirection.Input && outputPort.portDirection == EditorPortDirection.Output) ||
                                    (inputPort.portDirection == EditorPortDirection.Output && outputPort.portDirection == EditorPortDirection.Input);

            if (isDirectionValid == false) return false;

            Type inputType = inputPort.portElement.portType;
            Type outputType = outputPort.portElement.portType;

            if (inputType == outputType) return true;

            if (inputType == typeof(object) || outputType == typeof(object)) return true;

            if (inputType != null && outputType != null && inputType.IsAssignableFrom(outputType)) return true;

            return false;
        }
    }
}