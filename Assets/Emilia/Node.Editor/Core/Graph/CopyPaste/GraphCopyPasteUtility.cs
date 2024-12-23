using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Emilia.Node.Editor
{
    public static class GraphCopyPasteUtility
    {
        public static void PasteChild(IGraphAsset asset)
        {
            List<Object> pasteList = new List<Object>();
            List<Object> childAssets = asset.GetChildren();
            int amount = childAssets.Count;
            for (int i = 0; i < amount; i++)
            {
                Object child = childAssets[i];
                if (child == null) continue;
                Object pasteChild = Object.Instantiate(child);
                pasteChild.name = child.name;

                Undo.RegisterCreatedObjectUndo(pasteChild, "Graph Pause");

                IGraphAsset childAsset = pasteChild as IGraphAsset;
                if (childAsset != null) PasteChild(childAsset);

                pasteList.Add(pasteChild);
            }

            asset.SetChildren(pasteList);
        }
    }
}