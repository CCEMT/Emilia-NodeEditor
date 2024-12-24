using System;
using System.Collections;
using System.Collections.Generic;
using Emilia.Kit.Editor;
using Emilia.Node.Attributes;
using Emilia.Reflection.Editor;
using Sirenix.Utilities;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor
{
    public class EditorGraphView : GraphView_Hook
    {
        public static EditorGraphView focusedGraphView { get; private set; }

        private IGraphHandle graphHandle;

        private List<IEditorNodeView> _nodeViews = new List<IEditorNodeView>();
        private List<IEditorEdgeView> _edgeViews = new List<IEditorEdgeView>();
        private List<IEditorItemView> _itemViews = new List<IEditorItemView>();

        private GraphContentZoomer graphZoomer;
        private EditorCoroutine loadElementCoroutine;

        public Vector3 logicPosition { get; set; }
        public Vector3 logicScale { get; set; }

        public IReadOnlyList<IEditorNodeView> nodeViews => this._nodeViews;
        public IReadOnlyList<IEditorEdgeView> edgeViews => this._edgeViews;
        public IReadOnlyList<IEditorItemView> itemViews => this._itemViews;

        public EditorGraphAsset graphAsset { get; private set; }
        public GraphSettingAttribute graphSetting { get; private set; }
        public GraphLocalSettingSystem graphLocalSettingSystem { get; private set; }
        public GraphElementCache graphElementCache { get; private set; }
        public GraphOperate graphOperate { get; private set; }
        public GraphCopyPaste graphCopyPaste { get; private set; }
        public GraphUndo graphUndo { get; private set; }
        public GraphSelected graphSelected { get; private set; }
        public GraphPanelSystem graphPanelSystem { get; private set; }
        public GraphHotKeys hotKeys { get; private set; }
        public NodeSystem nodeSystem { get; private set; }
        public ConnectSystem connectSystem { get; private set; }
        public ItemSystem itemSystem { get; private set; }
        public OperateMenu operateMenu { get; private set; }
        public CreateNodeMenu createNodeMenu { get; private set; }
        public CreateItemMenu createItemMenu { get; private set; }
        public GraphDragAndDrop dragAndDrop { get; private set; }

        public float maxLoadTimeMs { get; private set; } = 0.0416f;
        public double lastUpdateTime { get; private set; }

        public float loadProgress { get; private set; }

        public EditorWindow window { get; set; }

        public event Action<Vector3, Vector3> onLogicTransformChange;

        public void Initialize()
        {
            graphLocalSettingSystem = new GraphLocalSettingSystem();
            graphElementCache = new GraphElementCache();
            graphOperate = new GraphOperate();
            graphCopyPaste = new GraphCopyPaste();
            graphUndo = new GraphUndo();
            graphSelected = new GraphSelected();
            graphPanelSystem = new GraphPanelSystem();
            hotKeys = new GraphHotKeys();
            nodeSystem = new NodeSystem();
            connectSystem = new ConnectSystem();
            itemSystem = new ItemSystem();
            operateMenu = new OperateMenu();
            createNodeMenu = new CreateNodeMenu();
            createItemMenu = new CreateItemMenu();
            dragAndDrop = new GraphDragAndDrop();

            serializeGraphElements = OnSerializeGraphElements;
            canPasteSerializedData = OnCanPasteSerializedData;
            unserializeAndPaste = OnUnserializeAndPaste;
            viewTransformChanged = OnViewTransformChanged;
            graphViewChanged = OnGraphViewChanged;
            elementResized = OnElementResized;

            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseUpEvent>(OnMouseUp);

            Undo.undoRedoPerformed += OnUndoRedoPerformed;

            SetupZoom(0.15f, 3f);
            SetViewTransform(Vector3.zero, Vector3.one, 0);
        }

        public void OnFocus()
        {
            if (focusedGraphView != this) UpdateSelectedInspector();
            focusedGraphView = this;
            this.graphHandle?.OnFocus();
        }

        public void OnUpdate()
        {
            lastUpdateTime = EditorApplication.timeSinceStartup;
            this.graphHandle?.OnUpdate();
        }

        public void Reload(EditorGraphAsset asset)
        {
            if (asset == null) return;

            schedule.Execute(OnReload).ExecuteLater(1);

            void OnReload()
            {
                if (loadElementCoroutine != null) EditorCoroutineUtility.StopCoroutine(loadElementCoroutine);

                bool allReload = graphAsset == null || graphAsset.GetType() != asset.GetType();
                graphAsset = asset;

                this.graphSetting = graphAsset.GetType().GetCustomAttribute<GraphSettingAttribute>();
                SyncSetting();

                loadProgress = 0;
                if (allReload) AllReload();
                else ElementReload();
            }
        }

        private void SyncSetting()
        {
            if (graphSetting == null) return;
            maxLoadTimeMs = graphSetting.maxLoadTimeMs;
            SetupZoom(graphSetting.zoomSize.x, graphSetting.zoomSize.y);
        }

        public void AllReload()
        {
            this.graphHandle = EditorHandleUtility.BuildHandle<IGraphHandle>(graphAsset.GetType(), this);

            ResetSystem();
            RemoveAllElement();

            nodeCreationRequest = (c) => createNodeMenu.ShowCreateNodeMenu(c);

            loadElementCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(LoadElement());
        }

        public void ElementReload()
        {
            RemoveAllElement();
            loadElementCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(LoadElement());
        }

        private void ResetSystem()
        {
            graphLocalSettingSystem.Reset(this);
            graphOperate.Reset(this);
            graphCopyPaste.Reset(this);
            graphUndo.Reset(this);
            graphSelected.Reset(this);
            graphPanelSystem.Reset(this);
            hotKeys.Reset(this);
            nodeSystem.Reset(this);
            connectSystem.Reset(this);
            itemSystem.Reset(this);
            operateMenu.Reset(this);
            createNodeMenu.Reset(this);
            createItemMenu.Reset(this);
            dragAndDrop.Reset(this);
        }

        private IEnumerator LoadElement()
        {
            graphElementCache.BuildCache(this);

            this.graphHandle?.OnLoadBefore();

            yield return LoadNodeView();
            yield return LoadEdge();
            yield return LoadItem();

            LoadSuccess();
        }

        private IEnumerator LoadNodeView()
        {
            int amount = graphAsset.nodes.Count;
            for (int i = 0; i < amount; i++)
            {
                EditorNodeAsset node = graphAsset.nodes[i];
                AddNodeView(node);

                loadProgress = (i + 1) / (float) (graphAsset.nodes.Count + graphAsset.edges.Count + graphAsset.items.Count);
                if (EditorApplication.timeSinceStartup - lastUpdateTime > maxLoadTimeMs) yield return 0;
            }
        }

        private IEnumerator LoadEdge()
        {
            int amount = graphAsset.edges.Count;
            for (int i = 0; i < amount; i++)
            {
                EditorEdgeAsset edge = graphAsset.edges[i];

                IEditorNodeView inputNode = graphElementCache.GetEditorNodeView(edge.inputNodeId);
                IEditorNodeView outputNode = graphElementCache.GetEditorNodeView(edge.outputNodeId);

                if (inputNode == null || outputNode == null) continue;

                IEditorPortView inputPort = inputNode.GetPortView(edge.inputPortId);
                IEditorPortView outputPort = outputNode.GetPortView(edge.outputPortId);

                if (inputPort == null || outputPort == null) continue;

                AddEdgeView(edge);

                loadProgress = (graphAsset.nodes.Count + i + 1) / (float) (graphAsset.nodes.Count + graphAsset.edges.Count + graphAsset.items.Count);
                if (EditorApplication.timeSinceStartup - lastUpdateTime > maxLoadTimeMs) yield return 0;
            }
        }

        private IEnumerator LoadItem()
        {
            int amount = graphAsset.items.Count;
            for (int i = 0; i < amount; i++)
            {
                EditorItemAsset itemAsset = graphAsset.items[i];
                AddItemView(itemAsset);

                loadProgress = (graphAsset.nodes.Count + graphAsset.edges.Count + i + 1) / (float) (graphAsset.nodes.Count + graphAsset.edges.Count + graphAsset.items.Count);
                if (EditorApplication.timeSinceStartup - lastUpdateTime > maxLoadTimeMs) yield return 0;
            }
        }

        private void LoadSuccess()
        {
            if (graphAsset == null) return;

            UpdateSelectedInspector();
            loadElementCoroutine = null;
            loadProgress = 1;

            this.graphHandle?.OnLoadAfter();
        }

        public IEditorNodeView AddNode(EditorNodeAsset nodeAsset)
        {
            graphAsset.AddNode(nodeAsset);
            IEditorNodeView nodeView = AddNodeView(nodeAsset);
            return nodeView;
        }

        public IEditorNodeView AddNodeView(EditorNodeAsset nodeAsset)
        {
            Type nodeViewType = GraphTypeCache.GetNodeViewType(nodeAsset.GetType());
            if (nodeViewType == null) return default;

            IEditorNodeView nodeView = ReflectUtility.CreateInstance(nodeViewType) as IEditorNodeView;
            nodeView.Initialize(this, nodeAsset);
            AddElement(nodeView.element);

            this._nodeViews.Add(nodeView);
            graphElementCache.SetNodeViewCache(nodeAsset.id, nodeView);
            return nodeView;
        }

        public void RemoveNodeView(IEditorNodeView nodeView)
        {
            if (nodeView == null) return;
            if (nodeView.asset != null) graphElementCache.RemoveNodeViewCache(nodeView.asset.id);

            nodeView.Dispose();
            this.RemoveElement(nodeView.element);
            this._nodeViews.Remove(nodeView);
        }

        public IEditorEdgeView AddEdge(EditorEdgeAsset asset)
        {
            graphAsset.AddEdge(asset);
            IEditorEdgeView edgeView = AddEdgeView(asset);
            return edgeView;
        }

        public IEditorEdgeView AddEdgeView(EditorEdgeAsset asset)
        {
            Type edgeViewType = GraphTypeCache.GetEdgeViewType(asset.GetType());
            if (edgeViewType == null) return default;

            IEditorEdgeView edgeView = ReflectUtility.CreateInstance(edgeViewType) as IEditorEdgeView;
            edgeView.edgeElement.RegisterCallback<MouseDownEvent>((_) => UpdateSelectedInspector());
            edgeView.Initialize(this, asset);
            AddElement(edgeView.edgeElement);

            this._edgeViews.Add(edgeView);
            graphElementCache.SetEdgeViewCache(asset.id, edgeView);
            return edgeView;
        }

        public void RemoveEdgeView(IEditorEdgeView edge)
        {
            if (edge == null) return;
            if (edge.asset != null) graphElementCache.RemoveEdgeViewCache(edge.asset.id);

            edge.Dispose();
            this.RemoveElement(edge.edgeElement);

            edge.inputPortView?.portElement.Disconnect(edge.edgeElement);
            edge.outputPortView?.portElement.Disconnect(edge.edgeElement);
            this._edgeViews.Remove(edge);
        }

        public IEditorItemView AddItem(EditorItemAsset asset)
        {
            graphAsset.AddItem(asset);
            IEditorItemView itemView = AddItemView(asset);
            return itemView;
        }

        public IEditorItemView AddItemView(EditorItemAsset asset)
        {
            Type itemViewType = GraphTypeCache.GetItemViewType(asset.GetType());
            if (itemViewType == null) return default;
            IEditorItemView itemView = ReflectUtility.CreateInstance(itemViewType) as IEditorItemView;
            itemView.element.RegisterCallback<MouseDownEvent>((_) => UpdateSelectedInspector());
            itemView.Initialize(this, asset);
            AddElement(itemView.element);

            this._itemViews.Add(itemView);
            graphElementCache.SetItemViewCache(asset.id, itemView);
            return itemView;
        }

        public void RemoveItemView(IEditorItemView item)
        {
            if (item == null) return;
            if (item.asset != null) graphElementCache.RemoveItemViewCache(item.asset.id);

            item.Dispose();
            this.RemoveElement(item.element);
            this._itemViews.Remove(item);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            OperateMenuContext menuContext = new OperateMenuContext();
            menuContext.evt = evt;
            menuContext.graphView = this;

            operateMenu.BuildMenu(menuContext);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            IEditorPortView startPortView = startPort as IEditorPortView;
            if (startPortView == null) return compatiblePorts;

            foreach (Port port in this.ports)
            {
                IEditorPortView portView = port as IEditorPortView;
                if (portView == null) continue;
                if (startPortView.master == portView.master) continue;

                bool canConnect = connectSystem.CanConnect(startPortView, portView);
                if (canConnect) compatiblePorts.Add(port);
            }

            return compatiblePorts;
        }

        private string OnSerializeGraphElements(IEnumerable<GraphElement> elements)
        {
            return graphCopyPaste.SerializeGraphElementsCallback(elements);
        }

        private bool OnCanPasteSerializedData(string data)
        {
            return graphCopyPaste.CanPasteSerializedDataCallback(data);
        }

        private void OnUnserializeAndPaste(string operationName, string data)
        {
            graphCopyPaste.UnserializeAndPasteCallback(operationName, data);
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove == null || graphViewChange.elementsToRemove.Count <= 0) return graphViewChange;

            Undo.IncrementCurrentGroup();

            Delete(graphViewChange.elementsToRemove);
            graphViewChange.elementsToRemove.Clear();
            UpdateSelectedInspector();

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            Undo.IncrementCurrentGroup();

            return graphViewChange;
        }

        private void Delete(List<GraphElement> elementsToRemove)
        {
            int amount = elementsToRemove.Count;
            for (int i = 0; i < amount; i++)
            {
                GraphElement element = elementsToRemove[i];
                IDeleteGraphElement iDeleteGraphElement = element as IDeleteGraphElement;
                if (iDeleteGraphElement != null) iDeleteGraphElement.Delete();
            }
        }

        public void UpdateLogicTransform(Vector3 position, Vector3 scale)
        {
            logicPosition = position;
            logicScale = scale;

            onLogicTransformChange?.Invoke(logicPosition, logicScale);
        }

        private void OnViewTransformChanged(GraphView graphview)
        {
            UpdateLogicTransform(graphview.viewTransform.position, graphview.viewTransform.scale);
        }

        private void OnElementResized(VisualElement element)
        {
            IResizedGraphElement graphElement = element as IResizedGraphElement;
            if (graphElement != null) graphElement.OnElementResized();
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            UpdateSelectedInspector();
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            schedule.Execute(UpdateSelectedInspector).ExecuteLater(1);
        }

        private void OnUndoRedoPerformed()
        {
            if (graphSetting != null && graphSetting.fastUndo == false) Reload(graphAsset);
            else graphUndo.OnUndoRedoPerformed();
        }

        public void UpdateSelectedInspector()
        {
            graphSelected.UpdateSelected(selection);
        }

        public void RegisterCompleteObjectUndo(string name)
        {
            List<Object> objects = new List<Object>();
            graphAsset.CollectAsset(objects);

            Undo.RegisterCompleteObjectUndo(objects.ToArray(), name);
        }

        private void RemoveAllElement()
        {
            foreach (GraphElement graphElement in graphElements)
            {
                IRemoveViewElement removeViewElement = graphElement as IRemoveViewElement;
                removeViewElement?.RemoveView();
            }
        }

        protected override bool OnUpdateContentZoomer()
        {
            if (minScale != maxScale)
            {
                if (graphZoomer == null)
                {
                    graphZoomer = new GraphContentZoomer();
                    this.AddManipulator(graphZoomer);
                }

                graphZoomer.minScale = minScale;
                graphZoomer.maxScale = maxScale;
                graphZoomer.scaleStep = scaleStep;
                graphZoomer.referenceScale = referenceScale;
            }
            else
            {
                if (graphZoomer != null) this.RemoveManipulator(graphZoomer);
            }

            ValidateTransform();
            return true;
        }

        private EditorCoroutine updateViewTransformCoroutine;

        public void SetViewTransform(Vector3 newPosition, Vector3 newScale, float time = 0.2f)
        {
            schedule.Execute(OnSetViewTransform).ExecuteLater(1);

            void OnSetViewTransform()
            {
                float validateFloat = newPosition.x + newPosition.y + newPosition.z + newScale.x + newScale.y + newScale.z;
                if (float.IsInfinity(validateFloat) || float.IsNaN(validateFloat)) return;

                newPosition.x = GUIUtility_Internals.RoundToPixelGrid_Internals(newPosition.x);
                newPosition.y = GUIUtility_Internals.RoundToPixelGrid_Internals(newPosition.y);

                UpdateLogicTransform(newPosition, newScale);

                if (time > 0)
                {
                    if (updateViewTransformCoroutine != null) EditorCoroutineUtility.StopCoroutine(updateViewTransformCoroutine);
                    updateViewTransformCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(SetTransformAnimation());
                }
                else
                {
                    contentViewContainer.transform.position = newPosition;
                    contentViewContainer.transform.scale = newScale;

                    UpdatePersistedViewTransform_Internals();
                    if (viewTransformChanged != null) viewTransformChanged(this);
                }
            }

            IEnumerator SetTransformAnimation()
            {
                Vector2 startPosition = contentViewContainer.transform.position;
                Vector3 startScale = contentViewContainer.transform.scale;

                double startTime = EditorApplication.timeSinceStartup;

                while (EditorApplication.timeSinceStartup - startTime < time)
                {
                    float t = (float) ((EditorApplication.timeSinceStartup - startTime) / time);

                    Vector2 currentPosition = Vector2.Lerp(startPosition, newPosition, t);
                    Vector3 currentScale = Vector3.Lerp(startScale, newScale, t);

                    contentViewContainer.transform.position = currentPosition;
                    contentViewContainer.transform.scale = currentScale;

                    yield return 0;
                }

                contentViewContainer.transform.position = newPosition;
                contentViewContainer.transform.scale = newScale;

                viewTransformChanged?.Invoke(this);
                UpdatePersistedViewTransform_Internals();

                updateViewTransformCoroutine = null;
            }
        }

        public void Dispose()
        {
            if (loadElementCoroutine != null) EditorCoroutineUtility.StopCoroutine(loadElementCoroutine);
            loadElementCoroutine = null;

            RemoveAllElement();

            Undo.undoRedoPerformed -= OnUndoRedoPerformed;

            if (this.graphHandle != null)
            {
                EditorHandleUtility.ReleaseHandle(this.graphHandle);
                this.graphHandle = null;
            }

            DisposeSystem();

            if (focusedGraphView == this) focusedGraphView = null;
        }

        private void DisposeSystem()
        {
            graphLocalSettingSystem.Dispose();
            graphElementCache.Dispose();
            graphOperate.Dispose();
            graphCopyPaste.Dispose();
            graphUndo.Dispose();
            graphSelected.Dispose();
            graphPanelSystem.Dispose();
            hotKeys.Dispose();
            nodeSystem.Dispose();
            connectSystem.Dispose();
            itemSystem.Dispose();
            operateMenu.Dispose();
            createNodeMenu.Dispose();
            createItemMenu.Dispose();
            dragAndDrop.Dispose();
        }
    }
}