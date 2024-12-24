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
            this.smartValue = weakSmartValue as EditorGraphView;
            this.parentHandle = parent as IGraphCopyPasteHandle;
        }

        public virtual string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements)
        {
            return parentHandle?.SerializeGraphElementsCallback(elements);
        }

        public virtual bool CanPasteSerializedDataCallback(string serializedData)
        {
            return parentHandle?.CanPasteSerializedDataCallback(serializedData) ?? false;
        }

        public virtual void UnserializeAndPasteCallback(string operationName, string serializedData)
        {
            parentHandle?.UnserializeAndPasteCallback(operationName, serializedData);
        }

        public virtual object CreateCopy(object value)
        {
            return parentHandle?.CreateCopy(value);
        }

        public override void Dispose()
        {
            base.Dispose();
            this.smartValue = default;
        }
    }
}