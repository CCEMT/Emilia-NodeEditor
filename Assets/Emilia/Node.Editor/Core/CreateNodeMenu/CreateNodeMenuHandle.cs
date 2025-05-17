using System.Collections.Generic;
using Emilia.Kit;
using UnityEditor.Experimental.GraphView;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public abstract class CreateNodeMenuHandle
    {
        public virtual string GetTitle(EditorGraphView graphView) => "Create Node";
        public virtual void Initialize(EditorGraphView graphView) { }
        public virtual void InitializeCache(EditorGraphView graphView, List<ICreateNodeHandle> createNodeHandles) { }
        public virtual void MenuCreateInitialize(EditorGraphView graphView, CreateNodeContext createNodeContext) { }
        public virtual void ShowCreateNodeMenu(EditorGraphView graphView, NodeCreationContext c) { }
        public virtual void CollectAllCreateNodeInfos(EditorGraphView graphView, List<CreateNodeInfo> createNodeInfos, CreateNodeContext createNodeContext) { }
        public virtual void Dispose(EditorGraphView graphView) { }
    }
}