using System.Collections.Generic;
using Emilia.Kit;
using Emilia.Node.Editor;
using Sirenix.Serialization;
using UnityEditor.Experimental.GraphView;

namespace Emilia.Node.Universal.Editor
{
    public class UniversalGraphCopyPasteHandle : GraphCopyPasteHandle<EditorUniversalGraphAsset>
    {
        public override string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements)
        {
            CopyPasteGraph graph = new CopyPasteGraph();

            foreach (GraphElement element in elements)
            {
                IGraphCopyPasteElement copyPasteElement = element as IGraphCopyPasteElement;
                if (copyPasteElement == null) continue;
                graph.AddPack(copyPasteElement.GetPack());
            }

            return SerializableUtility.ToJson(graph);
        }

        public override bool CanPasteSerializedDataCallback(string serializedData)
        {
            try
            {
                return SerializableUtility.FromJson<CopyPasteGraph>(serializedData) != null;
            }
            catch
            {
                return false;
            }
        }

        public override void UnserializeAndPasteCallback(string operationName, string serializedData)
        {
            CopyPasteGraph graph = SerializableUtility.FromJson<CopyPasteGraph>(serializedData);
            graph.StartPaste(smartValue);
        }

        public override object CreateCopy(object value)
        {
            return SerializationUtility.CreateCopy(value);
        }
    }
}