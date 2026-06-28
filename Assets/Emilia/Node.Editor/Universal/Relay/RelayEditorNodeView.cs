using System.Collections.Generic;
using Emilia.Node.Attributes;
using Emilia.Node.Editor;
using Emilia.Kit;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    public class RelayEditorPortView : EditorPortView
    {
        public override bool ContainsPoint(Vector2 localPoint)
        {
            float width = layout.width > 0 ? layout.width : resolvedStyle.width;
            float height = layout.height > 0 ? layout.height : resolvedStyle.height;

            if (width <= 0) width = 16;
            if (height <= 0) height = 16;

            return new Rect(0, 0, width, height).Contains(localPoint);
        }
    }

    [EditorNode(typeof(RelayNodeAsset))]
    public class RelayEditorNodeView : UniversalEditorNodeView,
        IUniversalConnectConstraintNodeView,
        IUniversalConnectionChangedNodeView
    {
        private static readonly Color DefaultColor = new Color(0.68f, 0.68f, 0.68f);

        protected RelayNodeAsset relayAsset;
        protected VisualElement relayBody;
        protected VisualElement relayPortLayer;
        protected VisualElement relayInputContainer;
        protected VisualElement relayOutputContainer;

        public override bool canExpanded => false;
        protected virtual bool enableRestoreRelayContextMenu => false;
        protected virtual IRelayRestoreEdgeDataStrategy restoreRelayEdgeDataStrategy => DefaultRelayEdgeDataStrategy.Instance;

        public override void Initialize(EditorGraphView graphView, EditorNodeAsset asset)
        {
            relayAsset = asset as RelayNodeAsset;
            base.Initialize(graphView, asset);

            StyleSheet relayStyleSheet = ResourceUtility.LoadResource<StyleSheet>("Node/Styles/RelayEditorNodeView.uss");
            if (relayStyleSheet != null) styleSheets.Add(relayStyleSheet);

            SetupRestoreRelayContextMenu();
            RefreshPortFromConnections();
        }

        private void SetupRestoreRelayContextMenu()
        {
            if (enableRestoreRelayContextMenu == false) return;
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            if (enableRestoreRelayContextMenu == false) return;

            RelayHelper.AppendRestoreRelayContextMenu(evt, this, restoreRelayEdgeDataStrategy);
        }

        protected override void InitializeNodeView()
        {
            base.InitializeNodeView();

            AddToClassList("relay-node");
            AddToClassList(GetRelayOrientationClass());

            titleContainer.style.display = DisplayStyle.None;
            mainContainer.style.display = DisplayStyle.None;

            relayBody = new VisualElement {name = "relay-body", pickingMode = PickingMode.Ignore};
            Insert(0, relayBody);

            relayPortLayer = new VisualElement {name = "relay-port-layer"};
            relayInputContainer = new VisualElement {name = "relay-input-container"};
            relayOutputContainer = new VisualElement {name = "relay-output-container"};

            relayPortLayer.Add(relayInputContainer);
            relayPortLayer.Add(relayOutputContainer);
            Add(relayPortLayer);
        }

        public override IEditorPortView AddPortView(int index, EditorPortInfo info)
        {
            IEditorPortView portView = base.AddPortView(index, info);
            portView.portElement.portName = string.Empty;
            portView.portElement.AddToClassList("relay-port");
            portView.portElement.AddToClassList(
                info.direction == EditorPortDirection.Input ? "relay-port-input" : "relay-port-output");
            AttachRelayPort(portView);
            return portView;
        }

        protected override void AddCustomPortView(int index, IEditorPortView portView, EditorPortInfo info)
        {
            AttachRelayPort(portView);
        }

        public override List<EditorPortInfo> CollectStaticPortAssets()
        {
            List<EditorPortInfo> portInfos = new();
            Color color = GetRelayPortColor();
            EditorOrientation orientation = relayAsset?.portOrientation ?? EditorOrientation.Horizontal;

            portInfos.Add(CreateRelayPortInfo(RelayHelper.InputPortId, EditorPortDirection.Input, orientation, color));
            portInfos.Add(CreateRelayPortInfo(RelayHelper.OutputPortId, EditorPortDirection.Output, orientation, color));

            return portInfos;
        }

        private static EditorPortInfo CreateRelayPortInfo(
            string id,
            EditorPortDirection direction,
            EditorOrientation orientation,
            Color color)
        {
            return new EditorPortInfo
            {
                id = id,
                displayName = string.Empty,
                portType = typeof(object),
                direction = direction,
                orientation = orientation,
                nodePortViewType = typeof(RelayEditorPortView),
                canMultiConnect = true,
                color = color
            };
        }

        public virtual bool CanConnect(UniversalConnectContext context)
        {
            if (context == null) return false;

            bool hasRelayInput = TryGetRelayInputCandidate(context, out IEditorPortView sourceCandidate);
            bool hasRelayOutput = TryGetRelayOutputCandidate(context, out IEditorPortView targetCandidate);
            if (hasRelayInput == false && hasRelayOutput == false) return true;
            if (hasRelayInput && hasRelayOutput) return false;

            List<IEditorPortView> sourceOutputs = RelayHelper.ResolveSourceOutputPorts(sourceCandidate);
            List<IEditorPortView> targetInputs = RelayHelper.ResolveTargetInputPorts(targetCandidate);

            if (hasRelayInput && CanConnectAsRelayInput(context, sourceOutputs) == false) return false;
            if (hasRelayOutput && CanConnectAsRelayOutput(context, targetInputs) == false) return false;

            return true;
        }

        private bool TryGetRelayInputCandidate(UniversalConnectContext context, out IEditorPortView sourceCandidate)
        {
            sourceCandidate = null;

            if (RelayHelper.IsRelayInputPort(context.inputPort) && context.inputPort.master == this)
            {
                sourceCandidate = context.outputPort;
                return true;
            }

            if (RelayHelper.IsRelayInputPort(context.outputPort) && context.outputPort.master == this)
            {
                sourceCandidate = context.inputPort;
                return true;
            }

            return false;
        }

        private bool TryGetRelayOutputCandidate(UniversalConnectContext context, out IEditorPortView targetCandidate)
        {
            targetCandidate = null;

            if (RelayHelper.IsRelayOutputPort(context.inputPort) && context.inputPort.master == this)
            {
                targetCandidate = context.outputPort;
                return true;
            }

            if (RelayHelper.IsRelayOutputPort(context.outputPort) && context.outputPort.master == this)
            {
                targetCandidate = context.inputPort;
                return true;
            }

            return false;
        }

        public virtual bool CanConnectRelayInput(UniversalConnectContext context, IEditorPortView sourceOutput)
        {
            if (sourceOutput == null) return false;
            return context.IsSourcePort(sourceOutput);
        }

        public virtual bool CanConnectRelayOutput(UniversalConnectContext context, IEditorPortView targetInput)
        {
            if (targetInput == null) return false;
            return context.IsTargetPort(targetInput);
        }

        public virtual bool CanConnectRelayThrough(
            UniversalConnectContext context,
            IEditorPortView sourceOutput,
            IEditorPortView targetInput)
        {
            if (sourceOutput == null || targetInput == null) return false;
            return context.CanConnectByDirectionAndType(targetInput, sourceOutput);
        }

        public virtual void AfterConnect(UniversalConnectContext context, IEditorEdgeView edgeView)
        {
            RelayHelper.RefreshRelayViews(graphView);
        }

        public virtual void AfterDisconnect(EditorGraphView graphView, EditorEdgeAsset edgeAsset)
        {
            RelayHelper.RefreshRelayViews(graphView);
        }

        private bool CanConnectAsRelayInput(UniversalConnectContext context, List<IEditorPortView> sourceOutputs)
        {
            int sourceCount = sourceOutputs.Count;
            for (int i = 0; i < sourceCount; i++)
            {
                if (CanConnectRelayInput(context, sourceOutputs[i]) == false) return false;
            }

            List<IEditorPortView> relayTargetInputs = RelayHelper.GetConnectedTargetInputs(this);
            if (relayTargetInputs.Count == 0) return true;
            if (ContainsRelayPort(sourceOutputs)) return true;

            return CanConnectResolvedThrough(context, sourceOutputs, relayTargetInputs);
        }

        private bool CanConnectAsRelayOutput(UniversalConnectContext context, List<IEditorPortView> targetInputs)
        {
            int targetCount = targetInputs.Count;
            for (int i = 0; i < targetCount; i++)
            {
                if (CanConnectRelayOutput(context, targetInputs[i]) == false) return false;
            }

            List<IEditorPortView> relaySourceOutputs = RelayHelper.GetConnectedSourceOutputs(this);
            if (relaySourceOutputs.Count == 0) return true;
            if (ContainsRelayPort(targetInputs)) return true;

            return CanConnectResolvedThrough(context, relaySourceOutputs, targetInputs);
        }

        private bool CanConnectResolvedThrough(
            UniversalConnectContext context,
            List<IEditorPortView> sourceOutputs,
            List<IEditorPortView> targetInputs)
        {
            int sourceCount = sourceOutputs.Count;
            for (int i = 0; i < sourceCount; i++)
            {
                int targetCount = targetInputs.Count;
                for (int j = 0; j < targetCount; j++)
                {
                    if (CanConnectRelayThrough(context, sourceOutputs[i], targetInputs[j]) == false) return false;
                }
            }

            return true;
        }

        private static bool ContainsRelayPort(List<IEditorPortView> portViews)
        {
            int portCount = portViews.Count;
            for (int i = 0; i < portCount; i++)
            {
                if (RelayHelper.IsRelayPort(portViews[i])) return true;
            }

            return false;
        }

        public void RefreshPortFromConnections()
        {
            SetColor(GetRelayPortColor());
            UpdateRelayPortColors(topicColor);
        }

        private Color GetRelayPortColor()
        {
            List<IEditorPortView> connectedPorts = RelayHelper.GetConnectedSourceOutputs(this);
            connectedPorts.AddRange(RelayHelper.GetConnectedTargetInputs(this));
            if (connectedPorts.Count == 0) return DefaultColor;

            Color color = connectedPorts[0].info.color;
            int portCount = connectedPorts.Count;
            for (int i = 1; i < portCount; i++)
            {
                if (connectedPorts[i].info.color.Equals(color) == false) return DefaultColor;
            }

            return color != Color.white ? color : DefaultColor;
        }

        private void UpdateRelayPortColors(Color color)
        {
            int portCount = portViews.Count;
            for (int i = 0; i < portCount; i++)
            {
                IEditorPortView portView = portViews[i];
                portView.info.color = color;
                portView.portElement.portColor = color;
                portView.portElement.portName = string.Empty;
            }
        }

        private void AttachRelayPort(IEditorPortView portView)
        {
            if (portView == null || relayInputContainer == null || relayOutputContainer == null) return;

            if (portView.portDirection == EditorPortDirection.Input)
                relayInputContainer.Add(portView.portElement);
            else if (portView.portDirection == EditorPortDirection.Output)
                relayOutputContainer.Add(portView.portElement);
        }

        private string GetRelayOrientationClass()
        {
            return relayAsset?.portOrientation switch
            {
                EditorOrientation.Vertical => "relay-node-vertical",
                EditorOrientation.Custom => "relay-node-custom",
                _ => "relay-node-horizontal"
            };
        }
    }
}
