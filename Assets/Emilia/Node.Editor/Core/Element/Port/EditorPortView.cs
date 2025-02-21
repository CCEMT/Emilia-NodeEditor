using System;
using System.Collections.Generic;
using Emilia.Kit;
using Emilia.Kit.Editor;
using Emilia.Reflection.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    public class EditorPortView : Port_Internals, IEditorPortView
    {
        private List<IEditorEdgeView> _edges = new List<IEditorEdgeView>();

        public EditorPortInfo info { get; private set; }
        public IEditorNodeView master { get; private set; }

        public virtual EditorPortDirection portDirection => info.direction;
        public virtual EditorOrientation editorOrientation => info.orientation;
        public Port portElement => this;
        public IReadOnlyList<IEditorEdgeView> edges => _edges;

        protected virtual string portStyleFilePath => "Node/Styles/UniversalEditorPortView.uss";

        public event Action<IEditorPortView, IEditorEdgeView> onConnected;
        public event Action<IEditorPortView, IEditorEdgeView> OnDisconnected;

        public EditorPortView() : base(default, default, default, null) { }

        public void Initialize(IEditorNodeView master, EditorPortInfo info)
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

        public void Dispose()
        {
            _edges.Clear();

            onConnected = null;
            OnDisconnected = null;
        }
    }
}