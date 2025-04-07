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

        public override void UnserializeAndPasteCallback(string operationName, string serializedData, GraphCopyPasteContext copyPasteContext)
        {
            CopyPasteGraph graph = SerializableUtility.FromJson<CopyPasteGraph>(serializedData);
            graph.StartPaste(copyPasteContext);
        }

        public override IEnumerable<GraphElement> GetCopyGraphElements(string serializedData)
        {
            try
            {
                CopyPasteGraph graph = SerializableUtility.FromJson<CopyPasteGraph>(serializedData);
                if (graph == null) return null;

                List<GraphElement> graphElements = new List<GraphElement>();
                List<ICopyPastePack> copyPastePacks = graph.GetAllPacks();

                int amount = copyPastePacks.Count;
                for (int i = 0; i < amount; i++)
                {
                    ICopyPastePack copyPastePack = copyPastePacks[i];

                    switch (copyPastePack)
                    {
                        case INodeCopyPastePack nodeCopyPastePack:
                            IEditorNodeView nodeView = smartValue.graphElementCache.nodeViewById.GetValueOrDefault(nodeCopyPastePack.copyAsset.id);
                            if (nodeView == null) continue;
                            graphElements.Add(nodeView.element);
                            break;

                        case IEdgeCopyPastePack edgeCopyPastePack:
                            IEditorEdgeView edgeView = smartValue.graphElementCache.edgeViewById.GetValueOrDefault(edgeCopyPastePack.copyAsset.id);
                            if (edgeView == null) continue;
                            graphElements.Add(edgeView.edgeElement);
                            break;

                        case IItemCopyPastePack itemCopyPastePack:
                            IEditorItemView itemView = smartValue.graphElementCache.itemViewById.GetValueOrDefault(itemCopyPastePack.copyAsset.id);
                            if (itemView == null) continue;
                            graphElements.Add(itemView.element);
                            break;
                    }
                }

                return graphElements;
            }
            catch
            {
                return null;
            }

            return null;
        }

        public override object CreateCopy(object value) => SerializationUtility.CreateCopy(value);
    }
}