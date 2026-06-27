using System;
using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    public class UniversalConnectContext
    {
        public EditorGraphView graphView { get; }
        public IEditorPortView inputPort { get; }
        public IEditorPortView outputPort { get; }

        public UniversalConnectContext(EditorGraphView graphView, IEditorPortView inputPort, IEditorPortView outputPort)
        {
            this.graphView = graphView;
            this.inputPort = inputPort;
            this.outputPort = outputPort;
        }

        public bool CanConnectByDirectionAndType()
        {
            return CanConnectByDirectionAndType(inputPort, outputPort);
        }

        public bool CanConnectByDirectionAndType(IEditorPortView inputPort, IEditorPortView outputPort)
        {
            if (inputPort == null || outputPort == null) return false;
            if (inputPort.portDirection == EditorPortDirection.Any || outputPort.portDirection == EditorPortDirection.Any) return true;

            IEditorPortView targetInputPort = null;
            IEditorPortView sourceOutputPort = null;

            if (inputPort.portDirection == EditorPortDirection.Input &&
                outputPort.portDirection == EditorPortDirection.Output)
            {
                targetInputPort = inputPort;
                sourceOutputPort = outputPort;
            }
            else if (inputPort.portDirection == EditorPortDirection.Output &&
                     outputPort.portDirection == EditorPortDirection.Input)
            {
                targetInputPort = outputPort;
                sourceOutputPort = inputPort;
            }

            if (targetInputPort == null || sourceOutputPort == null) return false;

            Type inputType = targetInputPort.portElement.portType;
            Type outputType = sourceOutputPort.portElement.portType;

            if (inputType == outputType) return true;
            if (inputType == typeof(object) || outputType == typeof(object)) return true;
            if (inputType != null && outputType != null && inputType.IsAssignableFrom(outputType)) return true;

            return false;
        }

        public bool IsSourcePort(IEditorPortView portView)
        {
            if (portView == null) return false;
            return portView.portDirection == EditorPortDirection.Output ||
                   portView.portDirection == EditorPortDirection.Any;
        }

        public bool IsTargetPort(IEditorPortView portView)
        {
            if (portView == null) return false;
            return portView.portDirection == EditorPortDirection.Input ||
                   portView.portDirection == EditorPortDirection.Any;
        }
    }

    public interface IUniversalConnectConstraintNodeView
    {
        bool CanConnect(UniversalConnectContext context);
    }

    public interface IUniversalConnectionChangedNodeView
    {
        void AfterConnect(UniversalConnectContext context, IEditorEdgeView edgeView);
        void AfterDisconnect(EditorGraphView graphView, EditorEdgeAsset edgeAsset);
    }
}
