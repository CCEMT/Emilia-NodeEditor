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
        private Dictionary<int, ICreateNodeHandle> createNodeHandleMap = new Dictionary<int, ICreateNodeHandle>();

        public CreateNodeTreeView(EditorGraphView graphView, TreeViewState state) : base(state)
        {
            baseIndent = 10;
            this.graphView = graphView;
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
            Dictionary<string, List<ICreateNodeHandle>> titleAndPriority = new Dictionary<string, List<ICreateNodeHandle>>();
            Dictionary<string, ICreateNodeHandle> nodeMap = new Dictionary<string, ICreateNodeHandle>();

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

                        if (titleAndPriority.ContainsKey(fullTitle) == false) titleAndPriority[fullTitle] = new List<ICreateNodeHandle>();

                        titleAndPriority[fullTitle].Add(createNodeHandle);
                    }
                }

                nodeMap[path] = createNodeHandle;
            }

            List<string> titlePaths = new List<string>();
            titlePaths.AddRange(titleAndPriority.Keys);

            titlePaths.Sort((a, b) => {

                List<ICreateNodeHandle> aItems = titleAndPriority[a];
                List<ICreateNodeHandle> bItems = titleAndPriority[b];

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

            List<string> nodePaths = new List<string>();
            nodePaths.AddRange(nodeMap.Keys);

            nodePaths.Sort((a, b) => {
                ICreateNodeHandle aItem = nodeMap[a];
                ICreateNodeHandle bItem = nodeMap[b];
                return aItem.priority.CompareTo(bItem.priority);
            });

            Dictionary<int, List<string>> groupLayerMap = new Dictionary<int, List<string>>();
            Dictionary<int, List<string>> nodeLayerMap = new Dictionary<int, List<string>>();

            for (int i = 0; i < titlePaths.Count; i++)
            {
                string title = titlePaths[i];
                string[] pathParts = title.Split('/');
                int layer = pathParts.Length - 1;

                if (groupLayerMap.ContainsKey(layer) == false) groupLayerMap[layer] = new List<string>();
                groupLayerMap[layer].Add(title);
            }

            for (int i = 0; i < nodePaths.Count; i++)
            {
                string title = nodePaths[i];
                string[] pathParts = title.Split('/');
                int layer = pathParts.Length - 1;

                if (nodeLayerMap.ContainsKey(layer) == false) nodeLayerMap[layer] = new List<string>();
                nodeLayerMap[layer].Add(title);
            }

            AddTreeItem(root, 0);

            void AddTreeItem(TreeViewItem parent, int layer)
            {
                if (groupLayerMap.TryGetValue(layer, out List<string> currentLayerTitle))
                {
                    int nextLayer = layer + 1;

                    for (int i = 0; i < currentLayerTitle.Count; i++)
                    {
                        string path = currentLayerTitle[i];

                        string[] pathParts = path.Split('/');
                        string title = pathParts.Length > 0 ? pathParts[pathParts.Length - 1] : path;

                        CreateNodeTitleTreeViewItem titleItem = new CreateNodeTitleTreeViewItem {
                            id = path.GetHashCode(),
                            depth = layer,
                            displayName = title,
                        };

                        SetExpanded(titleItem.id, true);

                        parent.AddChild(titleItem);

                        treeViewItems.Add(titleItem);

                        AddTreeItem(titleItem, nextLayer);
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

                        string groupTitle = path.Substring(0, path.LastIndexOf('/'));
                        if (parent is CreateNodeTitleTreeViewItem titleTreeViewItem)
                        {
                            if (titleTreeViewItem.displayName != groupTitle) continue;
                        }

                        CreateNodeEntryTreeViewItem nodeItem = new CreateNodeEntryTreeViewItem(createNodeHandle) {
                            id = path.GetHashCode(),
                            depth = layer + 1,
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

            for (int i = 0; i < nodePaths.Count; i++)
            {
                string path = nodePaths[i];
                ICreateNodeHandle createNodeHandle = nodeMap[path];

                string[] pathParts = path.Split('/');
                string title = pathParts.Length > 0 ? pathParts[pathParts.Length - 1] : path;

                if (SearchUtility.Search(title, searchString) == false) continue;

                CreateNodeEntryTreeViewItem nodeItem = new CreateNodeEntryTreeViewItem(createNodeHandle) {
                    id = path.GetHashCode(),
                    depth = 0,
                    displayName = title,
                };

                root.AddChild(nodeItem);

                treeViewItems.Add(nodeItem);
                createNodeHandleMap[nodeItem.id] = createNodeHandle;
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
    }
}