using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Emilia.Node.Editor
{
    public class GraphCopyPaste : BasicGraphViewModule
    {
        private IGraphCopyPasteHandle handle;
        public override int order => 300;

        public override void Initialize(EditorGraphView graphView)
        {
            base.Initialize(graphView);
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
        public IEnumerable<GraphElement> UnserializeAndPasteCallback(string operationName, string serializedData, Vector2? mousePosition = null)
        {
            if (this.handle == null) return null;
            GraphCopyPasteContext graphCopyPasteContext = new GraphCopyPasteContext();
            graphCopyPasteContext.graphView = this.graphView;
            graphCopyPasteContext.createPosition = mousePosition;

            return this.handle.UnserializeAndPasteCallback(operationName, serializedData, graphCopyPasteContext);
        }

        public IEnumerable<GraphElement> GetCopyGraphElements(string serializedData)
        {
            if (this.handle == null) return null;
            return handle.GetCopyGraphElements(serializedData);
        }

        /// <summary>
        /// 创建拷贝
        /// </summary>
        public object CreateCopy(object value)
        {
            if (handle == null) return null;
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