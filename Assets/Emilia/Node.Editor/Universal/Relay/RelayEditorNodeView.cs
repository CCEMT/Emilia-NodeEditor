using System.Collections.Generic;
using Emilia.Node.Attributes;
using Emilia.Node.Editor;
using Emilia.Kit;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    [EditorNode(typeof(RelayNodeAsset))]
    public class RelayEditorNodeView : UniversalEditorNodeView,
        IUniversalConnectConstraintNodeView,
        IUniversalConnectionChangedNodeView
    {
        private static readonly Color DefaultColor = new Color(0.68f, 0.68f, 0.68f);

        protected RelayNodeAsset relayAsset;

        public override bool canExpanded => false;

        public override void Initialize(EditorGraphView graphView, EditorNodeAsset asset)
        {
            relayAsset = asset as RelayNodeAsset;
            base.Initialize(graphView, asset);

            AddToClassList("relay-node");
            StyleSheet relayStyleSheet = ResourceUtility.LoadResource<StyleSheet>("Node/Styles/RelayEditorNodeView.uss");
            if (relayStyleSheet != null) styleSheets.Add(relayStyleSheet);

            titleContainer.style.display = DisplayStyle.None;
            RefreshPortFromConnections();
        }

        public override IEditorPortView AddPortView(int index, EditorPortInfo info)
        {
            IEditorPortView portView = base.AddPortView(index, info);
            portView.portElement.portName = string.Empty;
            return portView;
        }

        public override List<EditorPortInfo> CollectStaticPortAssets()
        {
            var portInfos = new List<EditorPortInfo>();
            var (_, color) = GetPortDisplayInfo();
            EditorOrientation orientation = relayAsset?.portOrientation ?? EditorOrientation.Horizontal;

            portInfos.Add(new EditorPortInfo
            {
                id = RelayHelper.InputPortId,
                displayName = string.Empty,
                portType = typeof(object),
                direction = EditorPortDirection.Input,
                orientation = orientation,
                canMultiConnect = true,
                color = color
            });

            portInfos.Add(new EditorPortInfo
            {
                id = RelayHelper.OutputPortId,
                displayName = string.Empty,
                portType = typeof(object),
                direction = EditorPortDirection.Output,
                orientation = orientation,
                canMultiConnect = true,
                color = color
            });

            return portInfos;
        }

        public virtual bool CanConnect(UniversalConnectContext context)
        {
            if (context == null) return false;

            bool isInputRelay = context.inputPort.master == this;
            bool isOutputRelay = context.outputPort.master == this;
            if (isInputRelay == false && isOutputRelay == false) return true;
            if (isInputRelay && isOutputRelay) return false;

            List<IEditorPortView> sourceOutputs = RelayHelper.ResolveSourceOutputPorts(context.outputPort);
            List<IEditorPortView> targetInputs = RelayHelper.ResolveTargetInputPorts(context.inputPort);

            if (isInputRelay && CanConnectAsRelayInput(context, sourceOutputs) == false) return false;
            if (isOutputRelay && CanConnectAsRelayOutput(context, targetInputs) == false) return false;

            return true;
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
            var (_, color) = GetPortDisplayInfo();
            SetColor(color != Color.white ? color : DefaultColor);
            UpdateRelayPortColors(topicColor);
        }

        private (string displayName, Color color) GetPortDisplayInfo()
        {
            List<IEditorPortView> connectedPorts = RelayHelper.GetConnectedSourceOutputs(this);
            connectedPorts.AddRange(RelayHelper.GetConnectedTargetInputs(this));
            if (connectedPorts.Count == 0) return (string.Empty, Color.white);

            Color color = connectedPorts[0].info.color;
            int portCount = connectedPorts.Count;
            for (int i = 1; i < portCount; i++)
            {
                if (connectedPorts[i].info.color.Equals(color) == false) return (string.Empty, DefaultColor);
            }

            return (string.Empty, color);
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
    }
}
