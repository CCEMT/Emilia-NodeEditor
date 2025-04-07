using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Emilia.Node.Editor
{
    public interface IGraphCopyPasteHandle : IEditorHandle
    {
        /// <summary>
        /// 序列化处理
        /// </summary>
        string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements);

        /// <summary>
        /// 是否可以序列化数据
        /// </summary>
        bool CanPasteSerializedDataCallback(string serializedData);

        /// <summary>
        /// 反序列化处理
        /// </summary>
        void UnserializeAndPasteCallback(string operationName, string serializedData, GraphCopyPasteContext copyPasteContext);

        /// <summary>
        /// 创建拷贝
        /// </summary>
        object CreateCopy(object value);

        /// <summary>
        /// 获取拷贝的元素
        /// </summary>
        IEnumerable<GraphElement> GetCopyGraphElements(string serializedData);
    }
}