using Emilia.Kit;
using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public abstract class GraphDragAndDropHandle
    {
        public virtual void DragUpdatedCallback(EditorGraphView graphView, DragUpdatedEvent evt) { }
        public virtual void DragPerformedCallback(EditorGraphView graphView, DragPerformEvent evt) { }
    }
}