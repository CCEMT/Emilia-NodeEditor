using System;
using System.Collections.Generic;
using Emilia.Kit;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor
{
    [Serializable]
    public class EditorNodeAsset : TitleAsset, IGraphAsset
    {
        [SerializeField, HideInInspector]
        private string _id;

        [SerializeField, HideInInspector]
        private Rect _position;

        [SerializeField, HideInInspector]
        private object _userData;

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

        public object userData
        {
            get => _userData;
            set => _userData = value;
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

        public virtual void CollectAsset(List<Object> childAssets)
        {
            childAssets.Add(this);
        }

        protected virtual void OnDisable()
        {
            _propertyTree?.Dispose();
            _propertyTree = null;
        }
    }
}