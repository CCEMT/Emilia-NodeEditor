namespace Emilia.Node.Editor
{
    public class GraphSave : GraphViewModule
    {
        private IGraphSaveHandle handle;

        private bool _dirty;

        public bool dirty => this._dirty;
        public override int order => 500;

        public override void Reset(EditorGraphView graphView)
        {
            base.Reset(graphView);

            if (handle != null) EditorHandleUtility.ReleaseHandle(handle);
            handle = EditorHandleUtility.BuildHandle<IGraphSaveHandle>(this.graphView.graphAsset.GetType(), this.graphView);
        }

        public void SetDirty()
        {
            if (this.graphView.loadProgress != 1) return;
            this._dirty = true;
        }

        public void OnSave()
        {
            handle?.OnSaveBefore();

            if (this.graphView.graphAsset != null) graphView.graphAsset.Save();

            this._dirty = false;

            handle?.OnSaveAfter();
        }
    }
}