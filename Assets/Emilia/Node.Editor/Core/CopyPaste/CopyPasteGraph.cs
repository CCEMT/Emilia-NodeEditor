using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEditor;

namespace Emilia.Node.Editor
{
    [Serializable]
    public class CopyPasteGraph
    {
        [OdinSerialize, NonSerialized]
        private List<CopyPasteNode> nodes = new List<CopyPasteNode>();

        public void StartPaste(object userData)
        {
            Undo.IncrementCurrentGroup();

            Queue<CopyPasteNode> nodeQueue = new Queue<CopyPasteNode>();

            int nodeAmount = this.nodes.Count;
            for (int i = 0; i < nodeAmount; i++)
            {
                CopyPasteNode node = nodes[i];
                if (node.input.Count == 0) nodeQueue.Enqueue(node);
            }

            List<CopyPasteNode> successNodes = new List<CopyPasteNode>();

            while (nodeQueue.Count > 0)
            {
                CopyPasteNode node = nodeQueue.Dequeue();

                bool isZero = node.input.Count == 0;
                bool isSuccess = node.input.TrueForAll(outputNode => successNodes.Contains(outputNode));
                bool canPaste = isZero || isSuccess;

                if (canPaste)
                {
                    CopyPasteContext copyPasteContext = new CopyPasteContext();
                    copyPasteContext.userData = userData;
                    copyPasteContext.dependency = GetInputPack(node);

                    node.pack.Paste(copyPasteContext);
                    successNodes.Add(node);

                    int inputAmount = node.output.Count;
                    for (int i = 0; i < inputAmount; i++)
                    {
                        CopyPasteNode outputNode = node.output[i];
                        if (successNodes.Contains(outputNode) || nodeQueue.Contains(outputNode)) continue;
                        nodeQueue.Enqueue(outputNode);
                    }

                    continue;
                }

                nodeQueue.Enqueue(node);
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            Undo.IncrementCurrentGroup();
        }

        private List<ICopyPastePack> GetInputPack(CopyPasteNode node)
        {
            List<ICopyPastePack> inputPacks = new List<ICopyPastePack>();
            int amount = node.input.Count;
            for (int i = 0; i < amount; i++)
            {
                CopyPasteNode inputNode = node.input[i];
                inputPacks.Add(inputNode.pack);
            }

            return inputPacks;
        }

        public void AddPack(ICopyPastePack pack)
        {
            CopyPasteNode addNode = new CopyPasteNode(pack);

            int amount = this.nodes.Count;
            for (int i = 0; i < amount; i++)
            {
                CopyPasteNode node = nodes[i];
                if (node.pack.CanDependency(addNode.pack))
                {
                    node.input.Add(addNode);
                    addNode.output.Add(node);
                }

                if (addNode.pack.CanDependency(node.pack))
                {
                    node.output.Add(addNode);
                    addNode.input.Add(node);
                }
            }

            nodes.Add(addNode);
        }

        public void RemovePack(ICopyPastePack pack)
        {
            CopyPasteNode removeNode = null;
            int nodeAmount = this.nodes.Count;
            for (int i = 0; i < nodeAmount; i++)
            {
                CopyPasteNode node = nodes[i];
                if (node.pack != pack) continue;
                removeNode = node;
                break;
            }

            if (removeNode == null) return;

            nodes.Remove(removeNode);

            int inputAmount = removeNode.input.Count;
            for (int i = 0; i < inputAmount; i++)
            {
                CopyPasteNode node = removeNode.input[i];
                node.output.Remove(removeNode);
            }

            int outputAmount = removeNode.output.Count;
            for (int i = 0; i < outputAmount; i++)
            {
                CopyPasteNode node = removeNode.output[i];
                node.input.Remove(removeNode);
            }
        }
    }
}