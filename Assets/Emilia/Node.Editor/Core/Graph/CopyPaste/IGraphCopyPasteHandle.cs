using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Emilia.Node.Editor
{
    public interface IGraphCopyPasteHandle : IEditorHandle
    {
        string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements);
        bool CanPasteSerializedDataCallback(string serializedData);
        void UnserializeAndPasteCallback(string operationName, string serializedData);
        object CreateCopy(object value);
    }
}