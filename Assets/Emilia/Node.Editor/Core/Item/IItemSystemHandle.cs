namespace Emilia.Node.Editor
{
    public interface IItemSystemHandle : IEditorHandle
    {
        /// <summary>
        /// 创建之后
        /// </summary>
        void OnCreateItem(IEditorItemView itemView);
    }
}