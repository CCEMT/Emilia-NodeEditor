using System;
using System.Collections.Generic;
using System.Linq;
using Emilia.Kit;
using Emilia.Kit.Editor;
using Emilia.Node.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Emilia.Node.Universal.Editor
{
    public interface IUniversalCreateNodeMenuInfoProvider
    {
        string GetTitle();
        void CreateNodeTree(CreateNodeContext createNodeContext, Action<CreateNodeMenuItem> groupCreate, Action<CreateNodeMenuItem> itemCreate);
        bool CreateNode(CreateNodeInfo createNodeInfo, CreateNodeContext createNodeContext);
    }

    [EditorHandle(typeof(EditorUniversalGraphAsset))]
    public class UniversalCreateNodeMenuHandle : CreateNodeMenuHandle, IUniversalCreateNodeMenuInfoProvider
    {
        private EditorGraphView editorGraphView;
        private Texture2D nullIcon;

        protected CreateNodeMenuProvider createNodeMenuProvider { get; private set; }

        public string GetTitle() => "Create Node";

        public override void Initialize(EditorGraphView graphView)
        {
            base.Initialize(graphView);
            this.editorGraphView = graphView;
            nullIcon = new Texture2D(1, 1);
            nullIcon.SetPixel(0, 0, Color.clear);
            nullIcon.Apply();

            createNodeMenuProvider = ScriptableObject.CreateInstance<CreateNodeMenuProvider>();
        }

        public override void InitializeCache(EditorGraphView graphView, List<ICreateNodeHandle> createNodeHandles)
        {
            InitializeRuntimeNodeCache(graphView, createNodeHandles);
            InitializeEditorNodeCache(graphView, createNodeHandles);
        }

        private void InitializeRuntimeNodeCache(EditorGraphView graphView, List<ICreateNodeHandle> createNodeHandles)
        {
            Type assetType = graphView.graphAsset.GetType();
            NodeToRuntimeAttribute attribute = assetType.GetCustomAttribute<NodeToRuntimeAttribute>(true);
            if (attribute == null) return;

            IList<Type> types = TypeCache.GetTypesDerivedFrom(attribute.baseRuntimeNodeType);
            int amount = types.Count;
            for (int i = 0; i < amount; i++)
            {
                Type type = types[i];
                if (type.IsAbstract || type.IsInterface || type.IsGenericType) continue;

                CreateNodeHandleContext createNodeHandleContext = new CreateNodeHandleContext();
                createNodeHandleContext.nodeType = type;
                createNodeHandleContext.defaultEditorNodeType = attribute.baseEditorNodeType;

                ICreateNodeHandle nodeHandle = EditorHandleUtility.CreateHandle<ICreateNodeHandle>(type);
                if (nodeHandle == null) continue;

                nodeHandle.Initialize(createNodeHandleContext);
                createNodeHandles.Add(nodeHandle);
            }
        }

        private void InitializeEditorNodeCache(EditorGraphView graphView, List<ICreateNodeHandle> createNodeHandles)
        {
            Type assetType = graphView.graphAsset.GetType();
            NodeToEditorAttribute attribute = assetType.GetCustomAttribute<NodeToEditorAttribute>(true);
            if (attribute == null) return;

            IList<Type> types = TypeCache.GetTypesDerivedFrom(attribute.baseEditorNodeType);
            int amount = types.Count;
            for (int i = 0; i < amount; i++)
            {
                Type type = types[i];
                if (type.IsAbstract || type.IsInterface) continue;

                NodeMenuAttribute nodeMenuAttribute = type.GetCustomAttribute<NodeMenuAttribute>();
                if (nodeMenuAttribute == null) continue;

                CreateNodeHandle createNodeHandle = new CreateNodeHandle();
                createNodeHandle.path = nodeMenuAttribute.path;
                createNodeHandle.priority = nodeMenuAttribute.priority;
                createNodeHandle.editorNodeType = type;

                createNodeHandles.Add(createNodeHandle);
            }
        }

        public override ICreateNodeCollector GetDefaultFilter(EditorGraphView graphView)
        {
            IEditorEdgeView edgeView = graphView.graphSelected.selected.OfType<IEditorEdgeView>().FirstOrDefault();
            if (edgeView != null) return new InsertNodeCollector(graphView, edgeView);

            return null;
        }

        public override void ShowCreateNodeMenu(EditorGraphView graphView, CreateNodeContext createNodeContext)
        {
            base.ShowCreateNodeMenu(graphView, createNodeContext);

            if (createNodeContext.nodeMenu == null) return;

            createNodeMenuProvider.Initialize(graphView, createNodeContext, this);
            SearchWindowContext searchWindowContext = new SearchWindowContext(createNodeContext.screenMousePosition);
            SearchWindow_Hook.Open<CreateNodeMenuProvider, SearchWindow_Hook>(searchWindowContext, createNodeMenuProvider);
        }

        /// <summary>
        /// 创建节点树
        /// </summary>
        public virtual void CreateNodeTree(CreateNodeContext createNodeContext, Action<CreateNodeMenuItem> groupCreate, Action<CreateNodeMenuItem> itemCreate)
        {
            Dictionary<string, List<CreateNodeMenuItem>> groupItemsByPath = new Dictionary<string, List<CreateNodeMenuItem>>();
            Dictionary<string, CreateNodeMenuItem> nodeItemByFullPath = new Dictionary<string, CreateNodeMenuItem>();

            List<MenuNodeInfo> allNodeInfos = new List<MenuNodeInfo>();
            CollectAllCreateNodeInfos(this.editorGraphView, allNodeInfos, createNodeContext);

            List<CreateNodeInfo> createNodeInfos = createNodeContext.nodeCollector != null
                ? createNodeContext.nodeCollector.Collect(allNodeInfos)
                : allNodeInfos.Select(info => new CreateNodeInfo(info)).ToList();

            int createCount = createNodeInfos.Count;
            for (int i = 0; i < createCount; i++)
            {
                CreateNodeInfo createNodeInfo = createNodeInfos[i];

                string fullPath = createNodeInfo.menuInfo.path;
                int nodeLevel = BuildGroupHierarchy(fullPath, createNodeInfo);

                CreateNodeMenuItem nodeMenuItem = new CreateNodeMenuItem();
                nodeMenuItem.info = createNodeInfo;
                nodeMenuItem.level = nodeLevel;

                nodeItemByFullPath[fullPath] = nodeMenuItem;
            }

            List<string> groupPaths = new List<string>();
            groupPaths.AddRange(groupItemsByPath.Keys);

            groupPaths.Sort((a, b) => {
                int aMaxPriority = GetMaxPriority(groupItemsByPath[a]);
                int bMaxPriority = GetMaxPriority(groupItemsByPath[b]);
                return aMaxPriority.CompareTo(bMaxPriority);
            });

            List<string> nodePaths = new List<string>();
            nodePaths.AddRange(nodeItemByFullPath.Keys);

            nodePaths.Sort((a, b) => {
                CreateNodeMenuItem aItem = nodeItemByFullPath[a];
                CreateNodeMenuItem bItem = nodeItemByFullPath[b];
                return aItem.info.menuInfo.priority.CompareTo(bItem.info.menuInfo.priority);
            });

            List<string> createdNodePaths = new List<string>();

            for (int i = 0; i < groupPaths.Count; i++)
            {
                string groupPath = groupPaths[i];
                CreateNodeMenuItem groupMenuItem = groupItemsByPath[groupPath].FirstOrDefault();
                groupCreate?.Invoke(groupMenuItem);

                for (int j = 0; j < nodePaths.Count; j++)
                {
                    string nodePath = nodePaths[j];
                    if (nodePath.Contains(groupPath) == false) continue;
                    AddItem(groupMenuItem, nodePath);
                }
            }

            for (int i = 0; i < nodePaths.Count; i++)
            {
                string nodePath = nodePaths[i];
                if (createdNodePaths.Contains(nodePath)) continue;
                AddItem(null, nodePath);
            }

            int BuildGroupHierarchy(string path, CreateNodeInfo info)
            {
                string[] parts = path.Split('/');
                if (parts.Length <= 1) return 0;

                string runningPath = string.Empty;
                int level = 0;

                int partAmount = parts.Length;
                for (int j = 0; j < partAmount - 1; j++)
                {
                    string title = parts[j];
                    runningPath = string.IsNullOrEmpty(runningPath) ? title : $"{runningPath}/{title}";

                    level = j + 1;

                    if (groupItemsByPath.ContainsKey(runningPath) == false) groupItemsByPath[runningPath] = new List<CreateNodeMenuItem>();

                    CreateNodeMenuItem menuItem = new CreateNodeMenuItem();
                    menuItem.info = info;
                    menuItem.level = level;
                    menuItem.title = title;

                    groupItemsByPath[runningPath].Add(menuItem);
                }

                return level;
            }

            int GetMaxPriority(List<CreateNodeMenuItem> items)
            {
                int maxPriority = int.MinValue;
                for (int i = 0; i < items.Count; i++)
                {
                    CreateNodeMenuItem item = items[i];
                    if (item.info.menuInfo.priority > maxPriority) maxPriority = item.info.menuInfo.priority;
                }
                return maxPriority;
            }

            void AddItem(CreateNodeMenuItem parent, string nodePath)
            {
                CreateNodeMenuItem menuItem = nodeItemByFullPath[nodePath];
                menuItem.parent = parent;

                Texture2D icon = nullIcon;
                if (menuItem.info.menuInfo.icon != null) icon = menuItem.info.menuInfo.icon;

                string nodeName = nodePath;
                string[] parts = nodePath.Split('/');
                if (parts.Length > 1) nodeName = parts[parts.Length - 1];

                CreateNodeMenuItem itemMenu = new CreateNodeMenuItem(menuItem.info, nodeName, menuItem.level + 1);
                itemMenu.info.menuInfo.icon = icon;

                itemCreate?.Invoke(itemMenu);

                createdNodePaths.Add(nodePath);
            }
        }

        /// <summary>
        /// 收集所有创建节点信息
        /// </summary>
        protected virtual void CollectAllCreateNodeInfos(EditorGraphView graphView, List<MenuNodeInfo> createNodeInfos, CreateNodeContext createNodeContext)
        {
            int amount = graphView.createNodeMenu.createNodeHandleCacheList.Count;
            for (int i = 0; i < amount; i++)
            {
                ICreateNodeHandle nodeHandle = graphView.createNodeMenu.createNodeHandleCacheList[i];
                if (nodeHandle.validity == false) continue;

                MenuNodeInfo menuNodeInfo = new MenuNodeInfo();
                menuNodeInfo.nodeData = nodeHandle.nodeData;
                menuNodeInfo.editorNodeAssetType = nodeHandle.editorNodeType;
                menuNodeInfo.path = nodeHandle.path;
                menuNodeInfo.priority = nodeHandle.priority;
                menuNodeInfo.icon = nodeHandle.icon;
                createNodeInfos.Add(menuNodeInfo);
            }
        }

        /// <summary>
        /// 创建节点
        /// </summary>
        public virtual bool CreateNode(CreateNodeInfo createNodeInfo, CreateNodeContext createNodeContext)
        {
            if (createNodeContext.nodeMenu == null) return false;
            EditorWindow window = this.editorGraphView.window;
            VisualElement windowRoot = window.rootVisualElement;
            Vector2 windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent, createNodeContext.screenMousePosition - window.position.position);
            Vector2 graphMousePosition = this.editorGraphView.contentViewContainer.WorldToLocal(windowMousePosition);

            Undo.IncrementCurrentGroup();

            IEditorNodeView nodeView = this.editorGraphView.nodeSystem.CreateNode(createNodeInfo.menuInfo.editorNodeAssetType, graphMousePosition, createNodeInfo.menuInfo.nodeData);
            createNodeInfo.postprocess?.Postprocess(this.editorGraphView, nodeView, createNodeContext);

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            Undo.IncrementCurrentGroup();

            return true;
        }

        public override void Dispose(EditorGraphView graphView)
        {
            base.Dispose(graphView);
            this.editorGraphView = null;

            if (this.nullIcon != null)
            {
                Object.DestroyImmediate(nullIcon);
                nullIcon = null;
            }

            if (createNodeMenuProvider != null)
            {
                Object.DestroyImmediate(createNodeMenuProvider);
                createNodeMenuProvider = null;
            }
        }
    }
}