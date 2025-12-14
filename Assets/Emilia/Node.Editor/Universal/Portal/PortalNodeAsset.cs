using System.Collections.Generic;
using Emilia.Node.Editor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    /// <summary>
    /// Portal节点方向
    /// </summary>
    public enum PortalDirection
    {
        /// <summary>
        /// 入口Portal - 接收来自原始输出端口的连接
        /// </summary>
        Entry,

        /// <summary>
        /// 出口Portal - 发送连接到原始输入端口
        /// </summary>
        Exit
    }

    /// <summary>
    /// Portal节点资产 - 用于将边转换为Portal连接
    /// Portal节点是透传节点，在逻辑上是透明的，不影响图的遍历
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

        /// <summary>
        /// Portal方向
        /// </summary>
        public PortalDirection direction
        {
            get => _direction;
            set => _direction = value;
        }

        /// <summary>
        /// Portal组Id - 同一组的Portal可以互相连接
        /// </summary>
        public string portalGroupId
        {
            get => _portalGroupId;
            set => _portalGroupId = value;
        }

        /// <summary>
        /// 关联的Portal节点Id
        /// </summary>
        public string linkedPortalId
        {
            get => _linkedPortalId;
            set => _linkedPortalId = value;
        }

        protected override string defaultDisplayName => direction == PortalDirection.Entry ? "Portal Entry" : "Portal Exit";

        public override string title
        {
            get
            {
                if (string.IsNullOrEmpty(displayName)) return defaultDisplayName;
                return displayName;
            }
        }

        /// <summary>
        /// 获取逻辑输出节点（重写基类方法，实现透传逻辑）
        /// </summary>
        public override List<EditorNodeAsset> GetLogicalOutputNodes(HashSet<string> visited = null)
        {
            if (graphAsset == null) return new List<EditorNodeAsset>();

            visited ??= new HashSet<string>();
            if (visited.Contains(id)) return new List<EditorNodeAsset>();
            visited.Add(id);

            List<EditorNodeAsset> result = new List<EditorNodeAsset>();

            if (direction == PortalDirection.Entry)
            {
                // Entry Portal: 跳转到关联的 Exit Portal，获取其逻辑输出
                if (!string.IsNullOrEmpty(linkedPortalId))
                {
                    EditorNodeAsset linkedPortal = graphAsset.nodeMap.GetValueOrDefault(linkedPortalId);
                    if (linkedPortal != null)
                    {
                        // 调用关联 Portal 的逻辑输出（多态）
                        result.AddRange(linkedPortal.GetLogicalOutputNodes(visited));
                    }
                }
            }
            else
            {
                // Exit Portal: 获取直接连接的节点，并递归解析
                List<EditorEdgeAsset> edges = graphAsset.GetOutputEdges(this);
                foreach (EditorEdgeAsset edge in edges)
                {
                    EditorNodeAsset targetNode = graphAsset.nodeMap.GetValueOrDefault(edge.inputNodeId);
                    if (targetNode == null) continue;

                    // 调用目标节点的逻辑输出（多态，自动处理链式透传）
                    result.AddRange(targetNode.GetLogicalOutputNodes(visited));
                }
            }

            return result;
        }

        /// <summary>
        /// 获取逻辑输入节点（重写基类方法，实现透传逻辑）
        /// </summary>
        public override List<EditorNodeAsset> GetLogicalInputNodes(HashSet<string> visited = null)
        {
            if (graphAsset == null) return new List<EditorNodeAsset>();

            visited ??= new HashSet<string>();
            if (visited.Contains(id)) return new List<EditorNodeAsset>();
            visited.Add(id);

            List<EditorNodeAsset> result = new List<EditorNodeAsset>();

            if (direction == PortalDirection.Exit)
            {
                // Exit Portal: 跳转到关联的 Entry Portal，获取其逻辑输入
                if (!string.IsNullOrEmpty(linkedPortalId))
                {
                    EditorNodeAsset linkedPortal = graphAsset.nodeMap.GetValueOrDefault(linkedPortalId);
                    if (linkedPortal != null)
                    {
                        // 调用关联 Portal 的逻辑输入（多态）
                        result.AddRange(linkedPortal.GetLogicalInputNodes(visited));
                    }
                }
            }
            else
            {
                // Entry Portal: 获取直接连接的节点，并递归解析
                List<EditorEdgeAsset> edges = graphAsset.GetInputEdges(this);
                foreach (EditorEdgeAsset edge in edges)
                {
                    EditorNodeAsset sourceNode = graphAsset.nodeMap.GetValueOrDefault(edge.outputNodeId);
                    if (sourceNode == null) continue;

                    // 调用来源节点的逻辑输入（多态，自动处理链式透传）
                    result.AddRange(sourceNode.GetLogicalInputNodes(visited));
                }
            }

            return result;
        }
    }
}
