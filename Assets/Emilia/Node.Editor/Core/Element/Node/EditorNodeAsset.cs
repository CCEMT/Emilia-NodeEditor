using System;
using System.Collections.Generic;
using Emilia.Kit;
using Emilia.Kit.Editor;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor
{
    /// <summary>
    /// 编辑器Node资源
    /// </summary>
    [Serializable, SelectedClear]
    public class EditorNodeAsset : TitleAsset, IUnityAsset
    {
        [SerializeField, HideInInspector]
        private string _id;

        [SerializeField, HideInInspector]
        private Rect _position;

        [SerializeField, HideInInspector]
        private bool _isExpanded = false;

        [SerializeField, HideInInspector]
        private object _userData;

        [SerializeField, HideInInspector]
        private EditorGraphAsset _graphAsset;

        [NonSerialized]
        private PropertyTree _propertyTree;

        public override string title => "Node";

        /// <summary>
        /// Id
        /// </summary>
        public string id
        {
            get => _id;
            set => _id = value;
        }

        /// <summary>
        /// 位置
        /// </summary>
        public Rect position
        {
            get => _position;
            set => _position = value;
        }

        /// <summary>
        /// 节点是否展开
        /// </summary>
        public bool isExpanded
        {
            get => this._isExpanded;
            set => this._isExpanded = value;
        }

        /// <summary>
        /// 自定义数据
        /// </summary>
        public object userData
        {
            get => _userData;
            set => _userData = value;
        }

        /// <summary>
        /// 所属图表资源
        /// </summary>
        public EditorGraphAsset graphAsset
        {
            get => _graphAsset;
            set => this._graphAsset = value;
        }

        /// <summary>
        /// 自身Odin属性树
        /// </summary>
        public PropertyTree propertyTree
        {
            get
            {
                if (_propertyTree == null) _propertyTree = PropertyTree.Create(this);
                return _propertyTree;
            }
        }

        /// <summary>
        /// 节点Tips
        /// </summary>
        public string tips { get; set; }

        public virtual bool isLogicalTransparent => false;

        protected virtual void OnEnable() { }

        public virtual void SetChildren(List<Object> childAssets) { }
        public virtual List<Object> GetChildren() => null;

        protected virtual void OnDisable()
        {
            _propertyTree?.Dispose();
            _propertyTree = null;
        }

        /// <summary>
        /// 获取逻辑输出连接
        /// </summary>
        public virtual List<EditorLogicalConnection> GetLogicalOutputNodes(HashSet<string> visited = null)
        {
            if (graphAsset == null) return new List<EditorLogicalConnection>();

            List<EditorLogicalConnection> result = new();
            List<EditorEdgeAsset> edges = graphAsset.GetOutputEdges(this);

            for (var i = 0; i < edges.Count; i++)
            {
                EditorEdgeAsset edge = edges[i];
                EditorNodeAsset targetNode = graphAsset.nodeMap.GetValueOrDefault(edge.inputNodeId);
                if (targetNode == null) continue;

                if (targetNode.isLogicalTransparent)
                {
                    HashSet<string> branchVisited = visited == null ? null : new HashSet<string>(visited);
                    List<EditorLogicalConnection> logicalConnections = targetNode.GetLogicalOutputNodes(branchVisited);
                    int count = logicalConnections.Count;
                    for (int j = 0; j < count; j++)
                    {
                        EditorLogicalConnection logicalConnection = logicalConnections[j];
                        result.Add(new EditorLogicalConnection(
                            this,
                            edge.outputPortId,
                            logicalConnection.inputNode,
                            logicalConnection.inputPortId));
                    }

                    continue;
                }

                result.Add(new EditorLogicalConnection(this, edge.outputPortId, targetNode, edge.inputPortId));
            }

            return result;
        }

        /// <summary>
        /// 获取逻辑输入连接
        /// </summary>
        public virtual List<EditorLogicalConnection> GetLogicalInputNodes(HashSet<string> visited = null)
        {
            if (graphAsset == null) return new List<EditorLogicalConnection>();

            List<EditorLogicalConnection> result = new();
            List<EditorEdgeAsset> edges = graphAsset.GetInputEdges(this);

            for (var i = 0; i < edges.Count; i++)
            {
                EditorEdgeAsset edge = edges[i];
                EditorNodeAsset sourceNode = graphAsset.nodeMap.GetValueOrDefault(edge.outputNodeId);
                if (sourceNode == null) continue;

                if (sourceNode.isLogicalTransparent)
                {
                    HashSet<string> branchVisited = visited == null ? null : new HashSet<string>(visited);
                    List<EditorLogicalConnection> logicalConnections = sourceNode.GetLogicalInputNodes(branchVisited);
                    int count = logicalConnections.Count;
                    for (int j = 0; j < count; j++)
                    {
                        EditorLogicalConnection logicalConnection = logicalConnections[j];
                        result.Add(new EditorLogicalConnection(
                            logicalConnection.outputNode,
                            logicalConnection.outputPortId,
                            this,
                            edge.inputPortId));
                    }

                    continue;
                }

                result.Add(new EditorLogicalConnection(sourceNode, edge.outputPortId, this, edge.inputPortId));
            }

            return result;
        }
    }
}
