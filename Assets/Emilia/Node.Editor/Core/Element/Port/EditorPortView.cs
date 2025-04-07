using System;
using System.Collections.Generic;
using System.Linq;
using Emilia.Kit;
using Emilia.Kit.Editor;
using Emilia.Reflection.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor
{
    public class EditorPortView : Port_Internals, IEditorPortView, ICollectibleElement
    {
        private List<IEditorEdgeView> _edges = new List<IEditorEdgeView>();

        public EditorPortInfo info { get; private set; }
        public IEditorNodeView master { get; private set; }

        public virtual EditorPortDirection portDirection => info.direction;
        public virtual EditorOrientation editorOrientation => info.orientation;
        public Port portElement => this;
        public bool isSelected { get; protected set; }
        public IReadOnlyList<IEditorEdgeView> edges => _edges;

        protected virtual string portStyleFilePath => "Node/Styles/UniversalEditorPortView.uss";

        public event Action<IEditorPortView, IEditorEdgeView> onConnected;
        public event Action<IEditorPortView, IEditorEdgeView> OnDisconnected;

        public EditorPortView() : base(default, default, default, null) { }

        public virtual void Initialize(IEditorNodeView master, EditorPortInfo info)
        {
            this.info = info;
            this.master = master;

            orientation_Internals = info.orientation == EditorOrientation.Horizontal ? Orientation.Horizontal : Orientation.Vertical;
            direction_Internals = info.direction == EditorPortDirection.Input ? Direction.Input : Direction.Output;
            capacity_Internals = info.canMultiConnect ? Capacity.Multi : Capacity.Single;
            portName = info.displayName;
            portType = info.portType;

            if (portType != null) visualClass = "Port_" + portType.Name;

            Type edgeAssetType = master.graphView.connectSystem.GetEdgeTypeByPort(this);
            Type edgeViewType = GraphTypeCache.GetEdgeViewType(edgeAssetType);

            EditorEdgeConnectorListener connectorListener = master.graphView.connectSystem.connectorListener;

            EditorEdgeConnector connector = ReflectUtility.CreateInstance(info.edgeConnectorType) as EditorEdgeConnector;
            connector.Initialize(edgeViewType, connectorListener);

            this.m_EdgeConnector = connector;
            this.AddManipulator(connector);

            StyleSheet portStyle = ResourceUtility.LoadResource<StyleSheet>(portStyleFilePath);
            styleSheets.Add(portStyle);

            m_ConnectorText.pickingMode = PickingMode.Position;
            m_ConnectorText.style.flexGrow = 1;

            if (info.orientation == EditorOrientation.Vertical) AddToClassList("Vertical");

            portColor = info.color;

            capabilities |= Capabilities.Copiable;

            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(OnContextualMenuManipulator);
            this.AddManipulator(contextualMenuManipulator);
        }

        private void OnContextualMenuManipulator(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction($"Copy {info.displayName} Connect", (_) => OnCopyConnect());
            evt.menu.AppendAction($"Paste Connect To {info.displayName}", (_) => OnPasteConnect(), CanPaste() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

            evt.menu.AppendSeparator();
        }

        protected virtual void OnCopyConnect()
        {
            master.graphView.ClearSelection();
            master.graphView.AddToSelection(this);
            master.graphView.graphOperate.Copy();
        }

        protected virtual bool CanPaste()
        {
            bool canPaste = master.graphView.graphCopyPaste.CanPasteSerializedDataCallback(master.graphView.GetSerializedData_Internal());
            if (canPaste == false) return false;
            IEditorEdgeView editorEdgeView = master.graphView.graphCopyPaste.GetCopyGraphElements(master.graphView.GetSerializedData_Internal()).OfType<IEditorEdgeView>().FirstOrDefault();
            if (editorEdgeView == null) return false;
            return true;
        }

        protected virtual void OnPasteConnect()
        {
            master.graphView.ClearSelection();
            master.graphView.AddToSelection(this);
            master.graphView.graphOperate.Paste();
        }

        private void OnContextualMenuPopulateEvent(ContextualMenuPopulateEvent evt)
        {
            Debug.Log("cc");
        }

        public override void Connect(Edge edge)
        {
            base.Connect(edge);
            IEditorEdgeView editorEdge = edge as IEditorEdgeView;
            if (editorEdge == null)
            {
                Debug.LogError($"{nameof(Edge)}必须继承{nameof(IEditorEdgeView)}");
                return;
            }

            _edges.Add(editorEdge);
            onConnected?.Invoke(this, editorEdge);
        }

        public override void Disconnect(Edge edge)
        {
            base.Disconnect(edge);
            IEditorEdgeView editorEdge = edge as IEditorEdgeView;
            if (editorEdge == null)
            {
                Debug.LogError($"{nameof(Edge)}必须继承{nameof(IEditorEdgeView)}");
                return;
            }

            _edges.Remove(editorEdge);
            OnDisconnected?.Invoke(this, editorEdge);
        }

        public void CollectElements(HashSet<GraphElement> collectedElementSet, Func<GraphElement, bool> conditionFunc)
        {
            collectedElementSet.Add(this);
        }

        public bool Validate() => true;

        public bool IsSelected() => isSelected;

        public void Select()
        {
            isSelected = true;
        }

        public void Unselect()
        {
            isSelected = false;
        }

        public IEnumerable<Object> GetSelectedObjects()
        {
            yield return null;
        }

        public virtual ICopyPastePack GetPack()
        {
            List<IEditorEdgeView> edgeViews = master.graphView.graphElementCache.GetEdgeView(this);

            List<ICopyPastePack> packs = new List<ICopyPastePack>(edgeViews.Count);
            for (int i = 0; i < edgeViews.Count; i++)
            {
                IEditorEdgeView edgeView = edgeViews[i];
                ICopyPastePack edgePack = edgeView.GetPack();
                packs.Add(edgePack);
            }

            PortCopyPastePack pack = new PortCopyPastePack(info.id, packs);
            return pack;
        }

        public virtual void Dispose()
        {
            _edges.Clear();

            onConnected = null;
            OnDisconnected = null;
        }
    }
}