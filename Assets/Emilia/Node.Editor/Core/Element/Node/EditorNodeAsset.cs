using System;
using System.Collections.Generic;
using Emilia.Kit;
using Emilia.Kit.Editor;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor
{
    [Serializable, SelectedClear]
    public class EditorNodeAsset : TitleAsset, IUnityAsset
    {
        [SerializeField, HideInInspector]
        private string _id;

        [SerializeField, HideInInspector]
        private Rect _position;

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

        public object userData
        {
            get => _userData;
            set => _userData = value;
        }

        public EditorGraphAsset graphAsset
        {
            get => _graphAsset;
            set => this._graphAsset = value;
        }

        public PropertyTree propertyTree => _propertyTree;

        protected virtual void OnEnable()
        {
            if (_propertyTree != null) _propertyTree.Dispose();
            _propertyTree = PropertyTree.Create(this);
        }

        public virtual void SetChildren(List<Object> childAssets) { }
        public virtual List<Object> GetChildren() => null;

        protected virtual void OnDisable()
        {
            _propertyTree?.Dispose();
            _propertyTree = null;
        }
    }
}