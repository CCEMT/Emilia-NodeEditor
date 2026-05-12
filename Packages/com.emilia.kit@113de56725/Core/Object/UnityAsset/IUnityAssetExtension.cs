#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Emilia.Kit.Editor
{
    public static class IUnityAssetExtension
    {
        public static List<Object> CollectAsset(this IUnityAsset unityAsset)
        {
            List<Object> allAsset = new List<Object>();

            Queue<IUnityAsset> queue = new Queue<IUnityAsset>();
            queue.Enqueue(unityAsset);

            while (queue.Count > 0)
            {
                IUnityAsset currentAsset = queue.Dequeue();
                Object unityObject = currentAsset as Object;
                if (unityObject != null) allAsset.Add(unityObject);

                List<Object> childAssets = currentAsset.GetChildren();
                if (childAssets == null) continue;
                int amount = childAssets.Count;
                for (int i = 0; i < amount; i++)
                {
                    Object child = childAssets[i];
                    if (child == null) continue;
                    IUnityAsset childAsset = child as IUnityAsset;
                    if (childAsset != null) queue.Enqueue(childAsset);
                    else allAsset.Add(child);
                }
            }

            return allAsset;
        }

        public static void SaveAll(this IUnityAsset unityAsset)
        {
            unityAsset.SetDirtyAll();
            AssetDatabase.SaveAssets();
        }
        
        public static void SetDirtyAll(this IUnityAsset unityAsset)
        {
            List<Object> allAsset = unityAsset.CollectAsset();
            int count = allAsset.Count;
            for (int i = 0; i < count; i++)
            {
                Object asset = allAsset[i];
                EditorUtility.SetDirty(asset);
            }
        }

        public static void OnlySaveAll(this IUnityAsset unityAsset)
        {
            HashSet<string> paths = new();

            List<Object> allAsset = unityAsset.CollectAsset();
            int count = allAsset.Count;
            for (int i = 0; i < count; i++)
            {
                Object asset = allAsset[i];
                EditorUtility.SetDirty(asset);

                string path = AssetDatabase.GetAssetPath(asset);
                paths.Add(path);
            }

            foreach (string path in paths)
            {
                GUID guid = AssetDatabase.GUIDFromAssetPath(path);
                AssetDatabase.SaveAssetIfDirty(guid);
            }
        }

        public static void SaveAssetIfDirtyAll(this IUnityAsset unityAsset)
        {
            HashSet<string> paths = new();

            List<Object> allAsset = unityAsset.CollectAsset();
            int count = allAsset.Count;
            for (int i = 0; i < count; i++)
            {
                Object asset = allAsset[i];

                string path = AssetDatabase.GetAssetPath(asset);
                paths.Add(path);
            }

            foreach (string path in paths)
            {
                GUID guid = AssetDatabase.GUIDFromAssetPath(path);
                AssetDatabase.SaveAssetIfDirty(guid);
            }
        }

        public static void PasteChild(this IUnityAsset asset)
        {
            List<Object> pasteList = new List<Object>();
            List<Object> childAssets = asset.GetChildren();

            if (childAssets != null)
            {
                int amount = childAssets.Count;
                for (int i = 0; i < amount; i++)
                {
                    Object child = childAssets[i];
                    if (child == null) continue;
                    Object pasteChild = Object.Instantiate(child);
                    pasteChild.name = child.name;

                    Undo.RegisterCreatedObjectUndo(pasteChild, "Paste");

                    IUnityAsset childAsset = pasteChild as IUnityAsset;
                    if (childAsset != null) PasteChild(childAsset);

                    pasteList.Add(pasteChild);
                }
            }

            asset.SetChildren(pasteList);
        }

        public static void DestroyImmediateAll(this IUnityAsset asset, bool allowDestroyingAssets = false)
        {
            List<Object> childAssets = asset.GetChildren();
            if (childAssets == null) return;

            for (var i = childAssets.Count - 1; i >= 0; i--)
            {
                Object childAsset = childAssets[i];
                if (childAsset == null) continue;
                Object.DestroyImmediate(childAsset, allowDestroyingAssets);
            }
        }
    }
}
#endif