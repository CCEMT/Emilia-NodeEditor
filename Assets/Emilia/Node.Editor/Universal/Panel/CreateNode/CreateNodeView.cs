﻿using Emilia.Node.Editor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    public class CreateNodeView : GraphPanel
    {
        private const float SearchFieldHeight = 20;
        
        private SearchField searchField;
        private CreateNodeViewState createNodeViewState;
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
            schedule.Execute(OnInitialize).ExecuteLater(1);
        }

        private void OnInitialize()
        {
            createNodeViewState = CreateNodeViewState.Get(graphView);
            treeViewState = new TreeViewState();
            
            treeViewState.expandedIDs.AddRange(createNodeViewState.expandedIDs);

            createNodeTreeView = new CreateNodeTreeView(graphView, createNodeViewState, treeViewState);

            string saveKey = CreateNodeViewState.CreateNodeViewStateSaveKey;
            if (graphView.graphLocalSettingSystem.HasTypeSetting(saveKey) == false) createNodeTreeView.SetExpandAll();

            createNodeTreeView.Reload();
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
                searchRect.height = SearchFieldHeight;

                createNodeTreeView.searchString = searchField.OnToolbarGUI(searchRect, createNodeTreeView.searchString);

                Rect treeRect = rect;
                treeRect.y += SearchFieldHeight;
                treeRect.height -= SearchFieldHeight;

                createNodeTreeView.OnGUI(treeRect);
            }
        }
    }
}