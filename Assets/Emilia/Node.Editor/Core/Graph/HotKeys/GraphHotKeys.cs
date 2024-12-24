using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    public class GraphHotKeys
    {
        private EditorGraphView graphView;
        private IGraphHotKeysHandle handle;

        public void Reset(EditorGraphView graphView)
        {
            this.graphView = graphView;

            if (this.handle != null) EditorHandleUtility.ReleaseHandle(this.handle);
            this.handle = EditorHandleUtility.BuildHandle<IGraphHotKeysHandle>(graphView.graphAsset.GetType(), graphView);

            graphView.UnregisterCallback<KeyDownEvent>(OnKeyDown);
            graphView.RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            this.handle?.OnKeyDown(evt);
        }

        public void Dispose()
        {
            if (this.handle != null)
            {
                EditorHandleUtility.ReleaseHandle(this.handle);
                this.handle = null;
            }

            graphView.UnregisterCallback<KeyDownEvent>(OnKeyDown);
            graphView = null;
        }
    }
}