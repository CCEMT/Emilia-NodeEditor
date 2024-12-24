namespace Emilia.Node.Editor
{
    public struct NodeCache
    {
        public object userData;
        public IEditorNodeView nodeView;

        public NodeCache(object userData, IEditorNodeView nodeView)
        {
            this.userData = userData;
            this.nodeView = nodeView;
        }
    }
}