using System.Collections.Generic;
using Emilia.Node.Editor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    [HideMonoScript, OnValueChanged(nameof(OnValueChanged), true)]
    public class UniversalNodeAsset : EditorNodeAsset
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

        public override string title => NodeViewDefaultTitle();

        protected virtual void OnValueChanged()
        {
            EditorGraphView graphView = EditorGraphView.focusedGraphView;
            if (graphView == null) return;
            UniversalEditorNodeView nodeView = graphView.graphElementCache.nodeViewById.GetValueOrDefault(id) as UniversalEditorNodeView;
            if (nodeView != null) nodeView.OnValueChanged();
        }

        protected virtual string NodeViewDefaultTitle()
        {
            EditorGraphView graphView = EditorGraphView.focusedGraphView;
            if (graphView == null) return base.title;
            UniversalEditorNodeView nodeView = graphView.graphElementCache.nodeViewById.GetValueOrDefault(id) as UniversalEditorNodeView;
            if (nodeView != null) return nodeView.defaultDisplayName;
            return base.title;
        }
    }
}