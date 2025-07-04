using System.Collections.Generic;
using Emilia.Kit;
using Emilia.Node.Editor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    [HideMonoScript, OnValueChanged(nameof(OnValueChanged), true)]
    public class UniversalNodeAsset : EditorNodeAsset, IObjectDescription
    {
        [SerializeField, HideInInspector]
        private string _displayName;

        [SerializeField, HideInInspector]
        private bool _isFold = true;

        /// <summary>
        /// 节点名称
        /// </summary>
        public string displayName
        {
            get => _displayName;
            set => _displayName = value;
        }

        /// <summary>
        /// 是否折叠
        /// </summary>
        public bool isFold
        {
            get => _isFold;
            set => _isFold = value;
        }

        public override string title
        {
            get
            {
                if (string.IsNullOrEmpty(_displayName)) return defaultDisplayName;
                return _displayName;
            }
        }

        protected virtual string defaultDisplayName => "节点";

        protected virtual void OnValueChanged()
        {
            EditorGraphView graphView = EditorGraphView.GetGraphView(graphAsset);
            if (graphView == null) return;
            UniversalEditorNodeView nodeView = graphView.graphElementCache.nodeViewById.GetValueOrDefault(id) as UniversalEditorNodeView;
            if (nodeView != null) nodeView.OnValueChanged();
        }

        public virtual string description
        {
            get
            {
                if (userData == null) return title;

                EditorGraphView graphView = EditorGraphView.GetGraphView(graphAsset);
                if (graphView == null) return title;

                string userDataDescription = ObjectDescriptionUtility.GetDescription(userData, graphView);
                return title + $"({userDataDescription})";
            }
        }
    }
}