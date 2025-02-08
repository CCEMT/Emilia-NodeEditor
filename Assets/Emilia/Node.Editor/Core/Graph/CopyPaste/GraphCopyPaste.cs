using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Emilia.Node.Editor
{
    public class GraphCopyPaste : GraphViewModule
    {
        private IGraphCopyPasteHandle handle;
        public override int order => 300;

        public override void Reset(EditorGraphView graphView)
        {
            base.Reset(graphView);
            if (handle != null) EditorHandleUtility.ReleaseHandle(handle);
            handle = EditorHandleUtility.BuildHandle<IGraphCopyPasteHandle>(graphView.graphAsset.GetType(), graphView);
        }

        /// <summary>
        /// 序列化处理
        /// </summary>
        public string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements)
        {
            if (this.handle == null) return string.Empty;
            return handle.SerializeGraphElementsCallback(elements);
        }

        /// <summary>
        /// 是否可以序列化数据
        /// </summary>
        public bool CanPasteSerializedDataCallback(string serializedData)
        {
            if (this.handle == null) return false;
            return this.handle.CanPasteSerializedDataCallback(serializedData);
        }

        /// <summary>
        /// 反序列化处理
        /// </summary>
        public void UnserializeAndPasteCallback(string operationName, string serializedData)
        {
            if (this.handle == null) return;
            this.handle.UnserializeAndPasteCallback(operationName, serializedData);
        }

        /// <summary>
        /// 创建拷贝
        /// </summary>
        public object CreateCopy(object value)
        {
            if (handle == default) return default;
            return handle.CreateCopy(value);
        }

        public override void Dispose()
        {
            if (handle != null)
            {
                EditorHandleUtility.ReleaseHandle(handle);
                handle = null;
            }

            base.Dispose();
        }
    }
}