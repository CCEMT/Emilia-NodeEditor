using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    public interface IGraphDragAndDropHandle : IEditorHandle
    {
        void DragUpdatedCallback(DragUpdatedEvent evt);

        void DragPerformedCallback(DragPerformEvent evt);
    }
}