using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Emilia.Node.Editor
{
    public static class IGraphAssetExpansion
    {
        public static void Save(this IGraphAsset graphAsset)
        {
            List<Object> allAsset = new List<Object>();
            graphAsset.CollectAsset(allAsset);

            int count = allAsset.Count;
            for (int i = 0; i < count; i++)
            {
                Object asset = allAsset[i];
                EditorUtility.SetDirty(asset);
            }

            AssetDatabase.SaveAssets();
        }
    }
}