using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Emilia.Node.Editor
{
    [GenericHandle]
    public abstract class GraphCopyPasteHandle<T> : EditorHandle, IGraphCopyPasteHandle where T : EditorGraphAsset
    {
        public EditorGraphView smartValue { get; private set; }
        public IGraphCopyPasteHandle parentHandle { get; private set; }

        public override void Initialize(object weakSmartValue)
        {
            base.Initialize(weakSmartValue);
            smartValue = weakSmartValue as EditorGraphView;
            parentHandle = parent as IGraphCopyPasteHandle;
        }

        public virtual string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements)
        {
            return parentHandle?.SerializeGraphElementsCallback(elements);
        }

        public virtual bool CanPasteSerializedDataCallback(string serializedData)
        {
            return parentHandle?.CanPasteSerializedDataCallback(serializedData) ?? false;
        }

        public virtual IEnumerable<GraphElement> UnserializeAndPasteCallback(string operationName, string serializedData, GraphCopyPasteContext copyPasteContext)
        {
            return parentHandle?.UnserializeAndPasteCallback(operationName, serializedData, copyPasteContext);
        }

        public virtual object CreateCopy(object value)
        {
            return parentHandle?.CreateCopy(value);
        }

        public virtual IEnumerable<GraphElement> GetCopyGraphElements(string serializedData)
        {
            return parentHandle?.GetCopyGraphElements(serializedData);
        }

        public override void Dispose()
        {
            base.Dispose();
            smartValue = null;
            parentHandle = null;
        }
    }
}