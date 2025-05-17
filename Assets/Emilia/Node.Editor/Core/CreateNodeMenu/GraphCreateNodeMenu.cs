using System.Collections.Generic;
using System.Linq;
using Emilia.Kit;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    public class GraphCreateNodeMenu : BasicGraphViewModule
    {
        struct MenuItem
        {
            public CreateNodeInfo info;
            public string title;
            public int level;
        }

        private CreateNodeContext createNodeContext;

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

        private void OnNodeCreationRequest(NodeCreationContext nodeCreationContext)
        {
            MenuCreateInitialize(new CreateNodeContext());
            ShowCreateNodeMenu(nodeCreationContext);
        }

        /// <summary>
        /// 显示创建节点菜单
        /// </summary>
        public void ShowCreateNodeMenu(NodeCreationContext nodeCreationContext)
        {
            handle?.ShowCreateNodeMenu(graphView, nodeCreationContext);
        }

        /// <summary>
        /// 创建节点初始化
        /// </summary>
        public void MenuCreateInitialize(CreateNodeContext context)
        {
            if (this.handle == null) return;
            this.createNodeContext = context;
            this.createNodeContext.nodeMenu = this;
            handle.MenuCreateInitialize(graphView, this.createNodeContext);
        }

        /// <summary>
        /// 获取节点菜单项
        /// </summary>
        public List<SearchTreeEntry> OnCreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>();
            searchTreeEntries.Add(new SearchTreeGroupEntry(new GUIContent(handle.GetTitle(graphView))));

            Dictionary<string, List<MenuItem>> titleAndPriority = new Dictionary<string, List<MenuItem>>();
            Dictionary<string, MenuItem> nodeMap = new Dictionary<string, MenuItem>();

            List<CreateNodeInfo> allNodeInfos = new List<CreateNodeInfo>();
            handle.CollectAllCreateNodeInfos(graphView, allNodeInfos, this.createNodeContext);

            List<CreateNodeInfo> createNodeInfos;
            if (createNodeContext.nodeCollect != null) createNodeInfos = this.createNodeContext.nodeCollect.Collect(allNodeInfos);
            else createNodeInfos = allNodeInfos;

            int amount = createNodeInfos.Count;
            for (int i = 0; i < amount; i++)
            {
                CreateNodeInfo createNodeInfo = createNodeInfos[i];

                string path = createNodeInfo.path;
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

                        if (titleAndPriority.ContainsKey(fullTitle) == false) titleAndPriority[fullTitle] = new List<MenuItem>();

                        MenuItem menuItem = new MenuItem();
                        menuItem.info = createNodeInfo;
                        menuItem.level = level;
                        menuItem.title = title;

                        titleAndPriority[fullTitle].Add(menuItem);
                    }
                }

                MenuItem nodeMenuItem = new MenuItem();
                nodeMenuItem.info = createNodeInfo;
                nodeMenuItem.level = level;

                nodeMap[path] = nodeMenuItem;

            }

            List<string> titlePaths = new List<string>();
            titlePaths.AddRange(titleAndPriority.Keys);

            titlePaths.Sort((a, b) => {

                List<MenuItem> aItems = titleAndPriority[a];
                List<MenuItem> bItems = titleAndPriority[b];

                int aMaxPriority = int.MinValue;
                int bMaxPriority = int.MinValue;

                for (var i = 0; i < aItems.Count; i++)
                {
                    MenuItem item = aItems[i];
                    if (item.info.priority > aMaxPriority) aMaxPriority = item.info.priority;
                }

                for (var i = 0; i < bItems.Count; i++)
                {
                    MenuItem item = bItems[i];
                    if (item.info.priority > bMaxPriority) bMaxPriority = item.info.priority;
                }

                return aMaxPriority.CompareTo(bMaxPriority);
            });

            List<string> nodePaths = new List<string>();
            nodePaths.AddRange(nodeMap.Keys);

            nodePaths.Sort((a, b) => {
                MenuItem aItem = nodeMap[a];
                MenuItem bItem = nodeMap[b];
                return aItem.info.priority.CompareTo(bItem.info.priority);
            });

            List<string> createNodePaths = new List<string>();

            for (var i = 0; i < titlePaths.Count; i++)
            {
                string titlePath = titlePaths[i];
                MenuItem groupMenuItem = titleAndPriority[titlePath].FirstOrDefault();
                searchTreeEntries.Add(new SearchTreeGroupEntry(new GUIContent(groupMenuItem.title), groupMenuItem.level));

                for (int j = 0; j < nodePaths.Count; j++)
                {
                    string nodePath = nodePaths[j];
                    if (nodePath.Contains(titlePath) == false) continue;
                    AddMenuItem(nodePath);
                }
            }

            for (int i = 0; i < nodePaths.Count; i++)
            {
                string nodePath = nodePaths[i];
                if (createNodePaths.Contains(nodePath)) continue;
                AddMenuItem(nodePath);
            }

            void AddMenuItem(string nodePath)
            {
                MenuItem menuItem = nodeMap[nodePath];
                Texture2D icon = nullIcon;
                if (menuItem.info.icon != null) icon = menuItem.info.icon;

                string nodeName = nodePath;
                string[] parts = nodePath.Split('/');
                if (parts.Length > 1) nodeName = parts[parts.Length - 1];

                searchTreeEntries.Add(new SearchTreeEntry(new GUIContent(nodeName, icon)) {
                    level = menuItem.level + 1,
                    userData = menuItem.info
                });

                createNodePaths.Add(nodePath);
            }

            return searchTreeEntries;
        }

        /// <summary>
        /// 选择选项
        /// </summary>
        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            EditorWindow window = graphView.window;
            VisualElement windowRoot = this.graphView.window.rootVisualElement;
            Vector2 windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent, context.screenMousePosition - window.position.position);
            Vector2 graphMousePosition = graphView.contentViewContainer.WorldToLocal(windowMousePosition);

            CreateNodeInfo createNodeInfo = (CreateNodeInfo) SearchTreeEntry.userData;
            createNodeContext.createNodeConnector.targetPortId = createNodeInfo.portId;

            Undo.IncrementCurrentGroup();

            IEditorNodeView nodeView = this.graphView.nodeSystem.CreateNode(createNodeInfo.editorNodeAssetType, graphMousePosition, createNodeInfo.nodeData);
            if (string.IsNullOrEmpty(this.createNodeContext.createNodeConnector.originalNodeId) == false) CreateConnect(nodeView);

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            Undo.IncrementCurrentGroup();

            return true;
        }

        private void CreateConnect(IEditorNodeView nodeView)
        {
            IEditorNodeView originalNodeView = graphView.graphElementCache.GetEditorNodeView(createNodeContext.createNodeConnector.originalNodeId);
            IEditorPortView originalPortView = originalNodeView.GetPortView(createNodeContext.createNodeConnector.originalPortId);
            IEditorPortView targetPortView = nodeView.GetPortView(createNodeContext.createNodeConnector.targetPortId);

            if (string.IsNullOrEmpty(this.createNodeContext.createNodeConnector.edgeId))
            {
                if (originalPortView.portDirection == EditorPortDirection.Input) this.graphView.connectSystem.Connect(originalPortView, targetPortView);
                else this.graphView.connectSystem.Connect(targetPortView, originalPortView);
            }
            else
            {
                IEditorEdgeView edgeView = graphView.graphElementCache.edgeViewById.GetValueOrDefault(this.createNodeContext.createNodeConnector.edgeId);
                if (edgeView == null) return;

                this.graphView.RegisterCompleteObjectUndo("Graph RedirectionEdge");

                EditorEdgeAsset edgeAsset = edgeView.asset;
                if (edgeAsset.inputNodeId == this.createNodeContext.createNodeConnector.originalNodeId)
                {
                    edgeAsset.outputNodeId = nodeView.asset.id;
                    edgeAsset.outputPortId = this.createNodeContext.createNodeConnector.targetPortId;
                }
                else
                {
                    edgeAsset.inputNodeId = nodeView.asset.id;
                    edgeAsset.inputPortId = this.createNodeContext.createNodeConnector.targetPortId;
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

            if (handle!=null)
            {
                handle.Dispose(this.graphView);
                handle = null;
            }

            base.Dispose();
        }
    }
}