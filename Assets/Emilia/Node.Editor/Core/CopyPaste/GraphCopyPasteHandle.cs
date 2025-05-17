using System.Collections.Generic;
using Emilia.Kit;
using UnityEditor.Experimental.GraphView;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public abstract class GraphCopyPasteHandle
    {
        public virtual string SerializeGraphElementsCallback(EditorGraphView graphView, IEnumerable<GraphElement> elements) => null;
        public virtual bool CanPasteSerializedDataCallback(EditorGraphView graphView, string serializedData) => false;
        public virtual IEnumerable<GraphElement> UnserializeAndPasteCallback(EditorGraphView graphView, string operationName, string serializedData, GraphCopyPasteContext copyPasteContext) => null;
        public virtual object CreateCopy(EditorGraphView graphView, object value) => null;
        public virtual IEnumerable<GraphElement> GetCopyGraphElements(EditorGraphView graphView, string serializedData) => null;
    }
}