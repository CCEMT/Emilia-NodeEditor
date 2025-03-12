namespace Emilia.Node.Editor
{
    public interface INodeSystemHandle : IEditorHandle
    {
        /// <summary>
        /// 创建节点之后
        /// </summary>
        void OnCreateNode(IEditorNodeView editorNodeView);
    }
}