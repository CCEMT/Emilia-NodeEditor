using System.Collections.Generic;
using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    public static partial class RelayHelper
    {
        public static bool IsRelayPort(IEditorPortView portView)
        {
            return portView?.master is RelayEditorNodeView;
        }

        public static bool IsRelayInputPort(IEditorPortView portView)
        {
            return portView?.master is RelayEditorNodeView && portView.info.id == InputPortId;
        }

        public static bool IsRelayOutputPort(IEditorPortView portView)
        {
            return portView?.master is RelayEditorNodeView && portView.info.id == OutputPortId;
        }

        public static IEditorPortView GetRelayInputPort(IEditorNodeView relayNodeView)
        {
            return relayNodeView?.GetPortView(InputPortId);
        }

        public static IEditorPortView GetRelayOutputPort(IEditorNodeView relayNodeView)
        {
            return relayNodeView?.GetPortView(OutputPortId);
        }

        public static IEditorPortView ResolveSourceOutputPort(IEditorPortView outputPort)
        {
            List<IEditorPortView> sourceOutputs = ResolveSourceOutputPorts(outputPort);
            return sourceOutputs.Count == 1 ? sourceOutputs[0] : outputPort;
        }

        public static List<IEditorPortView> ResolveSourceOutputPorts(IEditorPortView outputPort)
        {
            List<IEditorPortView> result = new();
            if (outputPort == null) return result;

            if (IsRelayOutputPort(outputPort))
            {
                List<IEditorPortView> sourceOutputs = GetConnectedSourceOutputs(outputPort.master as RelayEditorNodeView);
                if (sourceOutputs.Count > 0) return sourceOutputs;
            }

            result.Add(outputPort);
            return result;
        }

        public static IEditorPortView ResolveTargetInputPort(IEditorPortView inputPort)
        {
            List<IEditorPortView> targetInputs = ResolveTargetInputPorts(inputPort);
            return targetInputs.Count == 1 ? targetInputs[0] : inputPort;
        }

        public static List<IEditorPortView> ResolveTargetInputPorts(IEditorPortView inputPort)
        {
            List<IEditorPortView> result = new();
            if (inputPort == null) return result;

            if (IsRelayInputPort(inputPort))
            {
                List<IEditorPortView> targetInputs = GetConnectedTargetInputs(inputPort.master as RelayEditorNodeView);
                if (targetInputs.Count > 0) return targetInputs;
            }

            result.Add(inputPort);
            return result;
        }

        public static IEditorPortView GetConnectedSourceOutput(RelayEditorNodeView relayView, HashSet<string> visited = null)
        {
            List<IEditorPortView> sourceOutputs = GetConnectedSourceOutputs(relayView, visited);
            return sourceOutputs.Count > 0 ? sourceOutputs[0] : null;
        }

        public static List<IEditorPortView> GetConnectedSourceOutputs(RelayEditorNodeView relayView, HashSet<string> visited = null)
        {
            List<IEditorPortView> result = new();
            CollectConnectedSourceOutputs(relayView, result, visited);
            return result;
        }

        public static IEditorPortView GetConnectedTargetInput(RelayEditorNodeView relayView, HashSet<string> visited = null)
        {
            List<IEditorPortView> targetInputs = GetConnectedTargetInputs(relayView, visited);
            return targetInputs.Count > 0 ? targetInputs[0] : null;
        }

        public static List<IEditorPortView> GetConnectedTargetInputs(RelayEditorNodeView relayView, HashSet<string> visited = null)
        {
            List<IEditorPortView> result = new();
            CollectConnectedTargetInputs(relayView, result, visited);
            return result;
        }

        private static void CollectConnectedSourceOutputs(
            RelayEditorNodeView relayView,
            List<IEditorPortView> result,
            HashSet<string> visited = null)
        {
            if (relayView == null) return;

            visited ??= new HashSet<string>();
            if (visited.Add(relayView.asset.id) == false) return;

            IEditorPortView inputPort = GetRelayInputPort(relayView);
            if (inputPort == null) return;

            IReadOnlyList<IEditorEdgeView> edges = inputPort.edges;
            int edgeCount = edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                IEditorPortView sourceOutput = edges[i].outputPortView;
                if (IsRelayOutputPort(sourceOutput))
                {
                    CollectConnectedSourceOutputs(sourceOutput.master as RelayEditorNodeView, result, new HashSet<string>(visited));
                    continue;
                }

                AddDistinctPort(result, sourceOutput);
            }
        }

        private static void CollectConnectedTargetInputs(
            RelayEditorNodeView relayView,
            List<IEditorPortView> result,
            HashSet<string> visited = null)
        {
            if (relayView == null) return;

            visited ??= new HashSet<string>();
            if (visited.Add(relayView.asset.id) == false) return;

            IEditorPortView outputPort = GetRelayOutputPort(relayView);
            if (outputPort == null) return;

            IReadOnlyList<IEditorEdgeView> edges = outputPort.edges;
            int edgeCount = edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                IEditorPortView targetInput = edges[i].inputPortView;
                if (IsRelayInputPort(targetInput))
                {
                    CollectConnectedTargetInputs(targetInput.master as RelayEditorNodeView, result, new HashSet<string>(visited));
                    continue;
                }

                AddDistinctPort(result, targetInput);
            }
        }

        public static void RefreshRelayView(IEditorNodeView relayNodeView)
        {
            if (relayNodeView is RelayEditorNodeView relayView)
                relayView.RefreshPortFromConnections();
        }

        public static void RefreshRelayViews(EditorGraphView graphView)
        {
            if (graphView == null) return;

            foreach (IEditorNodeView nodeView in graphView.graphElementCache.nodeViewById.Values)
            {
                RefreshRelayView(nodeView);
            }
        }
    }
}