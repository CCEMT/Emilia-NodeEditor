using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    [GenericHandle]
    public class GraphDragAndDropHandle<T> : EditorHandle, IGraphDragAndDropHandle where T : EditorGraphAsset
    {
        public EditorGraphView smartValue { get; private set; }

        public IGraphDragAndDropHandle parentHandle { get; private set; }

        public override void Initialize(object weakSmartValue)
        {
            base.Initialize(weakSmartValue);
            smartValue = weakSmartValue as EditorGraphView;
            parentHandle = parent as IGraphDragAndDropHandle;
        }

        public virtual void DragUpdatedCallback(DragUpdatedEvent evt)
        {
            parentHandle?.DragUpdatedCallback(evt);
        }

        public virtual void DragPerformedCallback(DragPerformEvent evt)
        {
            parentHandle?.DragPerformedCallback(evt);
        }

        public override void Dispose()
        {
            base.Dispose();
            smartValue = null;
            parentHandle = null;
        }
    }
}