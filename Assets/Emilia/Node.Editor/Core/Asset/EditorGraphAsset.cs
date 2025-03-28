using System;
using System.Collections.Generic;
using Emilia.Kit;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace Emilia.Node.Editor
{
    [Serializable]
    public partial class EditorGraphAsset : SerializedScriptableObject
    {
        [SerializeField, HideInInspector]
        private string _id;

        [NonSerialized, OdinSerialize, HideInInspector]
        private List<EditorNodeAsset> _nodes = new List<EditorNodeAsset>();

        [NonSerialized, OdinSerialize, HideInInspector]
        private List<EditorEdgeAsset> _edges = new List<EditorEdgeAsset>();

        [NonSerialized, OdinSerialize, HideInInspector]
        private List<EditorItemAsset> _items = new List<EditorItemAsset>();

        [NonSerialized, OdinSerialize, HideInInspector]
        private Dictionary<string, EditorNodeAsset> _nodeMap = new Dictionary<string, EditorNodeAsset>();

        [NonSerialized, OdinSerialize, HideInInspector]
        private Dictionary<string, EditorEdgeAsset> _edgeMap = new Dictionary<string, EditorEdgeAsset>();

        [NonSerialized, OdinSerialize, HideInInspector]
        private Dictionary<string, EditorItemAsset> _itemMap = new Dictionary<string, EditorItemAsset>();

        [NonSerialized]
        private PropertyTree _propertyTree;

        /// <summary>
        /// Id
        /// </summary>
        public string id
        {
            get => _id;
            set => _id = value;
        }

        /// <summary>
        /// 所有Node
        /// </summary>
        public IReadOnlyList<EditorNodeAsset> nodes => _nodes;

        /// <summary>
        /// 所有Edge
        /// </summary>
        public IReadOnlyList<EditorEdgeAsset> edges => _edges;

        /// <summary>
        /// 所有Item
        /// </summary>
        public IReadOnlyList<EditorItemAsset> items => _items;

        /// <summary>
        /// 根据Id获取Node
        /// </summary>
        public IReadOnlyDictionary<string, EditorNodeAsset> nodeMap => _nodeMap;

        /// <summary>
        /// 根据Id获取Edge
        /// </summary>
        public IReadOnlyDictionary<string, EditorEdgeAsset> edgeMap => _edgeMap;

        /// <summary>
        /// 根据Id获取Item
        /// </summary>
        public IReadOnlyDictionary<string, EditorItemAsset> itemMap => _itemMap;

        public PropertyTree propertyTree => _propertyTree;

        protected virtual void Reset()
        {
            _id = Guid.NewGuid().ToString();
            AssetDatabase.SaveAssetIfDirty(this);
        }

        public override string ToString()
        {
            return name;
        }

        protected void OnEnable()
        {
            if (_propertyTree != null) _propertyTree.Dispose();
            _propertyTree = PropertyTree.Create(this);
        }

        /// <summary>
        /// 添加Node
        /// </summary>
        public void AddNode(EditorNodeAsset nodeAsset)
        {
            if (this._nodeMap.ContainsKey(nodeAsset.id)) return;

            nodeAsset.graphAsset = this;

            _nodes.Add(nodeAsset);
            this._nodeMap[nodeAsset.id] = nodeAsset;

            EditorAssetKit.SaveAssetIntoObject(nodeAsset, this);
        }

        /// <summary>
        /// 添加Edge
        /// </summary>
        public void AddEdge(EditorEdgeAsset edgeAsset)
        {
            if (this._edgeMap.ContainsKey(edgeAsset.id)) return;

            edgeAsset.graphAsset = this;

            _edges.Add(edgeAsset);
            this._edgeMap[edgeAsset.id] = edgeAsset;

            EditorAssetKit.SaveAssetIntoObject(edgeAsset, this);
        }

        /// <summary>
        /// 添加Item
        /// </summary>
        public void AddItem(EditorItemAsset itemAsset)
        {
            if (this._itemMap.ContainsKey(itemAsset.id)) return;

            itemAsset.graphAsset = this;

            _items.Add(itemAsset);
            this._itemMap[itemAsset.id] = itemAsset;

            EditorAssetKit.SaveAssetIntoObject(itemAsset, this);
        }

        /// <summary>
        /// 移除Node
        /// </summary>
        public void RemoveNode(EditorNodeAsset nodeAsset)
        {
            if (this._nodeMap.ContainsKey(nodeAsset.id) == false) return;

            nodeAsset.graphAsset = null;

            this._nodes.Remove(nodeAsset);
            this._nodeMap.Remove(nodeAsset.id);
        }

        /// <summary>
        /// 移除Edge
        /// </summary>
        public void RemoveEdge(EditorEdgeAsset edgeAsset)
        {
            if (this._edgeMap.ContainsKey(edgeAsset.id) == false) return;

            edgeAsset.graphAsset = null;

            this._edges.Remove(edgeAsset);
            this._edgeMap.Remove(edgeAsset.id);
        }

        /// <summary>
        /// 移除Item
        /// </summary>
        public void RemoveItem(EditorItemAsset itemAsset)
        {
            if (this._itemMap.ContainsKey(itemAsset.id) == false) return;

            itemAsset.graphAsset = null;

            this._items.Remove(itemAsset);
            this._itemMap.Remove(itemAsset.id);
        }

        protected virtual void OnDisable()
        {
            _propertyTree?.Dispose();
            _propertyTree = null;
        }
    }
}