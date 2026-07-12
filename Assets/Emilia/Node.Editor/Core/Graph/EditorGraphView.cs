using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Emilia.Kit;
using Emilia.Kit.Editor;
using Emilia.Reflection.Editor;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor
{
    /// <summary>
    /// 编辑器GraphView
    /// </summary>
    public partial class EditorGraphView : GraphView_Hook
    {
        private static Dictionary<EditorGraphAsset, EditorGraphView> graphViews = new();

        /// <summary>
        /// 聚焦的GraphView
        /// </summary>
        public static EditorGraphView focusedGraphView { get; set; }

        private GraphHandle graphHandle;
        private List<GraphData> datas = new();

        private Dictionary<Type, BasicGraphViewModule> modules = new();
        private Dictionary<Type, CustomGraphViewModule> customModules = new();

        private List<IEditorNodeView> _nodeViews = new();
        private List<IEditorEdgeView> _edgeViews = new();
        private List<IEditorItemView> _itemViews = new();

        private GraphContentZoomer graphZoomer;
        private EditorCoroutine loadElementCoroutine;
        private EditorCoroutine updateViewTransformCoroutine;

        /// <summary>
        /// 逻辑位置
        /// </summary>
        public Vector3 logicPosition { get; set; }

        /// <summary>
        /// 逻辑缩放
        /// </summary>
        public Vector3 logicScale { get; set; }

        /// <summary>
        /// 所有IEditorNodeView
        /// </summary>
        public IReadOnlyList<IEditorNodeView> nodeViews => this._nodeViews;

        /// <summary>
        /// 所有IEditorEdgeView
        /// </summary>
        public IReadOnlyList<IEditorEdgeView> edgeViews => this._edgeViews;

        /// <summary>
        /// 所有IEditorItemView
        /// </summary>
        public IReadOnlyList<IEditorItemView> itemViews => this._itemViews;

        /// <summary>
        /// 加载的GraphAsset
        /// </summary>
        public EditorGraphAsset graphAsset { get; set; }

        /// <summary>
        /// Element缓存
        /// </summary>
        public GraphElementCache graphElementCache { get; set; }

        /// <summary>
        /// 本地设置
        /// </summary>
        public GraphLocalSettingSystem graphLocalSettingSystem { get; set; }

        /// <summary>
        /// 操作
        /// </summary>
        public GraphOperate graphOperate { get; set; }

        /// <summary>
        /// 拷贝粘贴处理
        /// </summary>
        public GraphCopyPaste graphCopyPaste { get; set; }

        /// <summary>
        /// 撤销处理
        /// </summary>
        public GraphUndo graphUndo { get; set; }

        /// <summary>
        /// 保存处理
        /// </summary>
        public GraphSave graphSave { get; set; }

        /// <summary>
        /// 选中处理
        /// </summary>
        public GraphSelected graphSelected { get; set; }

        /// <summary>
        /// 面板管理
        /// </summary>
        public GraphPanelSystem graphPanelSystem { get; set; }

        /// <summary>
        /// 快捷键管理
        /// </summary>
        public GraphHotKeys hotKeys { get; set; }

        /// <summary>
        /// 节点管理
        /// </summary>
        public GraphNodeSystem nodeSystem { get; set; }

        /// <summary>
        /// 连接管理
        /// </summary>
        public GraphConnectSystem connectSystem { get; set; }

        /// <summary>
        /// Item管理
        /// </summary>
        public GraphItemSystem itemSystem { get; set; }

        /// <summary>
        /// 操作菜单
        /// </summary>
        public GraphOperateMenu operateMenu { get; set; }

        /// <summary>
        /// 创建节点菜单
        /// </summary>
        public GraphCreateNodeMenu createNodeMenu { get; set; }

        /// <summary>
        /// 创建Item菜单
        /// </summary>
        public GraphCreateItemMenu createItemMenu { get; set; }

        /// <summary>
        /// 拖拽管理
        /// </summary>
        public GraphDragAndDrop dragAndDrop { get; set; }

        /// <summary>
        /// 每帧最大加载时间（毫秒）
        /// </summary>
        public float maxLoadTimeMs { get; set; } = 0.0416f;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public double lastUpdateTime { get; set; }

        /// <summary>
        /// 加载进度
        /// </summary>
        public float loadProgress { get; set; }

        /// <summary>
        /// 初始化完成
        /// </summary>
        public bool isInitialized { get; set; }

        /// <summary>
        /// 是否聚焦
        /// </summary>
        public bool isFocus { get; set; }

        /// <summary>
        /// 当前窗口
        /// </summary>
        public EditorWindow window { get; set; }

        /// <summary>
        /// 更新事件
        /// </summary>
        public event Action onUpdate;

        /// <summary>
        /// 逻辑Transform改变事件
        /// </summary>
        public event Action<Vector3, Vector3> onLogicTransformChange;

        /// <summary>
        /// GraphAsset改变事件
        /// </summary>
        public event Action<EditorGraphAsset> onGraphAssetChange;

        public void Initialize()
        {
            InitializeModule();

            graphLocalSettingSystem = GetModule<GraphLocalSettingSystem>();
            graphOperate = GetModule<GraphOperate>();
            graphCopyPaste = GetModule<GraphCopyPaste>();
            graphUndo = GetModule<GraphUndo>();
            graphSave = GetModule<GraphSave>();
            graphSelected = GetModule<GraphSelected>();
            graphPanelSystem = GetModule<GraphPanelSystem>();
            hotKeys = GetModule<GraphHotKeys>();
            nodeSystem = GetModule<GraphNodeSystem>();
            connectSystem = GetModule<GraphConnectSystem>();
            itemSystem = GetModule<GraphItemSystem>();
            operateMenu = GetModule<GraphOperateMenu>();
            createNodeMenu = GetModule<GraphCreateNodeMenu>();
            createItemMenu = GetModule<GraphCreateItemMenu>();
            dragAndDrop = GetModule<GraphDragAndDrop>();

            graphElementCache = new GraphElementCache();

            serializeGraphElements = OnSerializeGraphElements;
            canPasteSerializedData = OnCanPasteSerializedData;
            unserializeAndPaste = OnUnserializeAndPaste;
            viewTransformChanged = OnViewTransformChanged;
            graphViewChanged = OnGraphViewChanged;
            elementResized = OnElementResized;

            Undo.undoRedoPerformed += OnUndoRedoPerformed;

            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseUpEvent>(OnMouseUp);

            RegisterCallback<MouseEnterEvent>((_) => OnEnterFocus());
            RegisterCallback<MouseMoveEvent>((_) => OnFocus());
            RegisterCallback<MouseLeaveEvent>((_) => OnExitFocus());

            SetupZoom(0.15f, 3f);
            SetViewTransform(Vector3.zero, Vector3.one, 0);
        }

        private void InitializeModule()
        {
            modules.Clear();

            IList<Type> types = TypeCache.GetTypesDerivedFrom<BasicGraphViewModule>();
            List<BasicGraphViewModule> moduleList = new();

            foreach (Type type in types)
            {
                if (type.IsAbstract) continue;
                BasicGraphViewModule module = ReflectUtility.CreateInstance(type) as BasicGraphViewModule;
                if (module == null) continue;
                moduleList.Add(module);
            }

            moduleList.Sort((x, y) => x.order.CompareTo(y.order));

            foreach (BasicGraphViewModule module in moduleList) modules.Add(module.GetType(), module);
        }

        /// <summary>
        /// 获取模块
        /// </summary>
        public T GetModule<T>() where T : GraphViewModule
        {
            T result = this.modules.GetValueOrDefault(typeof(T)) as T;
            if (result != null) return result;

            return this.customModules.GetValueOrDefault(typeof(T)) as T;
        }

        public void OnEnterFocus()
        {
            if (isInitialized == false) return;

            if (isFocus) return;
            isFocus = true;

            this.graphHandle?.OnEnterFocus(this);
        }

        public void OnFocus()
        {
            if (isInitialized == false) return;

            if (isFocus == false) OnEnterFocus();

            if (focusedGraphView != this)
            {
                focusedGraphView = this;
                graphUndo.OnUndoRedoPerformed(true);
            }

            this.graphHandle?.OnFocus(this);
        }

        public void OnExitFocus()
        {
            if (isInitialized == false) return;

            if (isFocus == false) return;
            isFocus = false;

            this.graphHandle?.OnExitFocus(this);
        }

        public void OnUpdate()
        {
            lastUpdateTime = EditorApplication.timeSinceStartup;
            this.graphHandle?.OnUpdate(this);
            onUpdate?.Invoke();
        }

        /// <summary>
        /// 获取GraphData
        /// </summary>
        public T GetGraphData<T>()
        {
            for (var i = 0; i < this.datas.Count; i++)
            {
                GraphData data = this.datas[i];
                if (data is T result) return result;
            }

            return default;
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void Save(bool force = true)
        {
            graphSave?.Save(force);
        }

        /// <summary>
        /// 有效性
        /// </summary>
        public bool Validate() => panel != null;

        public void Dispose()
        {
            if (graphAsset != null) graphViews.Remove(graphAsset);

            Save(false);

            if (loadElementCoroutine != null) EditorCoroutineUtility.StopCoroutine(loadElementCoroutine);
            loadElementCoroutine = null;

            RemoveAllElement();

            foreach (CustomGraphViewModule customModule in this.customModules.Values) customModule.Dispose();
            this.customModules.Clear();

            foreach (BasicGraphViewModule module in this.modules.Values) module.Dispose();

            graphElementCache.Clear();

            Undo.undoRedoPerformed -= OnUndoRedoPerformed;

            if (focusedGraphView == this) focusedGraphView = null;
            if (this.graphHandle != null)
            {
                this.graphHandle.Dispose(this);
                this.graphHandle = null;
            }
        }

        /// <summary>
        /// 销毁
        /// 只进行最基本的清理
        /// </summary>
        public void Destroy()
        {
            if (loadElementCoroutine != null) EditorCoroutineUtility.StopCoroutine(loadElementCoroutine);
            loadElementCoroutine = null;

            foreach (CustomGraphViewModule customModule in this.customModules.Values) customModule.Dispose();
            this.customModules.Clear();

            foreach (BasicGraphViewModule module in this.modules.Values) module.Dispose();

            Undo.undoRedoPerformed -= OnUndoRedoPerformed;

            if (focusedGraphView == this) focusedGraphView = null;
            if (this.graphHandle != null)
            {
                this.graphHandle.Dispose(this);
                this.graphHandle = null;
            }
        }

        /// <summary>
        /// 根据Asset获取View
        /// </summary>
        public static EditorGraphView GetGraphView(EditorGraphAsset asset)
        {
            if (asset == null) return null;
            EditorGraphView graphView = graphViews.GetValueOrDefault(asset);
            if (graphView == null) return null;

            bool validate = graphView.Validate();
            if (validate) return graphView;

            graphViews.Remove(asset);
            return null;
        }
    }
}
