using System.Collections.Generic;
using System.Linq;
using Emilia.Kit;
using Emilia.Node.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Emilia.Node.Universal.Editor
{
    public class CreateNodeTreeView : TreeView
    {
        private EditorGraphView graphView;
        private CreateNodeViewState createNodeViewState;
        private Dictionary<int, ICreateNodeHandle> createNodeHandleMap = new Dictionary<int, ICreateNodeHandle>();

        private bool isExpandAll = false;

        public CreateNodeTreeView(EditorGraphView graphView, CreateNodeViewState createNodeViewState, TreeViewState state) : base(state)
        {
            baseIndent = 10;
            this.graphView = graphView;
            this.createNodeViewState = createNodeViewState;
        }

        public void SetExpandAll()
        {
            isExpandAll = true;
        }

        protected override TreeViewItem BuildRoot() => new() {id = 0, depth = -1, displayName = "Root"};

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            List<TreeViewItem> treeViewItems = new List<TreeViewItem>();

            createNodeHandleMap.Clear();

            if (hasSearch == false) AddNormalItem(treeViewItems, root);
            else AddSearchItem(treeViewItems, root);

            return treeViewItems;
        }

        private void AddNormalItem(List<TreeViewItem> treeViewItems, TreeViewItem root)
        {
            Dictionary<string, List<ICreateNodeHandle>> titleToHandles = new Dictionary<string, List<ICreateNodeHandle>>();
            Dictionary<string, ICreateNodeHandle> nodeMap = new Dictionary<string, ICreateNodeHandle>();

            BuildMaps();

            List<string> titlePaths = SortTitlePaths();
            List<string> nodePaths = SortNodePaths();
            Dictionary<int, List<string>> groupLayerMap = BuildLayerMap(titlePaths);
            Dictionary<int, List<string>> nodeLayerMap = BuildLayerMap(nodePaths);

            AddTreeItems(root, 0);

            isExpandAll = false;

            void BuildMaps()
            {
                int itemCount = graphView.createNodeMenu.createNodeHandleCacheList.Count;
                for (int i = 0; i < itemCount; i++)
                {
                    ICreateNodeHandle createNodeHandle = graphView.createNodeMenu.createNodeHandleCacheList[i];
                    string path = createNodeHandle.path;

                    string[] pathParts = path.Split('/');

                    if (pathParts.Length > 1)
                    {
                        string fullTitle = "";
                        int partAmount = pathParts.Length;
                        for (int j = 0; j < partAmount - 1; j++)
                        {
                            string title = pathParts[j];

                            if (string.IsNullOrEmpty(fullTitle)) fullTitle = title;
                            else fullTitle += $"/{title}";

                            if (titleToHandles.ContainsKey(fullTitle) == false) titleToHandles[fullTitle] = new List<ICreateNodeHandle>();

                            titleToHandles[fullTitle].Add(createNodeHandle);
                        }
                    }

                    nodeMap[path] = createNodeHandle;
                }
            }

            List<string> SortTitlePaths()
            {
                List<string> resultTitlePaths = new List<string>();
                resultTitlePaths.AddRange(titleToHandles.Keys);

                resultTitlePaths.Sort((a, b) => {
                    List<ICreateNodeHandle> aItems = titleToHandles[a];
                    List<ICreateNodeHandle> bItems = titleToHandles[b];

                    int aMaxPriority = int.MinValue;
                    int bMaxPriority = int.MinValue;

                    for (var i = 0; i < aItems.Count; i++)
                    {
                        ICreateNodeHandle item = aItems[i];
                        if (item.priority > aMaxPriority) aMaxPriority = item.priority;
                    }

                    for (var i = 0; i < bItems.Count; i++)
                    {
                        ICreateNodeHandle item = bItems[i];
                        if (item.priority > bMaxPriority) bMaxPriority = item.priority;
                    }

                    return aMaxPriority.CompareTo(bMaxPriority);
                });

                return resultTitlePaths;
            }

            List<string> SortNodePaths()
            {
                List<string> resultNodePaths = new List<string>();
                resultNodePaths.AddRange(nodeMap.Keys);

                resultNodePaths.Sort((a, b) => {
                    ICreateNodeHandle aItem = nodeMap[a];
                    ICreateNodeHandle bItem = nodeMap[b];
                    return aItem.priority.CompareTo(bItem.priority);
                });

                return resultNodePaths;
            }

            Dictionary<int, List<string>> BuildLayerMap(List<string> paths)
            {
                Dictionary<int, List<string>> layerMap = new Dictionary<int, List<string>>();

                for (int i = 0; i < paths.Count; i++)
                {
                    string title = paths[i];
                    string[] pathParts = title.Split('/');
                    int layer = pathParts.Length - 1;

                    if (layerMap.ContainsKey(layer) == false) layerMap[layer] = new List<string>();
                    layerMap[layer].Add(title);
                }

                return layerMap;
            }

            void AddTreeItems(TreeViewItem parent, int layer)
            {
                if (groupLayerMap.TryGetValue(layer, out List<string> currentLayerTitle))
                {
                    int nextLayer = layer + 1;

                    for (int i = 0; i < currentLayerTitle.Count; i++)
                    {
                        string path = currentLayerTitle[i];

                        int lastIndex = path.LastIndexOf('/');
                        string parentPath = "";
                        if (lastIndex > 0) parentPath = path.Substring(0, lastIndex);

                        if (parent is CreateNodeTitleTreeViewItem parentTitleItem)
                        {
                            if (parentTitleItem.id != parentPath.GetHashCode()) continue;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(parentPath) == false) continue;
                        }

                        string[] pathParts = path.Split('/');
                        string title = pathParts.Length > 0 ? pathParts[pathParts.Length - 1] : path;

                        CreateNodeTitleTreeViewItem titleItem = new CreateNodeTitleTreeViewItem {
                            id = path.GetHashCode(),
                            depth = layer,
                            displayName = title,
                        };

                        parent.AddChild(titleItem);
                        treeViewItems.Add(titleItem);

                        if (isExpandAll) SetExpanded(titleItem.id, true);

                        bool isExpanded = IsExpanded(titleItem.id);

                        if (isExpanded) AddTreeItems(titleItem, nextLayer);
                        else titleItem.children = CreateChildListForCollapsedParent();
                    }
                }

                if (nodeLayerMap.TryGetValue(layer, out List<string> currentLayerNode))
                {
                    for (int i = 0; i < currentLayerNode.Count; i++)
                    {
                        string path = currentLayerNode[i];
                        ICreateNodeHandle createNodeHandle = nodeMap[path];

                        string[] pathParts = path.Split('/');
                        string title = pathParts.Length > 0 ? pathParts[pathParts.Length - 1] : path;

                        int lastIndex = path.LastIndexOf('/');
                        string groupTitle = "";
                        if (lastIndex > 0) groupTitle = path.Substring(0, lastIndex);

                        if (parent is CreateNodeTitleTreeViewItem titleTreeViewItem)
                        {
                            if (titleTreeViewItem.id != groupTitle.GetHashCode()) continue;
                        }

                        CreateNodeEntryTreeViewItem nodeItem = new CreateNodeEntryTreeViewItem(createNodeHandle) {
                            id = path.GetHashCode(),
                            depth = layer,
                            displayName = title,
                        };

                        parent.AddChild(nodeItem);

                        treeViewItems.Add(nodeItem);
                        createNodeHandleMap[nodeItem.id] = createNodeHandle;
                    }
                }
            }
        }

        void AddSearchItem(List<TreeViewItem> treeViewItems, TreeViewItem root)
        {
            Dictionary<string, ICreateNodeHandle> nodeMap = new Dictionary<string, ICreateNodeHandle>();
            int itemCount = graphView.createNodeMenu.createNodeHandleCacheList.Count;
            for (int i = 0; i < itemCount; i++)
            {
                ICreateNodeHandle createNodeHandle = graphView.createNodeMenu.createNodeHandleCacheList[i];
                string path = createNodeHandle.path;
                nodeMap[path] = createNodeHandle;
            }

            List<string> nodePaths = new List<string>();
            nodePaths.AddRange(nodeMap.Keys);

            nodePaths.Sort((a, b) => {
                ICreateNodeHandle aItem = nodeMap[a];
                ICreateNodeHandle bItem = nodeMap[b];
                return aItem.priority.CompareTo(bItem.priority);
            });

            List<(CreateNodeEntryTreeViewItem, int)> collects = new();

            for (int i = 0; i < nodePaths.Count; i++)
            {
                string path = nodePaths[i];
                ICreateNodeHandle createNodeHandle = nodeMap[path];

                string[] pathParts = path.Split('/');
                string title = pathParts.Length > 0 ? pathParts[pathParts.Length - 1] : path;

                int score = SearchUtility.Search(title, searchString);
                if (score == 0) continue;

                CreateNodeEntryTreeViewItem nodeItem = new CreateNodeEntryTreeViewItem(createNodeHandle) {
                    id = path.GetHashCode(),
                    depth = 0,
                    displayName = title,
                };

                collects.Add((nodeItem, score));
            }

            collects.Sort((a, b) => b.Item2.CompareTo(a.Item2));

            foreach (var pair in collects)
            {
                root.AddChild(pair.Item1);
                treeViewItems.Add(pair.Item1);
                createNodeHandleMap[pair.Item1.id] = pair.Item1.createNodeHandle;
            }
        }

        protected override bool CanStartDrag(CanStartDragArgs args) => args.draggedItem is CreateNodeEntryTreeViewItem;

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            int id = args.draggedItemIDs.FirstOrDefault();
            if (createNodeHandleMap.TryGetValue(id, out ICreateNodeHandle createNodeHandle))
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData(UniversalDragAndDropHandle.CreateNodeDragAndDropType, createNodeHandle);
                DragAndDrop.StartDrag(createNodeHandle.path);
            }
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args) => DragAndDropVisualMode.Move;

        protected override void ExpandedStateChanged()
        {
            this.createNodeViewState.SetExpandedIDs(state.expandedIDs);
            this.createNodeViewState.Save(this.graphView);
        }
    }
}