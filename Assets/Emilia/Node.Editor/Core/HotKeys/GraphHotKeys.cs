using Emilia.Kit;
using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    public class GraphHotKeys : BasicGraphViewModule
    {
        private GraphHotKeysHandle handle;
        public override int order => 800;

        public override void Initialize(EditorGraphView graphView)
        {
            base.Initialize(graphView);
            this.handle = EditorHandleUtility.CreateHandle<GraphHotKeysHandle>(graphView.graphAsset.GetType());
            this.handle.Initialize(graphView);

            graphView.UnregisterCallback<KeyDownEvent>(OnKeyDown);
            graphView.RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            this.handle?.OnKeyDown(this.graphView, evt);
        }

        public override void Dispose()
        {
            if (this.handle != null)
            {
                this.handle.Dispose();
                this.handle = null;
            }

            if (graphView != null) graphView.UnregisterCallback<KeyDownEvent>(OnKeyDown);

            base.Dispose();
        }
    }
}