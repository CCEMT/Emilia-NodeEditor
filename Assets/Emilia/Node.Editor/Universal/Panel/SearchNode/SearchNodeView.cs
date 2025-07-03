using System.Collections.Generic;
using Emilia.Kit;
using Emilia.Node.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    public class SearchNodeView : GraphPanel
    {
        private SearchField searchField;
        private TreeViewState treeViewState;
        private SearchNodeTreeView searchNodeTreeView;

        private List<UniversalEditorNodeView> dimNodeViews = new List<UniversalEditorNodeView>();

        public SearchNodeView()
        {
            name = nameof(SearchNodeView);

            IMGUIContainer container = new IMGUIContainer(OnTreeGUI);
            container.name = $"{nameof(SearchNodeView)}-TreeView";

            Add(container);
        }

        public override void Initialize(EditorGraphView graphView)
        {
            base.Initialize(graphView);

            searchField = new SearchField();

            schedule.Execute(() => {
                treeViewState = new TreeViewState();
                searchNodeTreeView = new SearchNodeTreeView(graphView, treeViewState);
                searchNodeTreeView.Reload();
            }).ExecuteLater(1);
        }

        public override void Dispose()
        {
            base.Dispose();
            treeViewState = null;
            searchNodeTreeView = null;
            ClearDim();
        }

        void OnTreeGUI()
        {
            if (float.IsNaN(layout.width) || float.IsNaN(layout.height)) return;

            Rect rect = new Rect(0.0f, 0.0f, layout.width, layout.height);

            if (searchNodeTreeView != null)
            {
                Rect searchRect = rect;
                searchRect.height = 20;

                EditorGUI.BeginChangeCheck();
                searchNodeTreeView.searchString = searchField.OnToolbarGUI(searchRect, searchNodeTreeView.searchString);
                if (EditorGUI.EndChangeCheck()) RefreshDim();

                Rect treeRect = rect;
                treeRect.y += 20;

                searchNodeTreeView.OnGUI(treeRect);
            }
        }

        private void RefreshDim()
        {
            ClearDim();

            int count = this.graphView.nodeViews.Count;
            for (int i = 0; i < count; i++)
            {
                IEditorNodeView nodeView = this.graphView.nodeViews[i];
                UniversalEditorNodeView universalNodeView = nodeView as UniversalEditorNodeView;
                if (universalNodeView == null) continue;

                string displayName = ObjectDescriptionUtility.GetDescription(nodeView.asset);
                if (string.IsNullOrEmpty(displayName)) displayName = nodeView.asset.name;

                if (SearchUtility.Search(displayName, searchNodeTreeView.searchString)) continue;

                universalNodeView.SetDisabled();
                dimNodeViews.Add(universalNodeView);
            }
        }

        private void ClearDim()
        {
            int count = dimNodeViews.Count;
            for (int i = 0; i < count; i++)
            {
                UniversalEditorNodeView nodeView = dimNodeViews[i];
                if (nodeView == null) continue;
                nodeView.ClearDisabled();
            }

            dimNodeViews.Clear();
        }
    }
}