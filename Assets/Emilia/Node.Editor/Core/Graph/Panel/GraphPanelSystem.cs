using System;
using System.Collections.Generic;
using System.Linq;
using Emilia.Kit.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    public class GraphPanelSystem
    {
        private const string SplitViewPlaceholderName = "splitView-placeholder";
        private const float DockOffsetSize = 5f;

        private EditorGraphView graphView;
        private IGraphPanelHandle handle;

        private List<IGraphPanel> openPanels = new List<IGraphPanel>();
        private Dictionary<Type, IGraphPanel> openPanelMap = new Dictionary<Type, IGraphPanel>();

        private GraphPanelContainer floatRootContainer;
        private Dictionary<IGraphPanel, GraphPanelContainer> floatPanelMap = new Dictionary<IGraphPanel, GraphPanelContainer>();

        private GraphPanelContainer _dockRootContainer;
        private List<IGraphPanel> dockPanels = new List<IGraphPanel>();

        private VisualElement dockLeisureArea;
        private Rect dockAreaOffset;

        public GraphPanelContainer dockRootContainer => this._dockRootContainer;

        public Rect graphRect { get; set; }
        public Rect graphLayoutRect { get; set; }

        public void Reset(EditorGraphView graphView)
        {
            CloseAllPanel();

            this.graphView = graphView;
            if (handle != null) EditorHandleUtility.ReleaseHandle(handle);
            handle = EditorHandleUtility.BuildHandle<IGraphPanelHandle>(graphView.graphAsset.GetType(), graphView);

            CreateContainer();

            graphRect = this.dockLeisureArea.worldBound;
            graphLayoutRect = dockLeisureArea.layout;
            dockAreaOffset = Rect.zero;

            graphView.RegisterCallback<GeometryChangedEvent>((_) => UpdateGraphRect());

            handle?.LoadPanel(this);
        }

        private void CreateContainer()
        {
            if (this._dockRootContainer != null) this._dockRootContainer.RemoveFromHierarchy();
            this._dockRootContainer = new GraphPanelContainer() {name = "dockPanel-root"};
            this.dockLeisureArea = this._dockRootContainer;
            graphView.Add(this._dockRootContainer);

            if (this.floatRootContainer != null) this.floatRootContainer.RemoveFromHierarchy();
            this.floatRootContainer = new GraphPanelContainer() {name = "floatPanel-root"};
            graphView.Add(this.floatRootContainer);

        }

        public T OpenFloatPanel<T>() where T : IGraphPanel
        {
            T panel = ReflectUtility.CreateInstance<T>();
            GraphPanelContainer container = AddFloatPanel(panel);
            openPanels.Add(panel);

            panel.rootView.RegisterCallback<GeometryChangedEvent>((_) => UpdateGraphRect());
            panel.Initialize(graphView);
            openPanelMap[typeof(T)] = panel;
            this.floatPanelMap[panel] = container;
            return panel;
        }

        public T OpenDockPanel<T>(float size, GraphDockPosition position) where T : IGraphPanel
        {
            T panel = ReflectUtility.CreateInstance<T>();
            AddDockPanel(panel, size, position);

            panel.rootView.RegisterCallback<GeometryChangedEvent>((_) => UpdateGraphRect());
            panel.Initialize(graphView);

            this.openPanelMap[typeof(T)] = panel;
            openPanels.Add(panel);
            dockPanels.Add(panel);
            return panel;
        }

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

                VisualElement original = default;
                if (area.contentContainer.childCount > 0) original = area.contentContainer.Children().FirstOrDefault();

                GraphTwoPaneSplitView addSplitView = AddDockPanel(panel, size, position, area);
                if (original == null) return panel;
                VisualElement placeholder = addSplitView.Q(SplitViewPlaceholderName);
                original.RemoveFromHierarchy();
                placeholder.Add(original);
            }

            return panel;
        }

        public void SetActive<T>(bool isActive) where T : IGraphPanel
        {
            IGraphPanel panel = this.openPanelMap.GetValueOrDefault(typeof(T));
            if (panel == null) return;
            panel.rootView.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void ClosePanel<T>() where T : IGraphPanel
        {
            IGraphPanel panel = this.openPanelMap.GetValueOrDefault(typeof(T));
            if (panel == null) return;
            panel.Dispose();

            panel.rootView.RemoveFromHierarchy();

            GraphPanelContainer container = floatPanelMap.GetValueOrDefault(panel);
            if (container != null) container.RemoveFromHierarchy();

            this.openPanels.Remove(panel);
            this.openPanelMap.Remove(typeof(T));
        }

        public T GetPanel<T>() where T : IGraphPanel
        {
            return (T) this.openPanelMap.GetValueOrDefault(typeof(T));
        }

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

        private GraphTwoPaneSplitView AddDockPanel(IGraphPanel panel, float size, GraphDockPosition position, VisualElement dockArea = default)
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

            GraphTwoPaneSplitView splitView = new GraphTwoPaneSplitView(0, size, orientation);
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

        public void CloseAllPanel()
        {
            foreach (IGraphPanel panel in this.openPanels)
            {
                panel.Dispose();
                panel.rootView.RemoveFromHierarchy();
            }

            openPanels.Clear();
            this.openPanelMap.Clear();
            this.floatPanelMap.Clear();
            this.dockPanels.Clear();
        }

        public void Dispose()
        {
            CloseAllPanel();

            if (this.handle != null)
            {
                EditorHandleUtility.ReleaseHandle(handle);
                this.handle = null;
            }

            this.dockLeisureArea = null;
            this.dockAreaOffset = default;
        }
    }
}