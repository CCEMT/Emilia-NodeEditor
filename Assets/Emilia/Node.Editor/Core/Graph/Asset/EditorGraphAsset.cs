using System;
using System.Collections.Generic;
using Emilia.Kit;
using Emilia.Kit.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor
{
    [Serializable]
    public class EditorGraphAsset : SerializedScriptableObject, IGraphAsset
    {
        [SerializeField, HideInInspector]
        private string _id;

        [NonSerialized, OdinSerialize, HideInInspector]
        private EditorGraphAsset _parent;

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

        public string id
        {
            get => _id;
            set => _id = value;
        }

        public EditorGraphAsset parent
        {
            get => _parent;
            set => _parent = value;
        }

        public IReadOnlyList<EditorNodeAsset> nodes => _nodes;
        public IReadOnlyList<EditorEdgeAsset> edges => _edges;
        public IReadOnlyList<EditorItemAsset> items => _items;

        public IReadOnlyDictionary<string, EditorNodeAsset> nodeMap => _nodeMap;
        public IReadOnlyDictionary<string, EditorEdgeAsset> edgeMap => _edgeMap;
        public IReadOnlyDictionary<string, EditorItemAsset> itemMap => _itemMap;

        public PropertyTree propertyTree => _propertyTree;

        protected virtual void Reset()
        {
            _id = Guid.NewGuid().ToString();
        }

        protected void OnEnable()
        {
            if (_propertyTree != null) _propertyTree.Dispose();
            _propertyTree = PropertyTree.Create(this);
        }

        public void AddNode(EditorNodeAsset nodeAsset)
        {
            if (this._nodeMap.ContainsKey(nodeAsset.id)) return;

            _nodes.Add(nodeAsset);
            this._nodeMap[nodeAsset.id] = nodeAsset;

            EditorAssetKit.SaveAssetIntoObject(nodeAsset, this);
        }

        public void AddEdge(EditorEdgeAsset edgeAsset)
        {
            if (this._edgeMap.ContainsKey(edgeAsset.id)) return;

            _edges.Add(edgeAsset);
            this._edgeMap[edgeAsset.id] = edgeAsset;

            EditorAssetKit.SaveAssetIntoObject(edgeAsset, this);
        }

        public void AddItem(EditorItemAsset itemAsset)
        {
            if (this._itemMap.ContainsKey(itemAsset.id)) return;

            _items.Add(itemAsset);
            this._itemMap[itemAsset.id] = itemAsset;

            EditorAssetKit.SaveAssetIntoObject(itemAsset, this);
        }

        public void RemoveNode(EditorNodeAsset nodeAsset)
        {
            if (this._nodeMap.ContainsKey(nodeAsset.id) == false) return;

            this._nodes.Remove(nodeAsset);
            this._nodeMap.Remove(nodeAsset.id);
        }

        public void RemoveEdge(EditorEdgeAsset edgeAsset)
        {
            if (this._edgeMap.ContainsKey(edgeAsset.id) == false) return;

            this._edges.Remove(edgeAsset);
            this._edgeMap.Remove(edgeAsset.id);
        }

        public void RemoveItem(EditorItemAsset itemAsset)
        {
            if (this._itemMap.ContainsKey(itemAsset.id) == false) return;

            this._items.Remove(itemAsset);
            this._itemMap.Remove(itemAsset.id);
        }

        public virtual void SetChildren(List<Object> childAssets)
        {
            _nodes.Clear();
            _edges.Clear();
            _items.Clear();

            this._nodeMap.Clear();
            this._edgeMap.Clear();
            this._itemMap.Clear();

            for (var i = 0; i < childAssets.Count; i++)
            {
                Object childAsset = childAssets[i];

                switch (childAsset)
                {
                    case EditorNodeAsset node:
                        this.AddNode(node);
                        break;
                    case EditorEdgeAsset edge:
                        this.AddEdge(edge);
                        break;
                    case EditorItemAsset item:
                        this.AddItem(item);
                        break;
                }
            }
        }

        public virtual List<Object> GetChildren()
        {
            List<Object> assets = new List<Object>();

            for (var i = 0; i < this._nodes.Count; i++)
            {
                EditorNodeAsset node = this._nodes[i];
                assets.Add(node);
            }

            for (var i = 0; i < this._edges.Count; i++)
            {
                EditorEdgeAsset edge = this._edges[i];
                assets.Add(edge);
            }

            for (var i = 0; i < this._items.Count; i++)
            {
                EditorItemAsset item = this._items[i];
                assets.Add(item);
            }

            return assets;
        }

        public virtual void CollectAsset(List<Object> allAssets)
        {
            allAssets.Add(this);

            for (var i = 0; i < this._nodes.Count; i++)
            {
                EditorNodeAsset node = this._nodes[i];
                node.CollectAsset(allAssets);
            }

            for (var i = 0; i < this._edges.Count; i++)
            {
                EditorEdgeAsset edge = this._edges[i];
                edge.CollectAsset(allAssets);
            }

            for (var i = 0; i < this._items.Count; i++)
            {
                EditorItemAsset item = this._items[i];
                item.CollectAsset(allAssets);
            }
        }

        protected virtual void OnDisable()
        {
            _propertyTree?.Dispose();
            _propertyTree = null;
        }
    }
}