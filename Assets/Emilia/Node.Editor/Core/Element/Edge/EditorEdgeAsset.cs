using System;
using System.Collections.Generic;
using Emilia.Kit;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor
{
    [Serializable]
    public class EditorEdgeAsset : TitleAsset, IGraphAsset
    {
        [SerializeField, HideInInspector]
        private string _id;

        [SerializeField, HideInInspector]
        private string _outputNodeId;

        [SerializeField, HideInInspector]
        private string _inputNodeId;

        [SerializeField, HideInInspector]
        private string _outputPortId;

        [SerializeField, HideInInspector]
        private string _inputPortId;

        [SerializeField, HideInInspector]
        private object _userData;

        [NonSerialized]
        private PropertyTree _propertyTree;

        public override string title => "Edge";

        public string id
        {
            get => _id;
            set => _id = value;
        }

        public string outputNodeId
        {
            get => _outputNodeId;
            set => _outputNodeId = value;
        }

        public string inputNodeId
        {
            get => _inputNodeId;
            set => _inputNodeId = value;
        }

        public string outputPortId
        {
            get => _outputPortId;
            set => _outputPortId = value;
        }

        public string inputPortId
        {
            get => _inputPortId;
            set => _inputPortId = value;
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