using System.Collections.Generic;
using Emilia.Node.Editor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    /// <summary>
    /// Portal节点方向枚举
    /// </summary>
    public enum PortalDirection
    {
        /// <summary>
        /// 入口Portal，接收来自其他节点输出端口的连接
        /// </summary>
        Entry,

        /// <summary>
        /// 出口Portal，将连接发送到其他节点的输入端口
        /// </summary>
        Exit
    }

    /// <summary>
    /// Portal节点资产，用于将长距离边连接转换为Portal传送连接。
    /// Portal是透传节点，在图遍历时逻辑透明，不影响实际的节点连接关系。
    /// Entry和Exit Portal成对存在，通过portalGroupId关联。
    /// </summary>
    [HideMonoScript]
    public class PortalNodeAsset : UniversalNodeAsset
    {
        [SerializeField, HideInInspector]
        private PortalDirection _direction;

        [SerializeField, HideInInspector]
        private string _portalGroupId;

        [SerializeField, HideInInspector]
        private string _linkedPortalId;

        [SerializeField, HideInInspector]
        private EditorOrientation _portOrientation = EditorOrientation.Horizontal;

        /// <summary>
        /// Portal方向（Entry或Exit）
        /// </summary>
        public PortalDirection direction
        {
            get => _direction;
            set => _direction = value;
        }

        /// <summary>
        /// Portal组ID，同一组的Portal可以互相透传连接
        /// </summary>
        public string portalGroupId
        {
            get => _portalGroupId;
            set => _portalGroupId = value;
        }

        /// <summary>
        /// 关联的Portal节点ID（Entry关联Exit，Exit关联Entry）
        /// </summary>
        public string linkedPortalId
        {
            get => _linkedPortalId;
            set => _linkedPortalId = value;
        }

        /// <summary>
        /// 端口方向（水平或垂直）
        /// </summary>
        public EditorOrientation portOrientation
        {
            get => _portOrientation;
            set => _portOrientation = value;
        }

        protected override string defaultDisplayName => direction == PortalDirection.Entry ? "Portal Entry" : "Portal Exit";

        public override string title => string.IsNullOrEmpty(displayName) ? defaultDisplayName : displayName;

        public override bool isLogicalTransparent => true;

        /// <summary>
        /// 获取逻辑输出连接，实现Portal的透传遍历。
        /// Entry Portal会跳转到关联的Exit Portal获取其输出节点。
        /// Exit Portal会获取其直接连接的目标节点。
        /// </summary>
        public override List<EditorLogicalConnection> GetLogicalOutputNodes(HashSet<string> visited = null)
        {
            if (graphAsset == null) return new List<EditorLogicalConnection>();

            visited ??= new HashSet<string>();
            if (visited.Add(id) == false) return new List<EditorLogicalConnection>();

            var result = new List<EditorLogicalConnection>();

            if (direction == PortalDirection.Entry) AppendLinkedPortalOutputs(result, visited);
            else AppendDirectOutputs(result, visited);

            return result;
        }

        /// <summary>
        /// 获取逻辑输入连接，实现Portal的透传遍历。
        /// Exit Portal会跳转到关联的Entry Portal获取其输入节点。
        /// Entry Portal会获取其直接连接的源节点。
        /// </summary>
        public override List<EditorLogicalConnection> GetLogicalInputNodes(HashSet<string> visited = null)
        {
            if (graphAsset == null) return new List<EditorLogicalConnection>();

            visited ??= new HashSet<string>();
            if (visited.Add(id) == false) return new List<EditorLogicalConnection>();

            var result = new List<EditorLogicalConnection>();

            if (direction == PortalDirection.Exit) AppendLinkedPortalInputs(result, visited);
            else AppendDirectInputs(result, visited);

            return result;
        }

        /// <summary>
        /// 通过关联Portal获取输出节点（Entry Portal使用）
        /// </summary>
        private void AppendLinkedPortalOutputs(List<EditorLogicalConnection> result, HashSet<string> visited)
        {
            List<EditorNodeAsset> linkedPortals = GetLinkedPortals(PortalDirection.Exit);
            int count = linkedPortals.Count;
            for (int i = 0; i < count; i++)
            {
                EditorNodeAsset linkedPortal = linkedPortals[i];
                result.AddRange(linkedPortal.GetLogicalOutputNodes(new HashSet<string>(visited)));
            }
        }

        /// <summary>
        /// 获取直接连接的输出节点（Exit Portal使用）
        /// </summary>
        private void AppendDirectOutputs(List<EditorLogicalConnection> result, HashSet<string> visited)
        {
            List<EditorEdgeAsset> edges = graphAsset.GetOutputEdges(this);
            foreach (EditorEdgeAsset edge in edges)
            {
                EditorNodeAsset targetNode = graphAsset.nodeMap.GetValueOrDefault(edge.inputNodeId);
                if (targetNode != null) AppendLogicalOutputNode(result, targetNode, edge, visited);
            }
        }

        /// <summary>
        /// 通过关联Portal获取输入节点（Exit Portal使用）
        /// </summary>
        private void AppendLinkedPortalInputs(List<EditorLogicalConnection> result, HashSet<string> visited)
        {
            List<EditorNodeAsset> linkedPortals = GetLinkedPortals(PortalDirection.Entry);
            int count = linkedPortals.Count;
            for (int i = 0; i < count; i++)
            {
                EditorNodeAsset linkedPortal = linkedPortals[i];
                result.AddRange(linkedPortal.GetLogicalInputNodes(new HashSet<string>(visited)));
            }
        }

        /// <summary>
        /// 获取直接连接的输入节点（Entry Portal使用）
        /// </summary>
        private void AppendDirectInputs(List<EditorLogicalConnection> result, HashSet<string> visited)
        {
            List<EditorEdgeAsset> edges = graphAsset.GetInputEdges(this);
            foreach (EditorEdgeAsset edge in edges)
            {
                EditorNodeAsset sourceNode = graphAsset.nodeMap.GetValueOrDefault(edge.outputNodeId);
                if (sourceNode != null) AppendLogicalInputNode(result, sourceNode, edge, visited);
            }
        }

        private void AppendLogicalOutputNode(
            List<EditorLogicalConnection> result,
            EditorNodeAsset nodeAsset,
            EditorEdgeAsset edge,
            HashSet<string> visited)
        {
            if (nodeAsset.isLogicalTransparent)
            {
                List<EditorLogicalConnection> logicalConnections = nodeAsset.GetLogicalOutputNodes(new HashSet<string>(visited));
                int count = logicalConnections.Count;
                for (int i = 0; i < count; i++)
                {
                    EditorLogicalConnection logicalConnection = logicalConnections[i];
                    result.Add(new EditorLogicalConnection(
                        this,
                        edge.outputPortId,
                        logicalConnection.inputNode,
                        logicalConnection.inputPortId));
                }

                return;
            }

            result.Add(new EditorLogicalConnection(this, edge.outputPortId, nodeAsset, edge.inputPortId));
        }

        private void AppendLogicalInputNode(
            List<EditorLogicalConnection> result,
            EditorNodeAsset nodeAsset,
            EditorEdgeAsset edge,
            HashSet<string> visited)
        {
            if (nodeAsset.isLogicalTransparent)
            {
                List<EditorLogicalConnection> logicalConnections = nodeAsset.GetLogicalInputNodes(new HashSet<string>(visited));
                int count = logicalConnections.Count;
                for (int i = 0; i < count; i++)
                {
                    EditorLogicalConnection logicalConnection = logicalConnections[i];
                    result.Add(new EditorLogicalConnection(
                        logicalConnection.outputNode,
                        logicalConnection.outputPortId,
                        this,
                        edge.inputPortId));
                }

                return;
            }

            result.Add(new EditorLogicalConnection(nodeAsset, edge.outputPortId, this, edge.inputPortId));
        }

        private List<EditorNodeAsset> GetLinkedPortals(PortalDirection targetDirection)
        {
            List<EditorNodeAsset> result = new();

            if (string.IsNullOrEmpty(portalGroupId) == false)
            {
                int nodeCount = graphAsset.nodes.Count;
                for (int i = 0; i < nodeCount; i++)
                {
                    if (graphAsset.nodes[i] is not PortalNodeAsset portal) continue;
                    if (portal.portalGroupId != portalGroupId) continue;
                    if (portal.direction != targetDirection) continue;

                    result.Add(portal);
                }
            }

            if (result.Count != 0 || string.IsNullOrEmpty(linkedPortalId)) return result;
            if (graphAsset.nodeMap.GetValueOrDefault(linkedPortalId) is PortalNodeAsset linkedPortal &&
                linkedPortal.direction == targetDirection)
            {
                result.Add(linkedPortal);
            }

            return result;
        }
    }
}