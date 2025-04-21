using Emilia.Node.Editor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    public class CreateNodeView : GraphPanel
    {
        private SearchField searchField;
        private TreeViewState treeViewState;
        private CreateNodeTreeView createNodeTreeView;

        public CreateNodeView()
        {
            name = nameof(CreateNodeView);

            IMGUIContainer container = new IMGUIContainer(OnTreeGUI);
            container.name = $"{nameof(CreateNodeView)}-TreeView";

            Add(container);
        }

        public override void Initialize(EditorGraphView graphView)
        {
            base.Initialize(graphView);

            searchField = new SearchField();

            schedule.Execute(() => {
                treeViewState = new TreeViewState();
                createNodeTreeView = new CreateNodeTreeView(graphView, treeViewState);
                createNodeTreeView.Reload();
            }).ExecuteLater(1);
        }

        public override void Dispose()
        {
            base.Dispose();
            treeViewState = null;
            createNodeTreeView = null;
        }

        void OnTreeGUI()
        {
            if (float.IsNaN(layout.width) || float.IsNaN(layout.height)) return;

            Rect rect = new Rect(0.0f, 0.0f, layout.width, layout.height);

            if (createNodeTreeView != null)
            {
                Rect searchRect = rect;
                searchRect.height = 20;

                createNodeTreeView.searchString = searchField.OnToolbarGUI(searchRect, createNodeTreeView.searchString);

                Rect treeRect = rect;
                treeRect.y += 20;

                createNodeTreeView.OnGUI(treeRect);
            }
        }
    }
}