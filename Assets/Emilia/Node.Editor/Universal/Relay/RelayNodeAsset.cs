using System.Collections.Generic;
using Emilia.Node.Editor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    /// <summary>
    /// Logical transparent relay node. Business graphs decide creation entry points and connection constraints in subclasses.
    /// </summary>
    [HideMonoScript]
    public class RelayNodeAsset : UniversalNodeAsset
    {
        [SerializeField, HideInInspector]
        private EditorOrientation _portOrientation = EditorOrientation.Horizontal;

        public EditorOrientation portOrientation
        {
            get => _portOrientation;
            set => _portOrientation = value;
        }

        protected override string defaultDisplayName => "Relay";

        public override bool isLogicalTransparent => true;

        public override List<EditorLogicalConnection> GetLogicalOutputNodes(HashSet<string> visited = null)
        {
            if (graphAsset == null) return new List<EditorLogicalConnection>();

            visited ??= new HashSet<string>();
            if (visited.Add(id) == false) return new List<EditorLogicalConnection>();

            List<RelayEndpoint> sources = ResolveInputEndpoints(visited);
            List<RelayEndpoint> targets = ResolveOutputEndpoints(visited);
            return CreateOutputConnections(sources, targets);
        }

        public override List<EditorLogicalConnection> GetLogicalInputNodes(HashSet<string> visited = null)
        {
            if (graphAsset == null) return new List<EditorLogicalConnection>();

            visited ??= new HashSet<string>();
            if (visited.Add(id) == false) return new List<EditorLogicalConnection>();

            List<RelayEndpoint> sources = ResolveInputEndpoints(visited);
            List<RelayEndpoint> targets = ResolveOutputEndpoints(visited);
            return CreateInputConnections(sources, targets);
        }

        private List<RelayEndpoint> ResolveInputEndpoints(HashSet<string> visited)
        {
            List<RelayEndpoint> result = new();
            List<EditorEdgeAsset> edges = graphAsset.GetInputEdges(this);

            int edgeCount = edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                EditorEdgeAsset edge = edges[i];
                if (edge.inputPortId != RelayHelper.InputPortId) continue;

                EditorNodeAsset sourceNode = graphAsset.nodeMap.GetValueOrDefault(edge.outputNodeId);
                if (sourceNode == null) continue;

                AppendInputEndpoint(result, sourceNode, edge, visited);
            }

            return result;
        }

        private List<RelayEndpoint> ResolveOutputEndpoints(HashSet<string> visited)
        {
            List<RelayEndpoint> result = new();
            List<EditorEdgeAsset> edges = graphAsset.GetOutputEdges(this);

            int edgeCount = edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                EditorEdgeAsset edge = edges[i];
                if (edge.outputPortId != RelayHelper.OutputPortId) continue;

                EditorNodeAsset targetNode = graphAsset.nodeMap.GetValueOrDefault(edge.inputNodeId);
                if (targetNode == null) continue;

                AppendOutputEndpoint(result, targetNode, edge, visited);
            }

            return result;
        }

        private void AppendInputEndpoint(
            List<RelayEndpoint> result,
            EditorNodeAsset sourceNode,
            EditorEdgeAsset edge,
            HashSet<string> visited)
        {
            if (sourceNode.isLogicalTransparent)
            {
                List<EditorLogicalConnection> logicalConnections = sourceNode.GetLogicalInputNodes(new HashSet<string>(visited));
                int count = logicalConnections.Count;
                for (int i = 0; i < count; i++)
                {
                    EditorLogicalConnection logicalConnection = logicalConnections[i];
                    AddEndpoint(result, logicalConnection.outputNode, logicalConnection.outputPortId);
                }

                return;
            }

            AddEndpoint(result, sourceNode, edge.outputPortId);
        }

        private void AppendOutputEndpoint(
            List<RelayEndpoint> result,
            EditorNodeAsset targetNode,
            EditorEdgeAsset edge,
            HashSet<string> visited)
        {
            if (targetNode.isLogicalTransparent)
            {
                List<EditorLogicalConnection> logicalConnections = targetNode.GetLogicalOutputNodes(new HashSet<string>(visited));
                int count = logicalConnections.Count;
                for (int i = 0; i < count; i++)
                {
                    EditorLogicalConnection logicalConnection = logicalConnections[i];
                    AddEndpoint(result, logicalConnection.inputNode, logicalConnection.inputPortId);
                }

                return;
            }

            AddEndpoint(result, targetNode, edge.inputPortId);
        }

        private List<EditorLogicalConnection> CreateOutputConnections(
            List<RelayEndpoint> sources,
            List<RelayEndpoint> targets)
        {
            List<EditorLogicalConnection> result = new();

            int targetCount = targets.Count;
            for (int targetIndex = 0; targetIndex < targetCount; targetIndex++)
            {
                RelayEndpoint target = targets[targetIndex];

                if (sources.Count == 0)
                {
                    result.Add(new EditorLogicalConnection(this, RelayHelper.OutputPortId, target.node, target.portId));
                    continue;
                }

                int sourceCount = sources.Count;
                for (int sourceIndex = 0; sourceIndex < sourceCount; sourceIndex++)
                {
                    RelayEndpoint source = sources[sourceIndex];
                    result.Add(new EditorLogicalConnection(source.node, source.portId, target.node, target.portId));
                }
            }

            return result;
        }

        private List<EditorLogicalConnection> CreateInputConnections(
            List<RelayEndpoint> sources,
            List<RelayEndpoint> targets)
        {
            List<EditorLogicalConnection> result = new();

            int sourceCount = sources.Count;
            for (int sourceIndex = 0; sourceIndex < sourceCount; sourceIndex++)
            {
                RelayEndpoint source = sources[sourceIndex];

                if (targets.Count == 0)
                {
                    result.Add(new EditorLogicalConnection(source.node, source.portId, this, RelayHelper.InputPortId));
                    continue;
                }

                int targetCount = targets.Count;
                for (int targetIndex = 0; targetIndex < targetCount; targetIndex++)
                {
                    RelayEndpoint target = targets[targetIndex];
                    result.Add(new EditorLogicalConnection(source.node, source.portId, target.node, target.portId));
                }
            }

            return result;
        }

        private static void AddEndpoint(List<RelayEndpoint> result, EditorNodeAsset node, string portId)
        {
            if (node == null || string.IsNullOrEmpty(portId)) return;

            int count = result.Count;
            for (int i = 0; i < count; i++)
            {
                RelayEndpoint endpoint = result[i];
                if (endpoint.node.id == node.id && endpoint.portId == portId) return;
            }

            result.Add(new RelayEndpoint {node = node, portId = portId});
        }

        private struct RelayEndpoint
        {
            public EditorNodeAsset node;
            public string portId;
        }
    }
}
