using System.Collections.Generic;
using Emilia.Node.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    public class CreateNodeMenuProvider : ScriptableObject, ISearchWindowProvider
    {
        private CreateNodeContext _createNodeContext;
        private EditorGraphView editorGraphView;
        public CreateNodeContext createNodeContext => _createNodeContext;

        public void Initialize(EditorGraphView graphView, CreateNodeContext createNodeContext)
        {
            editorGraphView = graphView;
            _createNodeContext = createNodeContext;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            if (this._createNodeContext.nodeMenu == null) return new List<SearchTreeEntry> {new SearchTreeGroupEntry(new GUIContent("Error"))};

            List<SearchTreeEntry> tree = new List<SearchTreeEntry>();
            tree.Add(new SearchTreeGroupEntry(new GUIContent(_createNodeContext.nodeMenu.GetTitle())));
            _createNodeContext.nodeMenu.CreateNodeTree(createNodeContext, (info) => CreateGroup(tree, info), (menuInfo) => CreateItem(tree, menuInfo));

            return tree;
        }

        private void CreateGroup(List<SearchTreeEntry> tree, CreateNodeMenuItem menuInfo)
        {
            tree.Add(new SearchTreeGroupEntry(new GUIContent(menuInfo.title), menuInfo.level));
        }

        private void CreateItem(List<SearchTreeEntry> tree, CreateNodeMenuItem menuInfo)
        {
            tree.Add(new SearchTreeEntry(new GUIContent(menuInfo.title, menuInfo.info.menuInfo.icon)) {
                level = menuInfo.level,
                userData = menuInfo.info
            });
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            CreateNodeInfo createNodeInfo = (CreateNodeInfo) SearchTreeEntry.userData;
            return this.editorGraphView.createNodeMenu.CreateNode(createNodeInfo, createNodeContext);
        }
    }
}