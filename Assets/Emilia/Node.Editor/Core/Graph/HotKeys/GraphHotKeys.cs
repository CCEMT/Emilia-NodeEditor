using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    public class GraphHotKeys : GraphViewModule
    {
        private IGraphHotKeysHandle handle;

        public override void Reset(EditorGraphView graphView)
        {
            base.Reset(graphView);

            if (this.handle != null) EditorHandleUtility.ReleaseHandle(this.handle);
            this.handle = EditorHandleUtility.BuildHandle<IGraphHotKeysHandle>(graphView.graphAsset.GetType(), graphView);

            graphView.UnregisterCallback<KeyDownEvent>(OnKeyDown);
            graphView.RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            this.handle?.OnKeyDown(evt);
        }

        public override void Dispose()
        {
            if (this.handle != null)
            {
                EditorHandleUtility.ReleaseHandle(this.handle);
                this.handle = null;
            }

            graphView.UnregisterCallback<KeyDownEvent>(OnKeyDown);

            base.Dispose();
        }
    }
}