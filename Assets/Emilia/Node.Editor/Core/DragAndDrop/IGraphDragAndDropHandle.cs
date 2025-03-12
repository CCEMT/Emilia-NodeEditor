using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    public interface IGraphDragAndDropHandle : IEditorHandle
    {
        /// <summary>
        /// 拖放更新回调
        /// </summary>
        void DragUpdatedCallback(DragUpdatedEvent evt);

        /// <summary>
        /// 拖放执行回调
        /// </summary>
        void DragPerformedCallback(DragPerformEvent evt);
    }
}