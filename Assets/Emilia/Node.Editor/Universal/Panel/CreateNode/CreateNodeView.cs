using Emilia.Node.Editor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    public class CreateNodeView : GraphPanel
    {
        private TreeViewState treeViewState;
        private CreateNodeTreeView createNodeTreeView;

        public CreateNodeView()
        {
            name = nameof(CreateNodeView);
            Add(new IMGUIContainer(OnTreeGUI));
        }

        public override void Initialize(EditorGraphView graphView)
        {
            base.Initialize(graphView);

            treeViewState = new TreeViewState();
            createNodeTreeView = new CreateNodeTreeView(graphView, treeViewState);
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
            if (createNodeTreeView != null) createNodeTreeView.OnGUI(rect);
        }
    }
}