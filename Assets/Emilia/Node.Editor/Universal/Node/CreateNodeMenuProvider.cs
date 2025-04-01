using System.Collections.Generic;
using Emilia.Node.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    public class CreateNodeMenuProvider : ScriptableObject, ISearchWindowProvider
    {
        private CreateNodeContext _createNodeContext;
        public CreateNodeContext createNodeContext => _createNodeContext;

        public void Initialize(CreateNodeContext createNodeContext)
        {
            _createNodeContext = createNodeContext;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            if (this._createNodeContext.nodeMenu == null) return new List<SearchTreeEntry>() {new SearchTreeGroupEntry(new GUIContent("Error"))};
            return _createNodeContext.nodeMenu.OnCreateSearchTree(context);
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            if (this._createNodeContext.nodeMenu == null) return false;
            return _createNodeContext.nodeMenu.OnSelectEntry(SearchTreeEntry, context);
        }
    }
}