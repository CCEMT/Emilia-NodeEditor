using System.Collections.Generic;
using System.Linq;
using Emilia.Kit;
using Emilia.Node.Attributes;
using Emilia.Node.Editor;
using Emilia.Node.Universal.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Example
{
    [NodeMenu("子级演示节点")]
    public class ChildrenDisplayNodeAsset : EditorCapabilitiesDisplayNodeAsset
    {
        [SerializeField, HideInInspector]
        private CapabilitiesDisplayAsset _capabilitiesDisplayAsset;

        public CapabilitiesDisplayAsset capabilitiesDisplayAsset
        {
            get => _capabilitiesDisplayAsset;
            set => _capabilitiesDisplayAsset = value;
        }

        protected override string defaultDisplayName => "子级演示节点";

        public override void SetChildren(List<Object> childAssets)
        {
            _capabilitiesDisplayAsset = null;
            base.SetChildren(childAssets);

            CapabilitiesDisplayAsset asset = childAssets.OfType<CapabilitiesDisplayAsset>().FirstOrDefault();
            if (asset == null) return;

            _capabilitiesDisplayAsset = asset;
            EditorAssetKit.SaveAssetIntoObject(this._capabilitiesDisplayAsset, this);
        }

        public override List<Object> GetChildren()
        {
            List<Object> list = new List<Object>();
            list.Add(this._capabilitiesDisplayAsset);
            return list;
        }
    }

    [EditorNode(typeof(ChildrenDisplayNodeAsset))]
    public class ChildrenDisplayNodeView : EditorCapabilitiesDisplayNodeView
    {
        private ChildrenDisplayNodeAsset childrenDisplayNodeAsset;

        protected override bool canRename => true;

        public override void Initialize(EditorGraphView graphView, EditorNodeAsset asset)
        {
            base.Initialize(graphView, asset);
            this.childrenDisplayNodeAsset = asset as ChildrenDisplayNodeAsset;
            SetColor(new Color(1, 0.5f, 0, 1));

            RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        public override List<EditorPortInfo> CollectStaticPortAssets()
        {
            List<EditorPortInfo> portInfos = new List<EditorPortInfo>();

            UniversalEditorPortInfo leftIntPortInfo = new UniversalEditorPortInfo();
            leftIntPortInfo.id = "leftIntPortInfo";
            leftIntPortInfo.displayName = "左端口(int)";
            leftIntPortInfo.portType = typeof(int);
            leftIntPortInfo.direction = EditorPortDirection.Input;
            leftIntPortInfo.orientation = EditorOrientation.Horizontal;
            leftIntPortInfo.color = Color.yellow;
            leftIntPortInfo.canMultiConnect = true;

            portInfos.Add(leftIntPortInfo);

            UniversalEditorPortInfo rightIntPortInfo = new UniversalEditorPortInfo();
            rightIntPortInfo.id = "rightIntPortInfo";
            rightIntPortInfo.displayName = "右端口(int)";
            rightIntPortInfo.portType = typeof(int);
            rightIntPortInfo.direction = EditorPortDirection.Output;
            rightIntPortInfo.orientation = EditorOrientation.Horizontal;
            rightIntPortInfo.color = Color.green;
            rightIntPortInfo.canMultiConnect = true;

            portInfos.Add(rightIntPortInfo);

            return portInfos;
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.clickCount == 2) graphView.Reload(this.childrenDisplayNodeAsset.capabilitiesDisplayAsset);
        }

        public override void OnValueChanged(bool isSilent = false)
        {
            base.OnValueChanged(isSilent);
            if (childrenDisplayNodeAsset.capabilitiesDisplayAsset == null) return;
            childrenDisplayNodeAsset.capabilitiesDisplayAsset.name = childrenDisplayNodeAsset.title;
        }
    }
}