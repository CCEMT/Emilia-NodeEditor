namespace Emilia.Node.Editor
{
    public struct NodeCache
    {
        public object nodeData;
        public IEditorNodeView nodeView;

        public NodeCache(object nodeData, IEditorNodeView nodeView)
        {
            this.nodeData = nodeData;
            this.nodeView = nodeView;
        }
    }
}