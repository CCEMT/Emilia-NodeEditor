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
            ValueDropdownList<T> list = new ValueDropdownList<T>();

            List<T> resources = EditorAssetKit.LoadAssetAtPath<T>(path);
            for (int i = 0; i < resources.Count; i++)
            {
                T asset = resources[i];
                if (asset == null) continue;

                string displayName = asset.name;
                string description = ObjectDescriptionUtility.GetDescription(asset);
                if (string.IsNullOrEmpty(description) == false) displayName += $"({description})";

                list.Add(displayName, asset);
            }

            return list;
        }

        public static ValueDropdownList<T2> GetScriptableObject<T1, T2>(string path, Func<T1, T2> onSelect) where T1 : ScriptableObject
        {
            ValueDropdownList<T2> list = new ValueDropdownList<T2>();

            List<T1> resources = EditorAssetKit.LoadAssetAtPath<T1>(path);
            for (int i = 0; i < resources.Count; i++)
            {
                T1 asset = resources[i];
                if (asset == null) continue;

                string displayName = asset.name;
                string description = ObjectDescriptionUtility.GetDescription(asset);
                if (string.IsNullOrEmpty(description) == false) displayName += $"({description})";

                T2 value = onSelect(asset);
                list.Add(displayName, value);
            }

            return list;
        }

        public static ValueDropdownList<T> GetScriptableObject<T>() where T : ScriptableObject
        {
            ValueDropdownList<T> list = new ValueDropdownList<T>();

            T[] resources = EditorAssetKit.GetEditorResources<T>();
            for (int i = 0; i < resources.Length; i++)
            {
                T asset = resources[i];
                if (asset == null) continue;

                string displayName = asset.name;
                string description = ObjectDescriptionUtility.GetDescription(asset);
                if (string.IsNullOrEmpty(description) == false) displayName += $"({description})";

                list.Add(displayName, asset);
            }

            return list;
        }

        public static ValueDropdownList<T2> GetScriptableObject<T1, T2>(Func<T1, T2> onSelect) where T1 : ScriptableObject
        {
            ValueDropdownList<T2> list = new ValueDropdownList<T2>();

            T1[] resources = EditorAssetKit.GetEditorResources<T1>();
            for (int i = 0; i < resources.Length; i++)
            {
                T1 asset = resources[i];
                if (asset == null) continue;

                string displayName = asset.name;
                string description = ObjectDescriptionUtility.GetDescription(asset);
                if (string.IsNullOrEmpty(description) == false) displayName += $"({description})";

                T2 value = onSelect(asset);
                list.Add(displayName, value);
            }

            return list;
        }

        public static ValueDropdownList<string> GetScriptableObjectName<T>() where T : ScriptableObject
        {
            ValueDropdownList<string> list = new ValueDropdownList<string>();

            T[] resources = EditorAssetKit.GetEditorResources<T>();
            for (int i = 0; i < resources.Length; i++)
            {
                T asset = resources[i];
                if (asset == null) continue;

                string displayName = asset.name;
                string description = ObjectDescriptionUtility.GetDescription(asset);
                if (string.IsNullOrEmpty(description) == false) displayName += $"({description})";

                list.Add(displayName, asset.name);
            }

            return list;
        }

        public static ValueDropdownList<string> GetScriptableObjectName<T>(string path) where T : ScriptableObject
        {
            ValueDropdownList<string> list = new ValueDropdownList<string>();

            List<T> resources = EditorAssetKit.LoadAssetAtPath<T>(path);
            for (int i = 0; i < resources.Count; i++)
            {
                T asset = resources[i];
                if (asset == null) continue;

                string displayName = asset.name;
                string description = ObjectDescriptionUtility.GetDescription(asset);
                if (string.IsNullOrEmpty(description) == false) displayName += $"({description})";

                list.Add(displayName, asset.name);
            }

            return list;
        }

        public static ValueDropdownList<string> GetScriptableObjectPath<T>() where T : ScriptableObject
        {
            ValueDropdownList<string> list = new ValueDropdownList<string>();

            T[] resources = EditorAssetKit.GetEditorResources<T>();
            for (int i = 0; i < resources.Length; i++)
            {
                T asset = resources[i];
                if (asset == null) continue;

                string displayName = asset.name;
                string description = ObjectDescriptionUtility.GetDescription(asset);
                if (string.IsNullOrEmpty(description) == false) displayName += $"({description})";

                string path = AssetDatabase.GetAssetPath(asset);
                list.Add(displayName, path);
            }

            return list;
        }

        public static ValueDropdownList<string> GetScriptableObjectPath<T>(string path) where T : ScriptableObject
        {
            ValueDropdownList<string> list = new ValueDropdownList<string>();

            List<T> resources = EditorAssetKit.LoadAssetAtPath<T>(path);
            for (int i = 0; i < resources.Count; i++)
            {
                T asset = resources[i];
                if (asset == null) continue;

                string displayName = asset.name;
                string description = ObjectDescriptionUtility.GetDescription(asset);
                if (string.IsNullOrEmpty(description) == false) displayName += $"({description})";

                string assetPath = AssetDatabase.GetAssetPath(asset);
                list.Add(displayName, assetPath);
            }

            return list;
        }

        public static ValueDropdownList<GameObject> GetPrefab(string path)
        {
            ValueDropdownList<GameObject> list = new ValueDropdownList<GameObject>();

            List<GameObject> resources = EditorAssetKit.LoadAtPath<GameObject>(path, "*.prefab");
            for (int i = 0; i < resources.Count; i++)
            {
                GameObject prefab = resources[i];
                if (prefab == null) continue;

                string displayName = prefab.name;
                IObjectDescription descriptionComponent = prefab.GetComponent<IObjectDescription>();
                if (descriptionComponent != null) displayName += $"({descriptionComponent.description})";

                list.Add(displayName, prefab);
            }

            return list;
        }

        public static ValueDropdownList<T> GetPrefab<T>(string path, Func<GameObject, T> onSelect)
        {
            ValueDropdownList<T> list = new ValueDropdownList<T>();

            List<GameObject> resources = EditorAssetKit.LoadAtPath<GameObject>(path, "*.prefab");
            for (int i = 0; i < resources.Count; i++)
            {
                GameObject prefab = resources[i];
                if (prefab == null) continue;

                string displayName = prefab.name;
                IObjectDescription descriptionComponent = prefab.GetComponent<IObjectDescription>();
                if (descriptionComponent != null) displayName += $"({descriptionComponent.description})";

                T value = onSelect(prefab);
                list.Add(displayName, value);
            }

            return list;
        }

        public static ValueDropdownList<string> GetPrefabName(string path)
        {
            ValueDropdownList<string> list = new ValueDropdownList<string>();

            List<GameObject> resources = EditorAssetKit.LoadAtPath<GameObject>(path, "*.prefab");
            for (int i = 0; i < resources.Count; i++)
            {
                GameObject prefab = resources[i];
                if (prefab == null) continue;

                string displayName = prefab.name;
                IObjectDescription descriptionComponent = prefab.GetComponent<IObjectDescription>();
                if (descriptionComponent != null) displayName += $"({descriptionComponent.description})";

                list.Add(displayName, prefab.name);
            }

            return list;
        }

        public static ValueDropdownList<string> GetPrefabPath(string path)
        {
            ValueDropdownList<string> list = new ValueDropdownList<string>();

            List<GameObject> resources = EditorAssetKit.LoadAtPath<GameObject>(path, "*.prefab");
            for (int i = 0; i < resources.Count; i++)
            {
                GameObject prefab = resources[i];
                if (prefab == null) continue;

                string displayName = prefab.name;
                IObjectDescription descriptionComponent = prefab.GetComponent<IObjectDescription>();
                if (descriptionComponent != null) displayName += $"({descriptionComponent.description})";

                string assetPath = AssetDatabase.GetAssetPath(prefab);
                list.Add(displayName, assetPath);
            }

            return list;
        }

        public static ValueDropdownList<T> GetAsset<T>(string path, string searchPattern) where T : Object
        {
            ValueDropdownList<T> list = new ValueDropdownList<T>();

            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            for (int i = 0; i < resources.Count; i++)
            {
                T asset = resources[i];
                if (asset == null) continue;

                string displayName = asset.name;
                string description = ObjectDescriptionUtility.GetDescription(asset);
                if (string.IsNullOrEmpty(description) == false) displayName += $"({description})";

                list.Add(displayName, asset);
            }

            return list;
        }

        public static ValueDropdownList<T2> GetAsset<T1, T2>(string path, string searchPattern, Func<T1, T2> onSelect) where T1 : Object
        {
            ValueDropdownList<T2> list = new ValueDropdownList<T2>();

            List<T1> resources = EditorAssetKit.LoadAtPath<T1>(path, searchPattern);
            for (int i = 0; i < resources.Count; i++)
            {
                T1 asset = resources[i];
                if (asset == null) continue;

                string displayName = asset.name;
                string description = ObjectDescriptionUtility.GetDescription(asset);
                if (string.IsNullOrEmpty(description) == false) displayName += $"({description})";

                T2 value = onSelect(asset);
                list.Add(displayName, value);
            }

            return list;
        }

        public static ValueDropdownList<string> GetAssetName<T>(string path, string searchPattern) where T : Object
        {
            ValueDropdownList<string> list = new ValueDropdownList<string>();

            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            for (int i = 0; i < resources.Count; i++)
            {
                T asset = resources[i];
                if (asset == null) continue;

                string displayName = asset.name;
                string description = ObjectDescriptionUtility.GetDescription(asset);
                if (string.IsNullOrEmpty(description) == false) displayName += $"({description})";

                list.Add(displayName, asset.name);
            }

            return list;
        }

        public static ValueDropdownList<string> GetAssetPath<T>(string path, string searchPattern) where T : Object
        {
            ValueDropdownList<string> list = new ValueDropdownList<string>();

            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            for (int i = 0; i < resources.Count; i++)
            {
                T asset = resources[i];
                if (asset == null) continue;

                string assetPath = AssetDatabase.GetAssetPath(asset);
                list.Add(asset.name, assetPath);
            }

            return list;
        }
    }
}
#endif