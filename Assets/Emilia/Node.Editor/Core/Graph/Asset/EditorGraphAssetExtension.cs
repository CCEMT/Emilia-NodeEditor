using System.Collections.Generic;

namespace Emilia.Node.Editor
{
    public static class EditorGraphAssetExtension
    {
        public static List<EditorNodeAsset> GetOutputNodes(this EditorGraphAsset graphAsset, EditorNodeAsset nodeAsset)
        {
            List<EditorNodeAsset> outputNodes = new List<EditorNodeAsset>();

            int edgeCount = graphAsset.edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                EditorEdgeAsset edgeAsset = graphAsset.edges[i];

                if (edgeAsset.outputNodeId != nodeAsset.id) continue;

                EditorNodeAsset outputNode = graphAsset.nodeMap.GetValueOrDefault(edgeAsset.inputNodeId);
                if (outputNode == null) continue;

                outputNodes.Add(outputNode);
            }

            return outputNodes;
        }

        public static List<EditorNodeAsset> GetInputNodes(this EditorGraphAsset graphAsset, EditorNodeAsset nodeAsset)
        {
            List<EditorNodeAsset> inputNodes = new List<EditorNodeAsset>();

            int edgeCount = graphAsset.edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                EditorEdgeAsset edgeAsset = graphAsset.edges[i];

                if (edgeAsset.inputNodeId != nodeAsset.id) continue;

                EditorNodeAsset inputNode = graphAsset.nodeMap.GetValueOrDefault(edgeAsset.outputNodeId);
                if (inputNode == null) continue;

                inputNodes.Add(inputNode);
            }

            return inputNodes;
        }

        public static List<EditorNodeAsset> GetAllOutputNodes(this EditorGraphAsset graphAsset, EditorNodeAsset nodeAsset)
        {
            List<EditorNodeAsset> outputNodes = new List<EditorNodeAsset>();

            int edgeCount = graphAsset.edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                EditorEdgeAsset edgeAsset = graphAsset.edges[i];

                if (edgeAsset.outputNodeId != nodeAsset.id) continue;

                EditorNodeAsset outputNode = graphAsset.nodeMap.GetValueOrDefault(edgeAsset.inputNodeId);
                if (outputNode == null) continue;

                outputNodes.Add(outputNode);
                outputNodes.AddRange(graphAsset.GetAllOutputNodes(outputNode));
            }

            return outputNodes;
        }

        public static List<EditorNodeAsset> GetAllInputNodes(this EditorGraphAsset graphAsset, EditorNodeAsset nodeAsset)
        {
            List<EditorNodeAsset> inputNodes = new List<EditorNodeAsset>();

            int edgeCount = graphAsset.edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                EditorEdgeAsset edgeAsset = graphAsset.edges[i];

                if (edgeAsset.inputNodeId != nodeAsset.id) continue;

                EditorNodeAsset inputNode = graphAsset.nodeMap.GetValueOrDefault(edgeAsset.outputNodeId);
                if (inputNode == null) continue;

                inputNodes.Add(inputNode);
                inputNodes.AddRange(graphAsset.GetAllInputNodes(inputNode));
            }

            return inputNodes;
        }

        public static List<EditorEdgeAsset> GetOutputEdges(this EditorGraphAsset graphAsset, EditorNodeAsset nodeAsset)
        {
            List<EditorEdgeAsset> outputEdges = new List<EditorEdgeAsset>();

            int edgeCount = graphAsset.edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                EditorEdgeAsset edgeAsset = graphAsset.edges[i];
                if (edgeAsset.outputNodeId != nodeAsset.id) continue;
                outputEdges.Add(edgeAsset);
            }

            return outputEdges;
        }

        public static List<EditorEdgeAsset> GetInputEdges(this EditorGraphAsset graphAsset, EditorNodeAsset nodeAsset)
        {
            List<EditorEdgeAsset> inputEdges = new List<EditorEdgeAsset>();

            int edgeCount = graphAsset.edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                EditorEdgeAsset edgeAsset = graphAsset.edges[i];
                if (edgeAsset.inputNodeId != nodeAsset.id) continue;
                inputEdges.Add(edgeAsset);
            }

            return inputEdges;
        }
    }
}