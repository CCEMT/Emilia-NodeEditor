using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Emilia.Node.Editor
{
    public class GraphCopyPaste
    {
        private EditorGraphView graphView;
        private IGraphCopyPasteHandle handle;

        public void Reset(EditorGraphView graphView)
        {
            this.graphView = graphView;
            if (handle != null) EditorHandleUtility.ReleaseHandle(handle);
            handle = EditorHandleUtility.BuildHandle<IGraphCopyPasteHandle>(graphView.graphAsset.GetType(), graphView);
        }

        public string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements)
        {
            if (this.handle == null) return string.Empty;
            return handle.SerializeGraphElementsCallback(elements);
        }

        public bool CanPasteSerializedDataCallback(string serializedData)
        {
            if (this.handle == null) return false;
            return this.handle.CanPasteSerializedDataCallback(serializedData);
        }

        public void UnserializeAndPasteCallback(string operationName, string serializedData)
        {
            if (this.handle == null) return;
            this.handle.UnserializeAndPasteCallback(operationName, serializedData);
        }

        public object CreateCopy(object value)
        {
            if (handle == default) return default;
            return handle.CreateCopy(value);
        }

        public void Dispose()
        {
            if (handle != null)
            {
                EditorHandleUtility.ReleaseHandle(handle);
                handle = null;
            }

            graphView = null;
        }
    }
}