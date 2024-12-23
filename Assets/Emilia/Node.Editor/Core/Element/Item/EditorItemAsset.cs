using System;
using System.Collections.Generic;
using Emilia.Kit;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor
{
    [Serializable]
    public abstract class EditorItemAsset : TitleAsset, IGraphAsset
    {
        [SerializeField, HideInInspector]
        private string _id;

        [SerializeField, HideInInspector]
        private Rect _position;

        [NonSerialized]
        private PropertyTree _propertyTree;

        public string id
        {
            get => this._id;
            set => this._id = value;
        }

        public Rect position
        {
            get => this._position;
            set => this._position = value;
        }

        public PropertyTree propertyTree => _propertyTree;

        protected virtual void OnEnable()
        {
            if (_propertyTree != null) _propertyTree.Dispose();
            _propertyTree = PropertyTree.Create(this);
        }

        public virtual void SetChildren(List<Object> childAssets) { }

        public virtual List<Object> GetChildren()
        {
            return new List<Object>();
        }

        public virtual void CollectAsset(List<Object> allAssets)
        {
            allAssets.Add(this);
        }

        protected virtual void OnDisable()
        {
            _propertyTree?.Dispose();
            _propertyTree = null;
        }
    }
}