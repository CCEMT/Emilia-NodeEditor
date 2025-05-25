using Emilia.Kit;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public abstract class ItemSystemHandle
    {
        /// <summary>
        /// 创建Item视图处理
        /// </summary>
        public virtual void OnCreateItem(EditorGraphView graphView, IEditorItemView itemView) { }
    }
}