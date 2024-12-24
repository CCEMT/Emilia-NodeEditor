using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Emilia.Node.Editor
{
    public interface ICreateNodeMenuHandle : IEditorHandle
    {
        string title { get; }

        void InitializeCache();

        void MenuCreateInitialize(CreateNodeContext createNodeContext);

        void ShowCreateNodeMenu(NodeCreationContext nodeCreationContext);
        
        void CollectAllCreateNodeInfos(List<CreateNodeInfo> createNodeInfos, CreateNodeContext createNodeContext);
    }
}