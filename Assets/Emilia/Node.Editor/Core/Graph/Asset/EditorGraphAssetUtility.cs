using Emilia.Kit;
using UnityEditor;
using UnityEngine;

namespace Emilia.Node.Editor
{
    public static class EditorGraphAssetUtility
    {
        public static T CreateAsAttached<T>(string assetPath) where T : EditorGraphAsset
        {
            Object masterAsset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (masterAsset == null)
            {
                Debug.LogError($"Master asset not found at path: {assetPath}");
                return null;
            }

            T asset = ScriptableObject.CreateInstance<T>();
            EditorAssetKit.SaveAssetIntoObject(asset, masterAsset);
            AssetDatabase.SaveAssets();
            return asset;
        }

        public static T Create<T>(string savePath) where T : EditorGraphAsset
        {
            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, savePath);
            AssetDatabase.SaveAssets();
            return asset;
        }
    }
}