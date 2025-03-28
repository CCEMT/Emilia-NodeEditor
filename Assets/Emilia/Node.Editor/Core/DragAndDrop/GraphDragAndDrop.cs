using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    public class GraphDragAndDrop : BasicGraphViewModule
    {
        private IGraphDragAndDropHandle handle;
        public override int order => 1500;

        public override void Initialize(EditorGraphView graphView)
        {
            base.Initialize(graphView);
            this.handle = EditorHandleUtility.BuildHandle<IGraphDragAndDropHandle>(graphView.graphAsset.GetType(), graphView);

            graphView.UnregisterCallback<DragUpdatedEvent>(DragUpdatedCallback);
            graphView.UnregisterCallback<DragPerformEvent>(DragPerformedCallback);

            graphView.RegisterCallback<DragUpdatedEvent>(DragUpdatedCallback);
            graphView.RegisterCallback<DragPerformEvent>(DragPerformedCallback);
        }

        private void DragUpdatedCallback(DragUpdatedEvent evt)
        {
            handle?.DragUpdatedCallback(evt);
        }

        private void DragPerformedCallback(DragPerformEvent evt)
        {
            handle?.DragPerformedCallback(evt);
        }

        public override void Dispose()
        {
            if (graphView == null) return;

            graphView.UnregisterCallback<DragUpdatedEvent>(DragUpdatedCallback);
            graphView.UnregisterCallback<DragPerformEvent>(DragPerformedCallback);

            if (handle != null)
            {
                EditorHandleUtility.ReleaseHandle(handle);
                handle = null;
            }

            base.Dispose();
        }
    }
}