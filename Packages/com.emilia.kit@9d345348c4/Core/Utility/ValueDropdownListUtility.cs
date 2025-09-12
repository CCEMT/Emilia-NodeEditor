#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Kit
{
    public static class ValueDropdownListUtility
    {
        public static ValueDropdownList<T> GetScriptableObject<T>(string path) where T : ScriptableObject
        {
            List<T> resources = EditorAssetKit.LoadAssetAtPath<T>(path);
            return BuildList(resources, (asset) => ObjectDescriptionUtility.GetDescription(asset), asset => asset);
        }

        public static ValueDropdownList<T> GetScriptableObject<T>(string path, Func<T, string> getDescription) where T : ScriptableObject
        {
            List<T> resources = EditorAssetKit.LoadAssetAtPath<T>(path);
            return BuildList(resources, getDescription, asset => asset);
        }

        public static ValueDropdownList<T2> GetScriptableObject<T1, T2>(string path, Func<T1, T2> onSelect) where T1 : ScriptableObject
        {
            List<T1> resources = EditorAssetKit.LoadAssetAtPath<T1>(path);
            return BuildList(resources, (asset) => ObjectDescriptionUtility.GetDescription(asset), onSelect);
        }

        public static ValueDropdownList<T2> GetScriptableObject<T1, T2>(string path, 
            Func<T1, T2> onSelect, Func<T1, string> getDescription) where T1 : ScriptableObject
        {
            List<T1> resources = EditorAssetKit.LoadAssetAtPath<T1>(path);
            return BuildList(resources, getDescription, onSelect);
        }

        public static ValueDropdownList<T> GetScriptableObject<T>() where T : ScriptableObject
        {
            T[] resources = EditorAssetKit.GetEditorResources<T>();
            return BuildList(resources, (asset) => ObjectDescriptionUtility.GetDescription(asset), asset => asset);
        }

        public static ValueDropdownList<T> GetScriptableObject<T>(Func<T, string> getDescription) where T : ScriptableObject
        {
            T[] resources = EditorAssetKit.GetEditorResources<T>();
            return BuildList(resources, getDescription, asset => asset);
        }

        public static ValueDropdownList<T2> GetScriptableObject<T1, T2>(Func<T1, T2> onSelect) where T1 : ScriptableObject
        {
            T1[] resources = EditorAssetKit.GetEditorResources<T1>();
            return BuildList(resources, (asset) => ObjectDescriptionUtility.GetDescription(asset), onSelect);
        }

        public static ValueDropdownList<T2> GetScriptableObject<T1, T2>(
            Func<T1, T2> onSelect, Func<T1, string> getDescription) where T1 : ScriptableObject
        {
            T1[] resources = EditorAssetKit.GetEditorResources<T1>();
            return BuildList(resources, getDescription, onSelect);
        }

        public static ValueDropdownList<string> GetScriptableObjectName<T>() where T : ScriptableObject
        {
            T[] resources = EditorAssetKit.GetEditorResources<T>();
            return BuildList(resources, (asset) => ObjectDescriptionUtility.GetDescription(asset), asset => asset.name);
        }

        public static ValueDropdownList<string> GetScriptableObjectName<T>(Func<T, string> getDescription) where T : ScriptableObject
        {
            T[] resources = EditorAssetKit.GetEditorResources<T>();
            return BuildList(resources, getDescription, asset => asset.name);
        }

        public static ValueDropdownList<string> GetScriptableObjectName<T>(string path) where T : ScriptableObject
        {
            List<T> resources = EditorAssetKit.LoadAssetAtPath<T>(path);
            return BuildList(resources, (asset) => ObjectDescriptionUtility.GetDescription(asset), asset => asset.name);
        }

        public static ValueDropdownList<string> GetScriptableObjectName<T>(string path, Func<T, string> getDescription) where T : ScriptableObject
        {
            List<T> resources = EditorAssetKit.LoadAssetAtPath<T>(path);
            return BuildList(resources, getDescription, asset => asset.name);
        }

        public static ValueDropdownList<string> GetScriptableObjectPath<T>() where T : ScriptableObject
        {
            T[] resources = EditorAssetKit.GetEditorResources<T>();
            return BuildList(resources, (asset) => ObjectDescriptionUtility.GetDescription(asset), AssetDatabase.GetAssetPath);
        }

        public static ValueDropdownList<string> GetScriptableObjectPath<T>(Func<T, string> getDescription) where T : ScriptableObject
        {
            T[] resources = EditorAssetKit.GetEditorResources<T>();
            return BuildList(resources, getDescription, AssetDatabase.GetAssetPath);
        }

        public static ValueDropdownList<string> GetScriptableObjectPath<T>(string path) where T : ScriptableObject
        {
            List<T> resources = EditorAssetKit.LoadAssetAtPath<T>(path);
            return BuildList(resources, (asset) => ObjectDescriptionUtility.GetDescription(asset), AssetDatabase.GetAssetPath);
        }

        public static ValueDropdownList<string> GetScriptableObjectPath<T>(string path, Func<T, string> getDescription) where T : ScriptableObject
        {
            List<T> resources = EditorAssetKit.LoadAssetAtPath<T>(path);
            return BuildList(resources, getDescription, AssetDatabase.GetAssetPath);
        }

        public static ValueDropdownList<GameObject> GetPrefab(string path)
        {
            List<GameObject> resources = EditorAssetKit.LoadAtPath<GameObject>(path, "*.prefab");
            return BuildList(resources, PrefabDefaultDescription, prefab => prefab);
        }

        public static ValueDropdownList<GameObject> GetPrefab(string path, Func<GameObject, string> getDescription)
        {
            List<GameObject> resources = EditorAssetKit.LoadAtPath<GameObject>(path, "*.prefab");
            return BuildList(resources, getDescription, prefab => prefab);
        }

        public static ValueDropdownList<T> GetPrefab<T>(string path, Func<GameObject, T> onSelect)
        {
            List<GameObject> resources = EditorAssetKit.LoadAtPath<GameObject>(path, "*.prefab");
            return BuildList(resources, PrefabDefaultDescription, onSelect);
        }

        public static ValueDropdownList<T> GetPrefab<T>(string path, Func<GameObject, T> onSelect, Func<GameObject, string> getDescription)
        {
            List<GameObject> resources = EditorAssetKit.LoadAtPath<GameObject>(path, "*.prefab");
            return BuildList(resources, getDescription, onSelect);
        }

        public static ValueDropdownList<string> GetPrefabName(string path)
        {
            List<GameObject> resources = EditorAssetKit.LoadAtPath<GameObject>(path, "*.prefab");
            return BuildList(resources, PrefabDefaultDescription, prefab => prefab.name);
        }

        public static ValueDropdownList<string> GetPrefabName(string path, Func<GameObject, string> getDescription)
        {
            List<GameObject> resources = EditorAssetKit.LoadAtPath<GameObject>(path, "*.prefab");
            return BuildList(resources, getDescription, prefab => prefab.name);
        }

        public static ValueDropdownList<string> GetPrefabPath(string path)
        {
            List<GameObject> resources = EditorAssetKit.LoadAtPath<GameObject>(path, "*.prefab");
            return BuildList(resources, PrefabDefaultDescription, AssetDatabase.GetAssetPath);
        }

        public static ValueDropdownList<string> GetPrefabPath(string path, Func<GameObject, string> getDescription)
        {
            List<GameObject> resources = EditorAssetKit.LoadAtPath<GameObject>(path, "*.prefab");
            return BuildList(resources, getDescription, AssetDatabase.GetAssetPath);
        }

        public static ValueDropdownList<T> GetAsset<T>(string path, string searchPattern) where T : Object
        {
            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            return BuildList(resources, (asset) => ObjectDescriptionUtility.GetDescription(asset), asset => asset);
        }

        public static ValueDropdownList<T> GetAsset<T>(string path, string searchPattern, Func<T, string> getDescription) where T : Object
        {
            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            return BuildList(resources, getDescription, asset => asset);
        }

        public static ValueDropdownList<T2> GetAsset<T1, T2>(string path, string searchPattern, Func<T1, T2> onSelect) where T1 : Object
        {
            List<T1> resources = EditorAssetKit.LoadAtPath<T1>(path, searchPattern);
            return BuildList(resources, (asset) => ObjectDescriptionUtility.GetDescription(asset), onSelect);
        }

        public static ValueDropdownList<T2> GetAsset<T1, T2>(string path, string searchPattern,
            Func<T1, T2> onSelect, Func<T1, string> getDescription) where T1 : Object
        {
            List<T1> resources = EditorAssetKit.LoadAtPath<T1>(path, searchPattern);
            return BuildList(resources, getDescription, onSelect);
        }

        public static ValueDropdownList<string> GetAssetName<T>(string path, string searchPattern) where T : Object
        {
            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            return BuildList(resources, (asset) => ObjectDescriptionUtility.GetDescription(asset), asset => asset.name);
        }

        public static ValueDropdownList<string> GetAssetName<T>(string path, string searchPattern, Func<T, string> getDescription) where T : Object
        {
            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            return BuildList(resources, getDescription, asset => asset.name);
        }

        public static ValueDropdownList<string> GetAssetPath<T>(string path, string searchPattern) where T : Object
        {
            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            return BuildList(resources, (asset) => ObjectDescriptionUtility.GetDescription(asset), AssetDatabase.GetAssetPath);
        }

        public static ValueDropdownList<string> GetAssetPath<T>(string path, string searchPattern, Func<T, string> getDescription) where T : Object
        {
            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            return BuildList(resources, getDescription, AssetDatabase.GetAssetPath);
        }

        private static ValueDropdownList<TOut> BuildList<TAsset, TOut>(
            IEnumerable<TAsset> assets,
            Func<TAsset, string> getDescription,
            Func<TAsset, TOut> selectValue)
            where TAsset : Object
        {
            ValueDropdownList<TOut> list = new ValueDropdownList<TOut>();

            foreach (TAsset asset in assets)
            {
                if (asset == null) continue;
                if (HideUtility.IsHide(asset)) continue;

                string displayName = asset.name;
                string description = getDescription?.Invoke(asset) ?? string.Empty;
                if (string.IsNullOrEmpty(description) == false) displayName += $"({description})";

                list.Add(displayName, selectValue(asset));
            }

            return list;
        }

        private static string PrefabDefaultDescription(GameObject prefab)
        {
            IObjectDescription descriptionComponent = prefab != null ? prefab.GetComponent<IObjectDescription>() : null;
            return descriptionComponent != null ? descriptionComponent.description : string.Empty;
        }
    }
}
#endif