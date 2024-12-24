using UnityEngine;

namespace Emilia.Node.Editor
{
    public class GraphOperate
    {
        private EditorGraphView graphView;
        private IGraphOperateHandle handle;

        public void Reset(EditorGraphView graphView)
        {
            this.graphView = graphView;
            if (handle != null) EditorHandleUtility.ReleaseHandle(handle);
            handle = EditorHandleUtility.BuildHandle<IGraphOperateHandle>(graphView.graphAsset.GetType(), graphView);
        }

        public void OpenCreateNodeMenu(Vector2 mousePosition, CreateNodeContext createNodeContext = default)
        {
            handle?.OpenCreateNodeMenu(mousePosition, createNodeContext);
        }

        public void Cut()
        {
            handle?.Cut();
        }

        public void Copy()
        {
            handle?.Copy();
        }

        public void Paste()
        {
            handle?.Paste();
        }

        public void Delete()
        {
            handle?.Delete();
        }

        public void Duplicate()
        {
            handle?.Duplicate();
        }

        public void Save()
        {
            handle?.Save();
        }

        public void Dispose()
        {
            if (handle != null)
            {
                EditorHandleUtility.ReleaseHandle(handle);
                handle = null;
            }

            this.graphView = null;
        }
    }
}