using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    public class GraphHotKeys : BasicGraphViewModule
    {
        private IGraphHotKeysHandle handle;
        public override int order => 800;

        public override void Initialize(EditorGraphView graphView)
        {
            base.Initialize(graphView);
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

            if (graphView != null) graphView.UnregisterCallback<KeyDownEvent>(OnKeyDown);

            base.Dispose();
        }
    }
}