﻿using System.Collections;
using System.Collections.Generic;
using Emilia.Node.Attributes;
using Emilia.Node.Editor;
using Sirenix.Utilities;
using Unity.EditorCoroutines.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    [EditorNode(typeof(UniversalNodeAsset))]
    public class UniversalEditorNodeView : EditorNodeView
    {
        private EditorCoroutine focusCoroutine;
        private VisualElement focusBorder;

        private UniversalNodeAsset _universalNodeAsset;

        private VisualElement horizontalContainer;
        private VisualElement horizontalInputContainer;
        private VisualElement horizontalOutputContainer;

        private VisualElement verticalContainer;
        private VisualElement verticalInputContainer;
        private VisualElement verticalOutputContainer;

        private TextField titleTextField;

        private NodeMessageButtonElement messageButtonElement;
        private NodeMessageContainer messageContainer;
        private List<NodeMessageElement> messageElements = new List<NodeMessageElement>();

        private Dictionary<string, NodeTipsElement> tipsElements = new Dictionary<string, NodeTipsElement>();

        public virtual bool canExpanded => true;

        public override bool expanded
        {
            get => this._universalNodeAsset == null || this._universalNodeAsset.isFold;
            set
            {
                base.expanded = value;
                _universalNodeAsset.isFold = value;
                UpdateFoldState();
            }
        }

        protected virtual bool canRename => false;
        public virtual string defaultDisplayName => "节点";
        protected virtual string iconPath => null;
        protected override string styleFilePath => "Node/Styles/UniversalEditorNodeView.uss";

        public override void Initialize(EditorGraphView graphView, EditorNodeAsset asset)
        {
            base.Initialize(graphView, asset);
            this._universalNodeAsset = asset as UniversalNodeAsset;

            if ((capabilities & Capabilities.Renamable) != 0) InitializeRenamableTitle();

            UpdateTitle();
            UpdateFoldState();
        }

        protected override void InitializeNodeView()
        {
            base.InitializeNodeView();

            if (canRename) capabilities |= Capabilities.Renamable;

            InitializeIcon();
            InitializeExpandButton();
            InitializeFocusBorder();
            InitializeMessage();
        }

        protected void SetNodeColor(object nodeAsset)
        {
            if (nodeAsset == null) return;
            NodeColorAttribute flowNodeColor = nodeAsset.GetType().GetCustomAttribute<NodeColorAttribute>(true);
            if (flowNodeColor == null) return;
            SetColor(new Color(flowNodeColor.r, flowNodeColor.g, flowNodeColor.b));
        }

        private void InitializeExpandButton()
        {
            if (canExpanded) return;

            VisualElement expandButton = this.Q("collapse-button");
            expandButton.style.display = DisplayStyle.None;
        }

        private void InitializeIcon()
        {
            if (string.IsNullOrEmpty(iconPath)) return;
            Texture2D icon = ResourceUtility.LoadResource<Texture2D>(styleFilePath);
            VisualElement iconElement = new VisualElement();
            iconElement.style.backgroundImage = icon;
            titleContainer.Insert(0, iconElement);
        }

        private void InitializeFocusBorder()
        {
            const float borderWidth = 2;

            focusBorder = new VisualElement();
            focusBorder.pickingMode = PickingMode.Ignore;
            focusBorder.name = "focus-border";

            this.RegisterCallback<GeometryChangedEvent>(_ => {
                focusBorder.style.width = this.layout.width + borderWidth;
                focusBorder.style.height = this.layout.height + borderWidth;
            });

            Add(focusBorder);
        }

        private void InitializeRenamableTitle()
        {
            this.titleTextField = new TextField();
            titleTextField.style.flexGrow = 1;
            titleTextField.style.display = DisplayStyle.None;
            titleTextField.style.width = new StyleLength(StyleKeyword.Auto);
            titleTextField.style.maxWidth = 300;

            titleContainer.Insert(0, this.titleTextField);

            titleLabel.RegisterCallback<MouseDownEvent>(e => {
                if (e.clickCount == 2 && e.button == 0) StartRenaming();
            });

            titleTextField.RegisterCallback<FocusOutEvent>(_ => EndRenaming(titleTextField.value));

            void StartRenaming()
            {
                titleTextField.style.minWidth = titleLabel.layout.width;
                titleTextField.style.display = DisplayStyle.Flex;
                titleLabel.style.display = DisplayStyle.None;
                titleTextField.focusable = true;

                titleTextField.SetValueWithoutNotify(title);
                titleTextField.Focus();
                titleTextField.SelectAll();
            }

            void EndRenaming(string newName)
            {
                titleTextField.style.display = DisplayStyle.None;
                titleLabel.style.display = DisplayStyle.Flex;
                titleTextField.focusable = false;

                RegisterCompleteObjectUndo("Graph Rename");
                _universalNodeAsset.displayName = newName;

                UpdateTitle();
            }
        }

        private void InitializeMessage()
        {
            messageButtonElement = new NodeMessageButtonElement(SwitchMessageContainerState);
            messageButtonElement.style.display = DisplayStyle.None;

            titleButtonContainer.Add(messageButtonElement);

            messageContainer = new NodeMessageContainer();
            messageContainer.style.display = DisplayStyle.None;

            topLayerContainer.Add(messageContainer);
        }

        private void SwitchMessageContainerState()
        {
            if (messageContainer.style.display == DisplayStyle.None) messageContainer.style.display = DisplayStyle.Flex;
            else messageContainer.style.display = DisplayStyle.None;
        }

        public override void OnValueChanged()
        {
            base.OnValueChanged();
            UpdateTitle();
        }

        public void UpdateTitle()
        {
            if (string.IsNullOrEmpty(this._universalNodeAsset.displayName)) title = defaultDisplayName;
            else title = this._universalNodeAsset.displayName;
        }

        public override List<EditorPortInfo> CollectStaticPortAssets()
        {
            return new List<EditorPortInfo>();
        }

        public override IEditorPortView AddPortView(EditorPortInfo info)
        {
            IEditorPortView portView = base.AddPortView(info);

            switch (portView.editorOrientation)
            {
                case EditorOrientation.Horizontal:
                    if (horizontalContainer == null) CreateHorizontalContainer();
                    if (portView.portDirection == EditorPortDirection.Input) this.horizontalInputContainer.Add(portView.portElement);
                    else if (portView.portDirection == EditorPortDirection.Output) this.horizontalOutputContainer.Add(portView.portElement);
                    break;
                case EditorOrientation.Vertical:
                    if (verticalContainer == null) CreateVerticalContainer();
                    if (portView.portDirection == EditorPortDirection.Input) this.verticalInputContainer.Add(portView.portElement);
                    else if (portView.portDirection == EditorPortDirection.Output) this.verticalOutputContainer.Add(portView.portElement);
                    break;
                case EditorOrientation.Custom:
                    AddCustomPortView(portView, info);
                    break;
            }

            return portView;
        }

        private void CreateHorizontalContainer()
        {
            NodeHorizontalContainer nodeHorizontalContainer = new NodeHorizontalContainer();
            this.horizontalContainer = nodeHorizontalContainer;
            this.horizontalInputContainer = nodeHorizontalContainer.inputContainer;
            this.horizontalOutputContainer = nodeHorizontalContainer.outputContainer;
            portNodeBottomContainer.Add(nodeHorizontalContainer);
        }

        private void CreateVerticalContainer()
        {
            NodeVerticalContainer inputVerticalContainer = new NodeVerticalContainer();
            this.verticalInputContainer = inputVerticalContainer;
            nodeTopContainer.Add(inputVerticalContainer);

            NodeVerticalContainer outputVerticalContainer = new NodeVerticalContainer();
            this.verticalOutputContainer = outputVerticalContainer;
            nodeBottomContainer.Add(outputVerticalContainer);

            this.verticalContainer = outputVerticalContainer;
        }

        protected virtual void AddCustomPortView(IEditorPortView portView, EditorPortInfo info) { }

        public virtual void UpdateFoldState() { }

        public NodeMessageElement SendMessage(string message, NodeMessageLevel level)
        {
            NodeMessageElement nodeMessageElement = new NodeMessageElement();
            nodeMessageElement.Init(message, level);

            nodeMessageElement.onRemove += () => RemoveMessage(nodeMessageElement);

            messageElements.Add(nodeMessageElement);
            messageContainer.Add(nodeMessageElement);

            UpdateMessageButtonState();

            return nodeMessageElement;
        }

        private void RemoveMessage(NodeMessageElement nodeMessageElement)
        {
            if (this.messageElements.Remove(nodeMessageElement) == false) return;
            nodeMessageElement.RemoveFromHierarchy();
            UpdateMessageButtonState();
        }

        private void UpdateMessageButtonState()
        {
            if (this.messageElements.Count == 0) this.messageButtonElement.style.display = DisplayStyle.None;
            else
            {
                NodeMessageLevel maxLevel = GetMaxLevel();
                this.messageButtonElement.SetLevel(maxLevel);

                this.messageButtonElement.style.display = DisplayStyle.Flex;
            }
        }

        private NodeMessageLevel GetMaxLevel()
        {
            NodeMessageLevel maxLevel = NodeMessageLevel.Info;
            int count = messageElements.Count;
            for (int i = 0; i < count; i++)
            {
                NodeMessageElement messageElement = messageElements[i];
                if (messageElement.level > maxLevel) maxLevel = messageElement.level;
            }

            return maxLevel;
        }

        public void Tips(string text, long timeMs = 1500, float speed = 10)
        {
            if (tipsElements.ContainsKey(text)) return;

            NodeTipsElement nodeTipsElement = new NodeTipsElement();
            nodeTipsElement.text = text;

            tipsElements[text] = nodeTipsElement;

            schedule.Execute(() => {

                Add(nodeTipsElement);
                Vector3 titleCenter = titleContainer.layout.center;
                nodeTipsElement.Init(speed, titleCenter);

            }).ExecuteLater(1);

            schedule.Execute(() => RemoveTips(text)).ExecuteLater(timeMs);
        }

        public void RemoveTips(string text)
        {
            if (this.tipsElements.Remove(text, out NodeTipsElement nodeTipsElement) == false) return;
            nodeTipsElement.RemoveFromHierarchy();
        }

        public void ClearTips()
        {
            foreach (NodeTipsElement tipsElement in tipsElements.Values) tipsElement.RemoveFromHierarchy();
            tipsElements.Clear();
        }

        public void SetFocus(Color borderColor, long timeMs = -1)
        {
            schedule.Execute(OnSetFocus).ExecuteLater(1);

            void OnSetFocus()
            {
                if (focusCoroutine != null) EditorCoroutineUtility.StopCoroutine(focusCoroutine);

                focusBorder.AddToClassList("enable");

                focusBorder.style.borderTopColor = borderColor;
                focusBorder.style.borderRightColor = borderColor;
                focusBorder.style.borderBottomColor = borderColor;
                focusBorder.style.borderLeftColor = borderColor;

                if (timeMs > 0) focusCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(OnFadeAway(timeMs));
            }
        }

        private IEnumerator OnFadeAway(long timeMs)
        {
            float time = timeMs / 1000f;
            float startTime = Time.realtimeSinceStartup;

            while (Time.realtimeSinceStartup - startTime < time)
            {
                float t = (Time.realtimeSinceStartup - startTime) / time;
                float alpha = 1 - t;

                SetFocusBorderAlpha(alpha);

                yield return 0;
            }

            void SetFocusBorderAlpha(float alpha)
            {
                Color borderTopColor = focusBorder.style.borderTopColor.value;
                borderTopColor.a = alpha;
                focusBorder.style.borderTopColor = borderTopColor;

                Color borderRightColor = focusBorder.style.borderRightColor.value;
                borderRightColor.a = alpha;
                focusBorder.style.borderRightColor = borderRightColor;

                Color borderBottomColor = focusBorder.style.borderBottomColor.value;
                borderBottomColor.a = alpha;
                focusBorder.style.borderBottomColor = borderBottomColor;

                Color borderLeftColor = focusBorder.style.borderLeftColor.value;
                borderLeftColor.a = alpha;
                focusBorder.style.borderLeftColor = borderLeftColor;
            }

            focusCoroutine = null;
            ClearFocus();
        }

        public void ClearFocus()
        {
            schedule.Execute(OnClearFocus).ExecuteLater(1);

            void OnClearFocus()
            {
                if (focusCoroutine != null) EditorCoroutineUtility.StopCoroutine(focusCoroutine);
                focusBorder.RemoveFromClassList("enable");
            }
        }
    }
}