using System;
using System.Collections.Generic;
using System.Linq;
using Emilia.Kit;
using Emilia.Kit.Editor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using NodeView = UnityEditor.Experimental.GraphView.Node;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor
{
    public abstract class EditorNodeView : NodeView, IEditorNodeView
    {
        protected List<IEditorPortView> _portViews = new List<IEditorPortView>();
        protected Dictionary<string, IEditorPortView> _portViewMap = new Dictionary<string, IEditorPortView>();

        protected Dictionary<string, EditorNodeInputPortEditInfo> inputEditInfos = new Dictionary<string, EditorNodeInputPortEditInfo>();
        protected Dictionary<string, VisualElement> inputEditElements = new Dictionary<string, VisualElement>();
        protected Dictionary<string, InspectorPropertyField> inputFields = new Dictionary<string, InspectorPropertyField>();

        public EditorNodeAsset asset { get; private set; }

        public EditorGraphView graphView { get; private set; }

        public VisualElement bottomLayerContainer { get; protected set; }
        public VisualElement topLayerContainer { get; protected set; }

        public VisualElement nodeTopContainer { get; protected set; }
        public VisualElement nodeBottomContainer { get; protected set; }

        public VisualElement portBottomContainer { get; protected set; }
        public VisualElement portTopContainer { get; protected set; }
        public VisualElement portNodeBottomContainer { get; protected set; }
        public VisualElement portNodeTopContainer { get; protected set; }

        public VisualElement inputEditContainer { get; protected set; }

        public Label titleLabel { get; protected set; }

        public virtual Color topicColor { get; private set; } = Color.black;

        public virtual GraphElement element => this;
        public IReadOnlyList<IEditorPortView> portViews => this._portViews;

        public bool isSelected { get; protected set; }
        protected virtual bool editInNode => false;

        protected virtual bool canDelete => true;
        protected virtual string styleFilePath { get; } = string.Empty;

        public virtual void Initialize(EditorGraphView graphView, EditorNodeAsset asset)
        {
            this.graphView = graphView;
            this.asset = asset;

            InitializeNodeView();
            RebuildPortView();
            InitializeExpandPort();
        }

        protected virtual void InitializeNodeView()
        {
            VisualElement contents = this.Q("contents");
            contents.RemoveFromHierarchy();

            bottomLayerContainer = new VisualElement();
            bottomLayerContainer.name = "bottom-layer-container";
            bottomLayerContainer.pickingMode = PickingMode.Ignore;

            topLayerContainer = new VisualElement();
            topLayerContainer.name = "top-layer-container";
            topLayerContainer.pickingMode = PickingMode.Ignore;

            nodeTopContainer = new VisualElement();
            nodeTopContainer.name = "node-top-container";
            nodeTopContainer.pickingMode = PickingMode.Ignore;

            nodeBottomContainer = new VisualElement();
            nodeBottomContainer.name = "node-bottom-container";
            nodeBottomContainer.pickingMode = PickingMode.Ignore;

            portBottomContainer = new VisualElement();
            portBottomContainer.name = "port-bottom-container";
            portBottomContainer.pickingMode = PickingMode.Ignore;

            portTopContainer = new VisualElement();
            portTopContainer.name = "port-top-container";
            portTopContainer.pickingMode = PickingMode.Ignore;

            portNodeBottomContainer = new VisualElement();
            portNodeBottomContainer.name = "port-node-bottom-container";
            portNodeBottomContainer.pickingMode = PickingMode.Ignore;

            portNodeTopContainer = new VisualElement();
            portNodeTopContainer.name = "port-node-top-container";
            portNodeTopContainer.pickingMode = PickingMode.Ignore;

            inputEditContainer = new VisualElement();
            inputEditContainer.name = "input-edit-container";
            inputEditContainer.pickingMode = PickingMode.Ignore;

            VisualElement layerCenter = mainContainer;

            int index = layerCenter.parent.IndexOf(layerCenter);
            layerCenter.parent.Insert(index, bottomLayerContainer);

            index = layerCenter.parent.IndexOf(layerCenter);
            layerCenter.parent.Insert(index + 1, topLayerContainer);

            VisualElement nodeCenter = titleContainer;

            index = nodeCenter.parent.IndexOf(nodeCenter);
            nodeCenter.parent.Insert(index, nodeTopContainer);

            index = nodeCenter.parent.IndexOf(nodeCenter);
            nodeCenter.parent.Insert(index + 1, nodeBottomContainer);

            bottomLayerContainer.Add(portBottomContainer);
            topLayerContainer.Add(portTopContainer);

            nodeTopContainer.Add(portNodeTopContainer);
            nodeBottomContainer.Add(portNodeBottomContainer);

            bottomLayerContainer.Insert(0, inputEditContainer);

            titleLabel = this.Q<Label>("title-label");

            if (editInNode)
            {
                IMGUIContainer assetContainer = new IMGUIContainer(() => asset.propertyTree?.Draw());
                topLayerContainer.Add(assetContainer);
            }

            if (string.IsNullOrEmpty(styleFilePath) == false)
            {
                StyleSheet styleSheet = ResourceUtility.LoadResource<StyleSheet>(styleFilePath);
                styleSheets.Add(styleSheet);
            }

            if (canDelete == false) capabilities &= ~Capabilities.Deletable;

            SetPositionNoUndo(asset.position);
            SetColor(topicColor);
        }

        protected virtual void RebuildPortView()
        {
            List<EditorPortInfo> portInfos = CollectStaticPortAssets();
            portInfos.Sort((a, b) => a.order.CompareTo(b.order));

            var categorizedPorts = new Dictionary<(EditorPortDirection, EditorOrientation), List<EditorPortInfo>> {
                {(EditorPortDirection.Input, EditorOrientation.Horizontal), new List<EditorPortInfo>()},
                {(EditorPortDirection.Input, EditorOrientation.Vertical), new List<EditorPortInfo>()},
                {(EditorPortDirection.Output, EditorOrientation.Horizontal), new List<EditorPortInfo>()},
                {(EditorPortDirection.Output, EditorOrientation.Vertical), new List<EditorPortInfo>()},
                {(EditorPortDirection.Any, EditorOrientation.Horizontal), new List<EditorPortInfo>()},
                {(EditorPortDirection.Any, EditorOrientation.Vertical), new List<EditorPortInfo>()}
            };

            for (var i = 0; i < portInfos.Count; i++)
            {
                EditorPortInfo info = portInfos[i];
                var key = (info.direction, info.orientation);
                if (categorizedPorts.ContainsKey(key)) categorizedPorts[key].Add(info);
            }

            foreach (var category in categorizedPorts) AddPortViews(category.Value);

            RemoveUnusedPortViews(portInfos);
        }

        private void AddPortViews(List<EditorPortInfo> portInfos)
        {
            for (int i = 0; i < portInfos.Count; i++)
            {
                EditorPortInfo info = portInfos[i];
                if (_portViewMap.ContainsKey(info.id) == false) AddPortView(i, info);
            }
        }

        private void RemoveUnusedPortViews(List<EditorPortInfo> portInfos)
        {
            for (int i = _portViews.Count - 1; i >= 0; i--)
            {
                IEditorPortView portView = _portViews[i];
                if (portInfos.All(p => p.id != portView.info.id)) RemovePortView(portView);
            }
        }

        /// <summary>
        /// 收集静态Port信息
        /// </summary>
        /// <returns></returns>
        public abstract List<EditorPortInfo> CollectStaticPortAssets();

        /// <summary>
        /// 根据Id获取PortView
        /// </summary>
        public virtual IEditorPortView GetPortView(string portId) => this._portViewMap.GetValueOrDefault(portId);

        /// <summary>
        /// 添加PortView
        /// </summary>
        public virtual IEditorPortView AddPortView(int index, EditorPortInfo info)
        {
            IEditorPortView portView = ReflectUtility.CreateInstance(info.nodePortViewType) as IEditorPortView;
            portView.Initialize(this, info);
            portView.onConnected += OnPortConnected;
            portView.OnDisconnected += OnPortDisconnected;

            this._portViews.Add(portView);
            this._portViewMap[info.id] = portView;

            return portView;
        }

        /// <summary>
        /// 移除PortView
        /// </summary>
        public virtual void RemovePortView(IEditorPortView portView)
        {
            if (portView == null) return;

            portView.onConnected -= OnPortConnected;
            portView.OnDisconnected -= OnPortDisconnected;

            this._portViews.Remove(portView);
            this._portViewMap.Remove(portView.info.id);

            portView.RemoveView();
        }

        protected virtual void OnPortConnected(IEditorPortView editorPortView, IEditorEdgeView editorEdgeView)
        {
            if (editorPortView.portDirection != EditorPortDirection.Input) return;
            if (editorPortView.edges.Count == 0) return;

            if (inputEditElements.TryGetValue(editorPortView.info.id, out VisualElement editElement)) editElement.AddToClassList("empty");
            if (inputFields.TryGetValue(editorPortView.info.id, out InspectorPropertyField field)) field.style.display = DisplayStyle.None;
        }

        protected virtual void OnPortDisconnected(IEditorPortView editorPortView, IEditorEdgeView editorEdgeView)
        {
            if (editorPortView.portDirection != EditorPortDirection.Input) return;
            if (editorPortView.edges.Count > 0) return;

            if (inputEditElements.TryGetValue(editorPortView.info.id, out VisualElement editElement)) editElement.RemoveFromClassList("empty");
            if (inputFields.TryGetValue(editorPortView.info.id, out InspectorPropertyField field)) field.style.display = DisplayStyle.Flex;
        }

        protected void InitializeExpandPort()
        {
            int amount = portViews.Count;
            for (int i = 0; i < amount; i++)
            {
                IEditorPortView portView = portViews[i];
                if (portView.portDirection != EditorPortDirection.Input) continue;

                if (inputEditInfos.TryGetValue(portView.info.id, out EditorNodeInputPortEditInfo info))
                {
                    AddInputEditContainer(info.portName, info.fieldPath, info.forceImGUIDraw);
                    continue;
                }

                AddEmptyInputEditContainer(portView.info.id);
            }

            if (inputEditElements.Count > 0)
            {
                EditorKit.UnityInvoke(SyncTop);

                void SyncTop()
                {
                    var pair = inputEditElements.FirstOrDefault();
                    IEditorPortView portView = portViews.FirstOrDefault(p => p.info.id == pair.Key);
                    if (portView == null) return;
                    float top = GetPortTop(portView);
                    top -= inputEditContainer.parent.layout.y;
                    inputEditContainer.style.top = top;
                }
            }
        }

        private float GetPortTop(IEditorPortView portView)
        {
            float top = 0;

            VisualElement visualElement = portView.portElement;

            while (visualElement != this)
            {
                top += visualElement.layout.y;
                visualElement = visualElement.parent;
            }

            return top;
        }

        protected void AddInputEditContainer(string portName, string fieldPath, bool forceImGUIDraw = false)
        {
            VisualElement editContainer = new VisualElement();
            editContainer.name = "edit-container";

            InspectorProperty inspectorProperty = asset.propertyTree.GetPropertyAtPath(fieldPath);
            InspectorPropertyField inspectorPropertyField = new InspectorPropertyField(inspectorProperty, forceImGUIDraw, false);
            inspectorPropertyField.AddToClassList("port-input-element");
            editContainer.Add(inspectorPropertyField);

            editContainer.RegisterCallback<GeometryChangedEvent>(_ => {
                IEditorPortView portView = this._portViewMap.GetValueOrDefault(portName);
                if (portView == null) return;
                float editHeight = editContainer.resolvedStyle.height + editContainer.resolvedStyle.marginTop + editContainer.resolvedStyle.marginBottom;
                float portHeight = portView.portElement.layout.height;

                float portMargin = Math.Max(editHeight - portHeight, 0);
                portView.portElement.style.marginBottom = portMargin;
            });

            inputEditContainer.Add(editContainer);

            inputEditElements[portName] = editContainer;
            inputFields[portName] = inspectorPropertyField;
        }

        protected void AddEmptyInputEditContainer(string portName)
        {
            VisualElement editContainer = new VisualElement();
            editContainer.name = "edit-container";
            editContainer.AddToClassList("empty");

            inputEditContainer.Add(editContainer);
            inputEditElements[portName] = editContainer;
        }

        public virtual void OnValueChanged(bool isSilent = false)
        {
            SetPositionNoUndo(asset.position);
            foreach (InspectorPropertyField value in inputFields.Values) value.Update();

            if (isSilent == false) graphView.graphSave.SetDirty();
        }

        public void RegisterCompleteObjectUndo(string name)
        {
            Undo.RegisterCompleteObjectUndo(asset, name);
            graphView.graphSave.SetDirty();
        }

        public override void CollectElements(HashSet<GraphElement> collectedElementSet, Func<GraphElement, bool> conditionFunc)
        {
            int amount = _portViews.Count;
            for (int i = 0; i < amount; i++)
            {
                IEditorPortView portView = this._portViews[i];
                List<IEditorEdgeView> edges = portView.GetEdges();
                int edgeAmount = edges.Count;
                for (int j = 0; j < edgeAmount; j++)
                {
                    IEditorEdgeView edge = edges[j];
                    collectedElementSet.Add(edge.edgeElement);
                }
            }
        }

        public override void SetPosition(Rect newPos)
        {
            if (capabilities.HasFlag(Capabilities.Movable) == false) return;
            RegisterCompleteObjectUndo("Graph MoveNode");
            base.SetPosition(newPos);
            asset.position = newPos;
        }

        public void SetPositionNoUndo(Rect newPos)
        {
            if (capabilities.HasFlag(Capabilities.Movable) == false) return;
            base.SetPosition(newPos);
            asset.position = newPos;
        }

        /// <summary>
        /// 设置节点主题颜色
        /// </summary>
        public void SetColor(Color color)
        {
            topicColor = color;

            Color titleContainerColor = topicColor;
            titleContainerColor.a = 0.25f;
            titleContainer.style.backgroundColor = titleContainerColor;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) { }

        public virtual void Delete()
        {
            graphView.nodeSystem.DeleteNode(this);
        }

        public virtual void RemoveView()
        {
            graphView.RemoveNodeView(this);
        }

        public virtual ICopyPastePack GetPack() => new NodeCopyPastePack(asset);

        public virtual bool Validate() => true;

        public virtual bool IsSelected() => isSelected;

        public virtual void Select()
        {
            isSelected = true;
        }

        public virtual void Unselect()
        {
            isSelected = false;
        }

        public virtual IEnumerable<Object> GetSelectedObjects()
        {
            if (editInNode) yield break;
            if (asset != null) yield return asset;
        }

        public override string ToString() => title;

        public virtual void Dispose()
        {
            int amount = this._portViews.Count;
            for (int i = 0; i < amount; i++)
            {
                IEditorPortView portView = this._portViews[i];
                portView.Dispose();
            }

            RemoveFromHierarchy();
        }
    }
}