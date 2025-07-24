using System.Collections.Generic;
using Emilia.Kit;
using Emilia.Node.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.IMGUI.Controls;

namespace Emilia.Node.Universal.Editor
{
    public class SearchNodeTreeView : TreeView
    {
        private EditorGraphView graphView;
        private Dictionary<int, IEditorNodeView> nodeViews = new();

        public SearchNodeTreeView(EditorGraphView graphView, TreeViewState state) : base(state)
        {
            baseIndent = 10;
            this.graphView = graphView;
        }

        protected override TreeViewItem BuildRoot() => new() {id = 0, depth = -1, displayName = "Root"};

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            List<TreeViewItem> treeViewItems = new List<TreeViewItem>();

            nodeViews.Clear();

            if (hasSearch == false) AddNormalItem(treeViewItems, root);
            else AddSearchItem(treeViewItems, root);

            return treeViewItems;
        }

        private void AddNormalItem(List<TreeViewItem> treeViewItems, TreeViewItem root)
        {
            int count = this.graphView.nodeViews.Count;
            for (int i = 0; i < count; i++)
            {
                IEditorNodeView nodeView = this.graphView.nodeViews[i];
                int id = nodeView.asset.id.GetHashCode();
                nodeViews.Add(id, nodeView);

                string displayName = ObjectDescriptionUtility.GetDescription(nodeView.asset);
                if (string.IsNullOrEmpty(displayName)) displayName = nodeView.asset.name;

                TreeViewItem item = new TreeViewItem(id, 0, displayName);

                root.AddChild(item);
                treeViewItems.Add(item);
            }
        }

        private void AddSearchItem(List<TreeViewItem> treeViewItems, TreeViewItem root)
        {
            List<(TreeViewItem, int)> collects = new List<(TreeViewItem, int)>();

            int count = this.graphView.nodeViews.Count;
            for (int i = 0; i < count; i++)
            {
                IEditorNodeView nodeView = this.graphView.nodeViews[i];

                int id = nodeView.asset.id.GetHashCode();
                nodeViews.Add(id, nodeView);

                string displayName = ObjectDescriptionUtility.GetDescription(nodeView.asset);
                if (string.IsNullOrEmpty(displayName)) displayName = nodeView.asset.name;

                int score = SearchUtility.Search(displayName, searchString);
                if (score == 0) continue;

                TreeViewItem item = new TreeViewItem(id, 0, displayName);
                collects.Add((item,score));
            }
            
            collects.Sort((a, b) => b.Item2.CompareTo(a.Item2));
            
            foreach (var collect in collects)
            {
                root.AddChild(collect.Item1);
                treeViewItems.Add(collect.Item1);
            }
        }

        protected override void SingleClickedItem(int id)
        {
            base.SingleClickedItem(id);
            IEditorNodeView nodeView = nodeViews.GetValueOrDefault(id);
            if (nodeView == null) return;

            this.graphView.SetSelection(new List<ISelectable> {nodeView.element});
            this.graphView.FrameSelection();
        }
    }
}