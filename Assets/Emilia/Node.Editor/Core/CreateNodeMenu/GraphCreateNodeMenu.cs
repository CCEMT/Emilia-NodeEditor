using System;
using System.Collections.Generic;
using System.Linq;
using Emilia.Kit;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor
{
    public class GraphCreateNodeMenu : BasicGraphViewModule
    {
        private Texture2D nullIcon;
        private CreateNodeMenuHandle handle;

        /// <summary>
        /// 缓存的创建节点Handle
        /// </summary>
        public List<ICreateNodeHandle> createNodeHandleCacheList { get; private set; } = new List<ICreateNodeHandle>();

        public override int order => 1300;

        public override void Initialize(EditorGraphView graphView)
        {
            base.Initialize(graphView);
            createNodeHandleCacheList.Clear();

            nullIcon = new Texture2D(1, 1);
            nullIcon.SetPixel(0, 0, Color.clear);
            nullIcon.Apply();

            handle = EditorHandleUtility.CreateHandle<CreateNodeMenuHandle>(graphView.graphAsset.GetType());
            handle.Initialize(graphView);
        }

        public override void AllModuleInitializeSuccess()
        {
            base.AllModuleInitializeSuccess();
            handle?.InitializeCache(this.graphView, createNodeHandleCacheList);

            this.graphView.nodeCreationRequest = OnNodeCreationRequest;
        }

        /// <summary>
        /// 获取菜单标题
        /// </summary>
        /// <returns></returns>
        public string GetTitle()
        {
            if (this.handle == null) return string.Empty;
            return this.handle.GetTitle(this.graphView);
        }

        private void OnNodeCreationRequest(NodeCreationContext nodeCreationContext)
        {
            CreateNodeContext createNodeContext = new CreateNodeContext();
            createNodeContext.screenMousePosition = nodeCreationContext.screenMousePosition;
            ShowCreateNodeMenu(createNodeContext);
        }

        /// <summary>
        /// 显示创建节点菜单
        /// </summary>
        public void ShowCreateNodeMenu(CreateNodeContext context)
        {
            if (this.handle == null) return;
            context.nodeMenu = this;
            handle.ShowCreateNodeMenu(graphView, context);
        }

        /// <summary>
        /// 创建节点树
        /// </summary>
        public void CreateNodeTree(CreateNodeContext createNodeContext, Action<CreateNodeMenuItem> groupCreate, Action<CreateNodeMenuItem> itemCreate)
        {
            Dictionary<string, List<CreateNodeMenuItem>> titleAndPriority = new Dictionary<string, List<CreateNodeMenuItem>>();
            Dictionary<string, CreateNodeMenuItem> nodeMap = new Dictionary<string, CreateNodeMenuItem>();

            List<MenuNodeInfo> allNodeInfos = new List<MenuNodeInfo>();
            handle.CollectAllCreateNodeInfos(graphView, allNodeInfos, createNodeContext);

            List<CreateNodeInfo> createNodeInfos;
            if (createNodeContext.nodeCollect != null) createNodeInfos = createNodeContext.nodeCollect.Collect(allNodeInfos);
            else
            {
                createNodeInfos = new List<CreateNodeInfo>();
                for (var i = 0; i < allNodeInfos.Count; i++)
                {
                    MenuNodeInfo menuNodeInfo = allNodeInfos[i];
                    createNodeInfos.Add(new CreateNodeInfo(menuNodeInfo));
                }
            }

            int amount = createNodeInfos.Count;
            for (int i = 0; i < amount; i++)
            {
                CreateNodeInfo menuNodeInfo = createNodeInfos[i];

                string path = menuNodeInfo.menuInfo.path;
                int level = 0;

                string[] pathParts = path.Split('/');

                string fullTitle = "";

                if (pathParts.Length > 1)
                {
                    level++;

                    int partAmount = pathParts.Length;
                    for (int j = 0; j < partAmount - 1; j++)
                    {
                        string title = pathParts[j];

                        if (string.IsNullOrEmpty(fullTitle)) fullTitle = title;
                        else fullTitle += $"/{title}";

                        level = j + 1;

                        if (titleAndPriority.ContainsKey(fullTitle) == false) titleAndPriority[fullTitle] = new List<CreateNodeMenuItem>();

                        CreateNodeMenuItem menuItem = new CreateNodeMenuItem();
                        menuItem.info = menuNodeInfo;
                        menuItem.level = level;
                        menuItem.title = title;

                        titleAndPriority[fullTitle].Add(menuItem);
                    }
                }

                CreateNodeMenuItem nodeMenuItem = new CreateNodeMenuItem();
                nodeMenuItem.info = menuNodeInfo;
                nodeMenuItem.level = level;

                nodeMap[path] = nodeMenuItem;

            }

            List<string> titlePaths = new List<string>();
            titlePaths.AddRange(titleAndPriority.Keys);

            titlePaths.Sort((a, b) => {

                List<CreateNodeMenuItem> aItems = titleAndPriority[a];
                List<CreateNodeMenuItem> bItems = titleAndPriority[b];

                int aMaxPriority = int.MinValue;
                int bMaxPriority = int.MinValue;

                for (var i = 0; i < aItems.Count; i++)
                {
                    CreateNodeMenuItem item = aItems[i];
                    if (item.info.menuInfo.priority > aMaxPriority) aMaxPriority = item.info.menuInfo.priority;
                }

                for (var i = 0; i < bItems.Count; i++)
                {
                    CreateNodeMenuItem item = bItems[i];
                    if (item.info.menuInfo.priority > bMaxPriority) bMaxPriority = item.info.menuInfo.priority;
                }

                return aMaxPriority.CompareTo(bMaxPriority);
            });

            List<string> nodePaths = new List<string>();
            nodePaths.AddRange(nodeMap.Keys);

            nodePaths.Sort((a, b) => {
                CreateNodeMenuItem aItem = nodeMap[a];
                CreateNodeMenuItem bItem = nodeMap[b];
                return aItem.info.menuInfo.priority.CompareTo(bItem.info.menuInfo.priority);
            });

            List<string> createNodePaths = new List<string>();

            for (var i = 0; i < titlePaths.Count; i++)
            {
                string titlePath = titlePaths[i];
                CreateNodeMenuItem groupMenuItem = titleAndPriority[titlePath].FirstOrDefault();
                groupCreate?.Invoke(groupMenuItem);

                for (int j = 0; j < nodePaths.Count; j++)
                {
                    string nodePath = nodePaths[j];
                    if (nodePath.Contains(titlePath) == false) continue;
                    AddMenuItem(groupMenuItem, nodePath);
                }
            }

            for (int i = 0; i < nodePaths.Count; i++)
            {
                string nodePath = nodePaths[i];
                if (createNodePaths.Contains(nodePath)) continue;
                AddMenuItem(null, nodePath);
            }

            void AddMenuItem(CreateNodeMenuItem parent, string nodePath)
            {
                CreateNodeMenuItem menuItem = nodeMap[nodePath];
                menuItem.parent = parent;

                Texture2D icon = nullIcon;
                if (menuItem.info.menuInfo.icon != null) icon = menuItem.info.menuInfo.icon;

                string nodeName = nodePath;
                string[] parts = nodePath.Split('/');
                if (parts.Length > 1) nodeName = parts[parts.Length - 1];

                CreateNodeMenuItem itemMenu = new CreateNodeMenuItem(menuItem.info, nodeName, menuItem.level + 1);
                itemMenu.info.menuInfo.icon = icon;

                itemCreate?.Invoke(itemMenu);

                createNodePaths.Add(nodePath);
            }
        }

        /// <summary>
        /// 创建节点
        /// </summary>
        public bool CreateNode(CreateNodeInfo createNodeInfo, CreateNodeContext createNodeContext)
        {
            if (createNodeContext.nodeMenu == null) return false;
            EditorWindow window = this.graphView.window;
            VisualElement windowRoot = window.rootVisualElement;
            Vector2 windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent, createNodeContext.screenMousePosition - window.position.position);
            Vector2 graphMousePosition = graphView.contentViewContainer.WorldToLocal(windowMousePosition);

            Undo.IncrementCurrentGroup();

            IEditorNodeView nodeView = this.graphView.nodeSystem.CreateNode(createNodeInfo.menuInfo.editorNodeAssetType, graphMousePosition, createNodeInfo.menuInfo.nodeData);
            if (createNodeInfo.createNodeConnector != null) CreateConnect(nodeView, createNodeInfo, createNodeContext);

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            Undo.IncrementCurrentGroup();

            return true;
        }

        private void CreateConnect(IEditorNodeView nodeView, CreateNodeInfo createNodeInfo, CreateNodeContext createNodeContext)
        {
            IEditorNodeView originalNodeView = graphView.graphElementCache.GetEditorNodeView(createNodeInfo.createNodeConnector.originalNodeId);
            IEditorPortView originalPortView = originalNodeView.GetPortView(createNodeInfo.createNodeConnector.originalPortId);
            IEditorPortView targetPortView = nodeView.GetPortView(createNodeInfo.createNodeConnector.targetPortId);

            if (string.IsNullOrEmpty(createNodeInfo.createNodeConnector.edgeId))
            {
                if (originalPortView.portDirection == EditorPortDirection.Input) this.graphView.connectSystem.Connect(originalPortView, targetPortView);
                else this.graphView.connectSystem.Connect(targetPortView, originalPortView);
            }
            else
            {
                IEditorEdgeView edgeView = graphView.graphElementCache.edgeViewById.GetValueOrDefault(createNodeInfo.createNodeConnector.edgeId);
                if (edgeView == null) return;

                this.graphView.RegisterCompleteObjectUndo("Graph RedirectionEdge");

                EditorEdgeAsset edgeAsset = edgeView.asset;
                if (edgeAsset.inputNodeId == createNodeInfo.createNodeConnector.originalNodeId)
                {
                    edgeAsset.outputNodeId = nodeView.asset.id;
                    edgeAsset.outputPortId = createNodeInfo.createNodeConnector.targetPortId;
                }
                else
                {
                    edgeAsset.inputNodeId = nodeView.asset.id;
                    edgeAsset.inputPortId = createNodeInfo.createNodeConnector.targetPortId;
                }

                this.graphView.RemoveEdgeView(edgeView);
                this.graphView.AddEdgeView(edgeAsset);
            }
        }

        public override void Dispose()
        {
            if (this.graphView == null) return;

            createNodeHandleCacheList.Clear();

            if (this.nullIcon != null)
            {
                Object.DestroyImmediate(nullIcon);
                nullIcon = null;
            }

            if (handle != null)
            {
                handle.Dispose(this.graphView);
                handle = null;
            }

            base.Dispose();
        }
    }
}