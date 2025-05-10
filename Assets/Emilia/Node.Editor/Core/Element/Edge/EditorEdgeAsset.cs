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
    public class EditorEdgeAsset : TitleAsset, IUnityAsset
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

        [SerializeField, HideInInspector]
        private EditorGraphAsset _graphAsset;

        [NonSerialized]
        private PropertyTree _propertyTree;

        public override string title => "Edge";

        /// <summary>
        /// Id 唯一标识
        /// </summary>
        public string id
        {
            get => _id;
            set => _id = value;
        }

        /// <summary>
        /// 输出节点ID
        /// </summary>
        public string outputNodeId
        {
            get => _outputNodeId;
            set => _outputNodeId = value;
        }

        /// <summary>
        /// 输入节点ID
        /// </summary>
        public string inputNodeId
        {
            get => _inputNodeId;
            set => _inputNodeId = value;
        }

        /// <summary>
        /// 输出端口ID
        /// </summary>
        public string outputPortId
        {
            get => _outputPortId;
            set => _outputPortId = value;
        }

        /// <summary>
        /// 输入端口ID
        /// </summary>
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

        public EditorGraphAsset graphAsset
        {
            get => _graphAsset;
            set => _graphAsset = value;
        }

        /// <summary>
        /// Odin属性树
        /// </summary>
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