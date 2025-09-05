namespace Emilia.Node.Editor
{
    public class NodeCache
    {
        /// <summary>
        /// 节点数据
        /// </summary>
        public object nodeData;

        /// <summary>
        /// 节点视图
        /// </summary>
        public IEditorNodeView nodeView;

        public NodeCache(object nodeData, IEditorNodeView nodeView)
        {
            this.nodeData = nodeData;
            this.nodeView = nodeView;
        }
    }
}