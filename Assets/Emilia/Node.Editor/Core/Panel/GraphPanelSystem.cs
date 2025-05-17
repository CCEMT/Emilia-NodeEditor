using System;
using System.Collections.Generic;
using System.Linq;
using Emilia.Kit;
using Emilia.Kit.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    public class GraphPanelSystem : BasicGraphViewModule
    {
        private const string SplitViewPlaceholderName = "splitView-placeholder";
        private const float DockOffsetSize = 5f;

        private GraphPanelHandle handle;

        private List<IGraphPanel> _openPanels = new List<IGraphPanel>();
        private Dictionary<Type, IGraphPanel> openPanelMap = new Dictionary<Type, IGraphPanel>();

        private GraphPanelContainer floatRootContainer;
        private Dictionary<IGraphPanel, GraphPanelContainer> floatPanelMap = new Dictionary<IGraphPanel, GraphPanelContainer>();

        private GraphPanelContainer _dockRootContainer;
        private List<IGraphPanel> dockPanels = new List<IGraphPanel>();

        private VisualElement dockLeisureArea;
        private Rect dockAreaOffset;

        /// <summary>
        /// 打开的面板列表
        /// </summary>
        public IReadOnlyList<IGraphPanel> openPanels => this._openPanels;

        /// <summary>
        /// 停靠主容器
        /// </summary>
        public GraphPanelContainer dockRootContainer => this._dockRootContainer;

        /// <summary>
        /// 实际GraphView的Rect
        /// </summary>
        public Rect graphRect { get; set; }

        /// <summary>
        /// 实际GraphView的LayoutRect
        /// </summary>
        public Rect graphLayoutRect { get; set; }

        public override int order => 700;

        public override void Initialize(EditorGraphView graphView)
        {
            base.Initialize(graphView);
            handle = EditorHandleUtility.CreateHandle<GraphPanelHandle>(graphView.graphAsset.GetType());

            CreateContainer();

            graphRect = this.dockLeisureArea.worldBound;
            graphLayoutRect = dockLeisureArea.layout;
            dockAreaOffset = Rect.zero;

            graphView.RegisterCallback<GeometryChangedEvent>((_) => UpdateGraphRect());
        }

        public override void AllModuleInitializeSuccess()
        {
            base.AllModuleInitializeSuccess();
            handle?.LoadPanel(this.graphView, this);
        }

        private void CreateContainer()
        {
            if (this._dockRootContainer != null) this._dockRootContainer.RemoveFromHierarchy();
            this._dockRootContainer = new GraphPanelContainer {name = "dockPanel-root"};
            this.dockLeisureArea = this._dockRootContainer;
            graphView.Add(this._dockRootContainer);

            if (this.floatRootContainer != null) this.floatRootContainer.RemoveFromHierarchy();
            this.floatRootContainer = new GraphPanelContainer {name = "floatPanel-root"};
            graphView.Add(this.floatRootContainer);
        }

        /// <summary>
        /// 打开面板以浮动形式
        /// </summary>
        public T OpenFloatPanel<T>() where T : IGraphPanel
        {
            T panel = ReflectUtility.CreateInstance<T>();
            GraphPanelContainer container = AddFloatPanel(panel);
            this._openPanels.Add(panel);

            panel.rootView.RegisterCallback<GeometryChangedEvent>((_) => UpdateGraphRect());
            panel.Initialize(graphView);

            this.openPanelMap.TryAdd(typeof(T), panel);
            this.floatPanelMap[panel] = container;

            return panel;
        }

        /// <summary>
        /// 打开面板以停靠的形式
        /// </summary>
        public T OpenDockPanel<T>(float size, GraphDockPosition position) where T : IGraphPanel
        {
            T panel = ReflectUtility.CreateInstance<T>();
            AddDockPanel(panel, size, position);

            panel.rootView.RegisterCallback<GeometryChangedEvent>((_) => UpdateGraphRect());
            panel.Initialize(graphView);

            this.openPanelMap.TryAdd(typeof(T), panel);

            this._openPanels.Add(panel);
            dockPanels.Add(panel);

            return panel;
        }

        /// <summary>
        /// 打开面板以停靠的形式
        /// </summary>
        public T OpenDockPanel<T>(VisualElement dockArea, float size, GraphDockPosition position) where T : IGraphPanel
        {
            VisualElement addDockArea = dockArea;
            GraphTwoPaneSplitView splitView = dockArea as GraphTwoPaneSplitView;

            if (splitView == null)
            {
                splitView = dockArea.parent as GraphTwoPaneSplitView;
                addDockArea = splitView;
            }

            if (splitView == null)
            {
                splitView = dockArea.Children().FirstOrDefault() as GraphTwoPaneSplitView;
                addDockArea = dockArea;
            }

            if (splitView == null) return default;

            T panel = ReflectUtility.CreateInstance<T>();

            if (splitView.contentContainer.childCount < 2)
            {
                splitView.Add(panel.rootView);
                panel.parentView = splitView;
            }
            else
            {
                VisualElement area;
                if (addDockArea is GraphTwoPaneSplitView splitViewArea) area = splitViewArea.Q(SplitViewPlaceholderName);
                else area = addDockArea;

                VisualElement original = null;
                if (area.contentContainer.childCount > 0) original = area.contentContainer.Children().FirstOrDefault();

                GraphTwoPaneSplitView addSplitView = AddDockPanel(panel, size, position, area);
                if (original == null) return panel;
                VisualElement placeholder = addSplitView.Q(SplitViewPlaceholderName);
                original.RemoveFromHierarchy();
                placeholder.Add(original);
            }

            return panel;
        }

        /// <summary>
        /// 设置面板激活状态
        /// </summary>
        public void SetActive<T>(bool isActive) where T : IGraphPanel
        {
            IGraphPanel panel = this.openPanelMap.GetValueOrDefault(typeof(T));
            if (panel == null) return;
            panel.rootView.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// 关闭面板
        /// </summary>
        public void ClosePanel<T>() where T : IGraphPanel
        {
            IGraphPanel panel = this.openPanelMap.GetValueOrDefault(typeof(T));
            if (panel == null) return;
            panel.Dispose();

            panel.rootView.RemoveFromHierarchy();

            GraphPanelContainer container = floatPanelMap.GetValueOrDefault(panel);
            if (container != null) container.RemoveFromHierarchy();

            this._openPanels.Remove(panel);
            this.openPanelMap.Remove(typeof(T));
        }

        /// <summary>
        /// 关闭面板
        /// </summary>
        public void ClosePanel(IGraphPanel panel)
        {
            if (panel == null) return;
            panel.Dispose();

            panel.rootView.RemoveFromHierarchy();

            GraphPanelContainer container = floatPanelMap.GetValueOrDefault(panel);
            if (container != null) container.RemoveFromHierarchy();

            this.openPanelMap.Remove(panel.GetType());
            this._openPanels.Remove(panel);
        }

        /// <summary>
        /// 获取面板
        /// </summary>
        public T GetPanel<T>() where T : IGraphPanel => (T) this.openPanelMap.GetValueOrDefault(typeof(T));

        /// <summary>
        /// 获取面板（通过Id）
        /// </summary>
        public T GetPanelById<T>(string id) where T : IGraphPanel
        {
            int count = this._openPanels.Count;
            for (int i = 0; i < count; i++)
            {
                IGraphPanel panel = this._openPanels[i];
                if (panel == null || panel.id != id) continue;
                return (T) panel;
            }

            return default;
        }

        /// <summary>
        /// 更新GraphView的Rect
        /// </summary>
        public void UpdateGraphRect()
        {
            Rect rect = this.dockLeisureArea.worldBound;
            rect.x += this.dockAreaOffset.x;
            rect.y += this.dockAreaOffset.y;
            rect.width += this.dockAreaOffset.width;
            rect.height += this.dockAreaOffset.height;

            graphRect = rect;
            graphLayoutRect = this.dockLeisureArea.layout;
        }

        private GraphPanelContainer AddFloatPanel(IGraphPanel panel)
        {
            GraphPanelContainer container = new GraphPanelContainer();

            container.Add(panel.rootView);
            this.floatRootContainer.Add(container);
            return container;
        }

        private GraphTwoPaneSplitView AddDockPanel(IGraphPanel panel, float size, GraphDockPosition position, VisualElement dockArea = null)
        {
            bool isInverted = false;
            TwoPaneSplitViewOrientation orientation = default;

            if (position == GraphDockPosition.Left || position == GraphDockPosition.Right)
            {
                orientation = TwoPaneSplitViewOrientation.Horizontal;
                isInverted = position == GraphDockPosition.Right;
            }
            else if (position == GraphDockPosition.Top || position == GraphDockPosition.Bottom)
            {
                orientation = TwoPaneSplitViewOrientation.Vertical;
                isInverted = position == GraphDockPosition.Bottom;
            }

            int index = isInverted ? 1 : 0;
            GraphTwoPaneSplitView splitView = new GraphTwoPaneSplitView(index, size, orientation);
            splitView.pickingMode = PickingMode.Ignore;

            VisualElement placeholder = new VisualElement();
            placeholder.name = SplitViewPlaceholderName;
            placeholder.pickingMode = PickingMode.Ignore;

            SetDockOffset(position);

            if (isInverted)
            {
                splitView.Add(placeholder);
                splitView.Add(panel.rootView);
            }
            else
            {
                splitView.Add(panel.rootView);
                splitView.Add(placeholder);
            }

            splitView.contentContainer.pickingMode = PickingMode.Ignore;

            if (dockArea != null) dockArea.Add(splitView);
            else
            {
                this.dockLeisureArea.Add(splitView);
                this.dockLeisureArea = placeholder;
            }

            panel.parentView = splitView;

            return splitView;
        }

        private void SetDockOffset(GraphDockPosition position)
        {
            switch (position)
            {
                case GraphDockPosition.Left:
                    this.dockAreaOffset.x += DockOffsetSize;
                    this.dockAreaOffset.width -= DockOffsetSize;
                    break;
                case GraphDockPosition.Right:
                    this.dockAreaOffset.x -= DockOffsetSize;
                    this.dockAreaOffset.width += DockOffsetSize;
                    break;
                case GraphDockPosition.Top:
                    this.dockAreaOffset.y += DockOffsetSize;
                    this.dockAreaOffset.height -= DockOffsetSize;
                    break;
                case GraphDockPosition.Bottom:
                    this.dockAreaOffset.y -= DockOffsetSize;
                    this.dockAreaOffset.height += DockOffsetSize;
                    break;
            }
        }

        /// <summary>
        /// 关闭所有面板
        /// </summary>
        public void CloseAllPanel()
        {
            foreach (IGraphPanel panel in this._openPanels)
            {
                panel.Dispose();
                panel.rootView.RemoveFromHierarchy();
            }

            this._openPanels.Clear();
            this.openPanelMap.Clear();
            this.floatPanelMap.Clear();
            this.dockPanels.Clear();
        }

        public override void Dispose()
        {
            CloseAllPanel();

            this.handle = null;
            this.dockLeisureArea = null;
            this.dockAreaOffset = default;

            base.Dispose();
        }
    }
}