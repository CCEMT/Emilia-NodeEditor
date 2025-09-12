#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Emilia.Kit.Editor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Action = System.Action;
using Object = UnityEngine.Object;

namespace Emilia.Kit
{
    public class OdinMenu
    {
        private const string DefaultName = "选择";
        private const float DefaultWidth = 200f;

        private struct MenuItem
        {
            public object userData;
            public Action<object> action;
        }

        private Dictionary<string, MenuItem> items = new Dictionary<string, MenuItem>();

        public string title { get; set; }

        public float defaultWidth { get; set; } = DefaultWidth;

        public bool isEnablePinYinSearch { get; set; } = true;

        public OdinMenu(string name = DefaultName)
        {
            title = name;
        }

        public bool HasItem(string name) => items.ContainsKey(name);

        public void AddItem(string name, Action action)
        {
            MenuItem menuItem = new MenuItem();
            menuItem.action = (_) => action?.Invoke();
            items[name] = menuItem;
        }

        public void AddItem(string name, object userData, Action<object> action)
        {
            MenuItem menuItem = new MenuItem();
            menuItem.userData = userData;
            menuItem.action = action;
            items[name] = menuItem;
        }

        public OdinEditorWindow ShowInPopup() => ShowInPopup(defaultWidth);

        public OdinEditorWindow ShowInPopup(float width)
        {
            GenericSelector<Action> customGenericSelector = GetSelector();
            return customGenericSelector.ShowInPopup(width);
        }

        public OdinEditorWindow ShowInPopup(Rect rect, float width)
        {
            GenericSelector<Action> customGenericSelector = GetSelector();
            return customGenericSelector.ShowInPopup(rect, width);
        }

        private GenericSelector<Action> GetSelector()
        {
            IEnumerable<GenericSelectorItem<Action>> customCollection = items.Keys.Select(itemName =>
                new GenericSelectorItem<Action>($"{itemName}", () => items[itemName].action(items[itemName].userData)));

            GenericSelector<Action> customGenericSelector = new(title, false, customCollection);

            if (isEnablePinYinSearch)
            {
                customGenericSelector.SelectionTree.Config.SearchFunction = item => {
                    string target = item.SearchString;
                    string input = customGenericSelector.SelectionTree.Config.SearchTerm;
                    return SearchUtility.Matching(target, input);
                };
            }

            customGenericSelector.EnableSingleClickToSelect();

            customGenericSelector.SelectionChanged += ints => {
                Action result = ints.FirstOrDefault();
                if (result != null) result();
            };

            return customGenericSelector;
        }

        public void Clear()
        {
            items.Clear();
        }

        public static void ShowInPopupObject<T>(Action<T> onSelected)
        {
            ShowInPopupObject(DefaultName, onSelected);
        }

        public static void ShowInPopupObject(Type type, Action<object> onSelected)
        {
            ShowInPopupObject(type, DefaultName, onSelected);
        }

        public static void ShowInPopupObject<T>(string title, Action<T> onSelected)
        {
            ShowInPopupObject(title, DefaultWidth, onSelected);
        }

        public static void ShowInPopupObject(Type type, string title, Action<object> onSelected)
        {
            ShowInPopupObject(type, title, DefaultWidth, onSelected);
        }

        public static void ShowInPopupObject<T>(string title, float width, Action<T> onSelected)
        {
            ShowInPopupObject(typeof(T), title, width, (obj) => onSelected?.Invoke((T) obj));
        }

        public static void ShowInPopupObject(Type objectType, string title, float width, Action<object> onSelected)
        {
            ShowInPopupType(objectType, title, width, (type) => onSelected?.Invoke(ReflectUtility.CreateInstance(type)));
        }

        public static void ShowInPopupType<T>(Action<Type> onSelected)
        {
            ShowInPopupType<T>(DefaultName, onSelected);
        }

        public static void ShowInPopupType(Type type, Action<Type> onSelected)
        {
            ShowInPopupType(type, DefaultName, onSelected);
        }

        public static void ShowInPopupType<T>(string title, Action<Type> onSelected)
        {
            ShowInPopupType<T>(title, DefaultWidth, onSelected);
        }

        public static void ShowInPopupType(Type type, string title, Action<Type> onSelected)
        {
            ShowInPopupType(type, title, DefaultWidth, onSelected);
        }

        public static void ShowInPopupType<T>(string title, float width, Action<Type> onSelected)
        {
            ShowInPopupType(typeof(T), title, width, onSelected);
        }

        public static void ShowInPopupType(Type objectType, string title, float width, Action<Type> onSelected)
        {
            OdinMenu menu = new(title);

            IList<Type> types = TypeCache.GetTypesDerivedFrom(objectType);
            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                if (type.IsAbstract || type.IsInterface) continue;

                if (HideUtility.IsHide(type)) continue;

                string displayName = type.Name;
                string description = ObjectDescriptionUtility.GetDescription(type);
                if (string.IsNullOrEmpty(description) == false) displayName = description;

                menu.AddItem(displayName, () => onSelected(type));
            }

            menu.ShowInPopup(width);
        }

        public static void ShowInPopupScriptableObject<T>(Action<T> onSelected) where T : ScriptableObject
        {
            T[] resources = EditorAssetKit.GetEditorResources<T>();
            ShowInPopupList(DefaultName, DefaultWidth, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), asset => asset, onSelected);
        }

        public static void ShowInPopupScriptableObject<T>(Func<T, string> getDescription, Action<T> onSelected) where T : ScriptableObject
        {
            T[] resources = EditorAssetKit.GetEditorResources<T>();
            ShowInPopupList(DefaultName, DefaultWidth, resources, getDescription, asset => asset, onSelected);
        }

        public static void ShowInPopupScriptableObject<T1, T2>(Func<T1, T2> selectValue, Action<T2> onSelected) where T1 : ScriptableObject
        {
            T1[] resources = EditorAssetKit.GetEditorResources<T1>();
            ShowInPopupList(DefaultName, DefaultWidth, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), selectValue, onSelected);
        }

        public static void ShowInPopupScriptableObject<T1, T2>(
            Func<T1, string> getDescription, Func<T1, T2> selectValue, Action<T2> onSelected) where T1 : ScriptableObject
        {
            T1[] resources = EditorAssetKit.GetEditorResources<T1>();
            ShowInPopupList(DefaultName, DefaultWidth, resources, getDescription, selectValue, onSelected);
        }

        public static void ShowInPopupScriptableObject(Type type, Action<ScriptableObject> onSelected)
        {
            var resources = EditorAssetKit.GetEditorResources(type).OfType<ScriptableObject>();
            ShowInPopupList(DefaultName, DefaultWidth, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), asset => asset, onSelected);
        }

        public static void ShowInPopupScriptableObject(Type type, Func<ScriptableObject, string> getDescription, Action<ScriptableObject> onSelected)
        {
            var resources = EditorAssetKit.GetEditorResources(type).OfType<ScriptableObject>();
            ShowInPopupList(DefaultName, DefaultWidth, resources, getDescription, asset => asset, onSelected);
        }

        public static void ShowInPopupScriptableObject<T>(Type type, Func<ScriptableObject, T> selectValue, Action<T> onSelected)
        {
            var resources = EditorAssetKit.GetEditorResources(type).OfType<ScriptableObject>();
            ShowInPopupList(DefaultName, DefaultWidth, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), selectValue, onSelected);
        }

        public static void ShowInPopupScriptableObject<T>(Type type,
            Func<ScriptableObject, string> getDescription, Func<ScriptableObject, T> selectValue, Action<T> onSelected)
        {
            var resources = EditorAssetKit.GetEditorResources(type).OfType<ScriptableObject>();
            ShowInPopupList(DefaultName, DefaultWidth, resources, getDescription, selectValue, onSelected);
        }

        public static void ShowInPopupScriptableObject<T>(string title, Action<T> onSelected) where T : ScriptableObject
        {
            T[] resources = EditorAssetKit.GetEditorResources<T>();
            ShowInPopupList(title, DefaultWidth, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), asset => asset, onSelected);
        }

        public static void ShowInPopupScriptableObject(Type type, string title, Action<ScriptableObject> onSelected)
        {
            var resources = EditorAssetKit.GetEditorResources(type).OfType<ScriptableObject>();
            ShowInPopupList(title, DefaultWidth, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), asset => asset, onSelected);
        }

        public static void ShowInPopupScriptableObject<T>(string title, float width, Action<T> onSelected) where T : ScriptableObject
        {
            T[] resources = EditorAssetKit.GetEditorResources<T>();
            ShowInPopupList(title, width, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), asset => asset, onSelected);
        }

        public static void ShowInPopupScriptableObject(Type objectType, string title, float width, Action<ScriptableObject> onSelected)
        {
            var resources = EditorAssetKit.GetEditorResources(objectType).OfType<ScriptableObject>();
            ShowInPopupList(title, width, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), asset => asset, onSelected);
        }

        public static void ShowInPopupScriptableObject<T>(string title, Func<T, string> getDescription, Action<T> onSelected) where T : ScriptableObject
        {
            T[] resources = EditorAssetKit.GetEditorResources<T>();
            ShowInPopupList(title, DefaultWidth, resources, getDescription, asset => asset, onSelected);
        }

        public static void ShowInPopupScriptableObject<T>(string title, float width,
            Func<T, string> getDescription, Action<T> onSelected) where T : ScriptableObject
        {
            T[] resources = EditorAssetKit.GetEditorResources<T>();
            ShowInPopupList(title, width, resources, getDescription, asset => asset, onSelected);
        }

        public static void ShowInPopupScriptableObject<T1, T2>(string title, float width,
            Func<T1, string> getDescription, Func<T1, T2> selectValue, Action<T2> onSelected) where T1 : ScriptableObject
        {
            T1[] resources = EditorAssetKit.GetEditorResources<T1>();
            ShowInPopupList(title, width, resources, getDescription, selectValue, onSelected);
        }

        public static void ShowInPopupScriptableObject(Type objectType, string title, float width,
            Func<ScriptableObject, string> getDescription, Action<ScriptableObject> onSelected)
        {
            IEnumerable<ScriptableObject> resources = EditorAssetKit.GetEditorResources(objectType).OfType<ScriptableObject>();
            ShowInPopupList(title, width, resources, getDescription, asset => asset, onSelected);
        }

        public static void ShowInPopupScriptableObject<T>(Type objectType, string title, float width,
            Func<ScriptableObject, string> getDescription, Func<ScriptableObject, T> selectValue, Action<T> onSelected)
        {
            IEnumerable<ScriptableObject> resources = EditorAssetKit.GetEditorResources(objectType).OfType<ScriptableObject>();
            ShowInPopupList(title, width, resources, getDescription, selectValue, onSelected);
        }

        public static void ShowInPopupScriptableObjectName<T>(Action<string> onSelected) where T : ScriptableObject
        {
            T[] resources = EditorAssetKit.GetEditorResources<T>();
            ShowInPopupList(DefaultName, DefaultWidth, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), asset => asset.name, onSelected);
        }

        public static void ShowInPopupScriptableObjectName<T>(Func<T, string> getDescription, Action<string> onSelected) where T : ScriptableObject
        {
            T[] resources = EditorAssetKit.GetEditorResources<T>();
            ShowInPopupList(DefaultName, DefaultWidth, resources, getDescription, asset => asset.name, onSelected);
        }

        public static void ShowInPopupScriptableObjectName<T>(string title, float width, Action<string> onSelected) where T : ScriptableObject
        {
            T[] resources = EditorAssetKit.GetEditorResources<T>();
            ShowInPopupList(title, width, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), asset => asset.name, onSelected);
        }

        public static void ShowInPopupScriptableObjectName<T>(string title, float width,
            Func<T, string> getDescription, Action<string> onSelected) where T : ScriptableObject
        {
            T[] resources = EditorAssetKit.GetEditorResources<T>();
            ShowInPopupList(title, width, resources, getDescription, asset => asset.name, onSelected);
        }

        public static void ShowInPopupScriptableObjectName<T>(string path, Action<string> onSelected) where T : ScriptableObject
        {
            List<T> resources = EditorAssetKit.LoadAssetAtPath<T>(path);
            ShowInPopupList(DefaultName, DefaultWidth, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), asset => asset.name, onSelected);
        }

        public static void ShowInPopupScriptableObjectName<T>(string path, 
            Func<T, string> getDescription, Action<string> onSelected) where T : ScriptableObject
        {
            List<T> resources = EditorAssetKit.LoadAssetAtPath<T>(path);
            ShowInPopupList(DefaultName, DefaultWidth, resources, getDescription, asset => asset.name, onSelected);
        }

        public static void ShowInPopupScriptableObjectName<T>(string path, string title, float width,
            Func<T, string> getDescription, Action<string> onSelected) where T : ScriptableObject
        {
            List<T> resources = EditorAssetKit.LoadAssetAtPath<T>(path);
            ShowInPopupList(title, width, resources, getDescription, asset => asset.name, onSelected);
        }

        public static void ShowInPopupScriptableObjectPath<T>(string path, Action<string> onSelected) where T : ScriptableObject
        {
            List<T> resources = EditorAssetKit.LoadAssetAtPath<T>(path);
            ShowInPopupList(DefaultName, DefaultWidth, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), AssetDatabase.GetAssetPath, onSelected);
        }

        public static void ShowInPopupScriptableObjectPath<T>(string path,
            Func<T, string> getDescription, Action<string> onSelected) where T : ScriptableObject
        {
            List<T> resources = EditorAssetKit.LoadAssetAtPath<T>(path);
            ShowInPopupList(DefaultName, DefaultWidth, resources, getDescription, AssetDatabase.GetAssetPath, onSelected);
        }

        public static void ShowInPopupScriptableObjectPath<T>(string path, string title, Action<string> onSelected) where T : ScriptableObject
        {
            List<T> resources = EditorAssetKit.LoadAssetAtPath<T>(path);
            ShowInPopupList(title, DefaultWidth, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), AssetDatabase.GetAssetPath, onSelected);
        }

        public static void ShowInPopupScriptableObjectPath<T>(string path, string title,
            Func<T, string> getDescription, Action<string> onSelected) where T : ScriptableObject
        {
            List<T> resources = EditorAssetKit.LoadAssetAtPath<T>(path);
            ShowInPopupList(title, DefaultWidth, resources, getDescription, AssetDatabase.GetAssetPath, onSelected);
        }

        public static void ShowInPopupScriptableObjectPath<T>(string path, string title, float width,
            Action<string> onSelected) where T : ScriptableObject
        {
            List<T> resources = EditorAssetKit.LoadAssetAtPath<T>(path);
            ShowInPopupList(title, width, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), AssetDatabase.GetAssetPath, onSelected);
        }

        public static void ShowInPopupScriptableObjectPath<T>(string path, string title, float width,
            Func<T, string> getDescription, Action<string> onSelected) where T : ScriptableObject
        {
            List<T> resources = EditorAssetKit.LoadAssetAtPath<T>(path);
            ShowInPopupList(title, width, resources, getDescription, AssetDatabase.GetAssetPath, onSelected);
        }

        public static void ShowInPopupPrefab(string path, Action<GameObject> onSelected)
        {
            List<GameObject> resources = EditorAssetKit.LoadAtPath<GameObject>(path, "*.prefab");
            ShowInPopupList(DefaultName, DefaultWidth, resources, PrefabDefaultDescription, prefab => prefab, onSelected);
        }

        public static void ShowInPopupPrefab(string path, Func<GameObject, string> getDescription, Action<GameObject> onSelected)
        {
            List<GameObject> resources = EditorAssetKit.LoadAtPath<GameObject>(path, "*.prefab");
            ShowInPopupList(DefaultName, DefaultWidth, resources, getDescription, prefab => prefab, onSelected);
        }

        public static void ShowInPopupPrefab<T>(string path, Func<GameObject, T> selectValue, Action<T> onSelected)
        {
            List<GameObject> resources = EditorAssetKit.LoadAtPath<GameObject>(path, "*.prefab");
            ShowInPopupList(DefaultName, DefaultWidth, resources, PrefabDefaultDescription, selectValue, onSelected);
        }

        public static void ShowInPopupPrefab<T>(string path,
            Func<GameObject, string> getDescription, Func<GameObject, T> selectValue, Action<T> onSelected)
        {
            List<GameObject> resources = EditorAssetKit.LoadAtPath<GameObject>(path, "*.prefab");
            ShowInPopupList(DefaultName, DefaultWidth, resources, getDescription, selectValue, onSelected);
        }

        public static void ShowInPopupPrefab(string path, string title, Action<GameObject> onSelected)
        {
            List<GameObject> resources = EditorAssetKit.LoadAtPath<GameObject>(path, "*.prefab");
            ShowInPopupList(title, DefaultWidth, resources, PrefabDefaultDescription, prefab => prefab, onSelected);
        }

        public static void ShowInPopupPrefab(string path, string title, float width, Action<GameObject> onSelected)
        {
            List<GameObject> resources = EditorAssetKit.LoadAtPath<GameObject>(path, "*.prefab");
            ShowInPopupList(title, width, resources, PrefabDefaultDescription, prefab => prefab, onSelected);
        }

        public static void ShowInPopupPrefab<T>(string path, string title, float width,
            Func<GameObject, string> getDescription, Func<GameObject, T> selectValue, Action<T> onSelected)
        {
            List<GameObject> resources = EditorAssetKit.LoadAtPath<GameObject>(path, "*.prefab");
            ShowInPopupList(title, width, resources, getDescription, selectValue, onSelected);
        }

        public static void ShowInPopupPrefabName(string path, Action<string> onSelected)
        {
            List<GameObject> resources = EditorAssetKit.LoadAtPath<GameObject>(path, "*.prefab");
            ShowInPopupList(DefaultName, DefaultWidth, resources, PrefabDefaultDescription, prefab => prefab.name, onSelected);
        }

        public static void ShowInPopupPrefabName(string path, Func<GameObject, string> getDescription, Action<string> onSelected)
        {
            List<GameObject> resources = EditorAssetKit.LoadAtPath<GameObject>(path, "*.prefab");
            ShowInPopupList(DefaultName, DefaultWidth, resources, getDescription, prefab => prefab.name, onSelected);
        }

        public static void ShowInPopupPrefabPath(string path, Action<string> onSelected)
        {
            List<GameObject> resources = EditorAssetKit.LoadAtPath<GameObject>(path, "*.prefab");
            ShowInPopupList(DefaultName, DefaultWidth, resources, PrefabDefaultDescription, AssetDatabase.GetAssetPath, onSelected);
        }

        public static void ShowInPopupPrefabPath(string path, Func<GameObject, string> getDescription, Action<string> onSelected)
        {
            List<GameObject> resources = EditorAssetKit.LoadAtPath<GameObject>(path, "*.prefab");
            ShowInPopupList(DefaultName, DefaultWidth, resources, getDescription, AssetDatabase.GetAssetPath, onSelected);
        }

        public static void ShowInPopupAsset<T>(string path, string searchPattern, Action<T> onSelected) where T : Object
        {
            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            ShowInPopupList(DefaultName, DefaultWidth, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), asset => asset, onSelected);
        }

        public static void ShowInPopupAsset<T>(string path, string searchPattern,
            Func<T, string> getDescription, Action<T> onSelected) where T : Object
        {
            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            ShowInPopupList(DefaultName, DefaultWidth, resources, getDescription, asset => asset, onSelected);
        }

        public static void ShowInPopupAsset<T1, T2>(string path, string searchPattern,
            Func<T1, T2> selectValue, Action<T2> onSelected) where T1 : Object
        {
            List<T1> resources = EditorAssetKit.LoadAtPath<T1>(path, searchPattern);
            ShowInPopupList(DefaultName, DefaultWidth, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), selectValue, onSelected);
        }

        public static void ShowInPopupAsset<T1, T2>(string path, string searchPattern,
            Func<T1, string> getDescription, Func<T1, T2> selectValue, Action<T2> onSelected) where T1 : Object
        {
            List<T1> resources = EditorAssetKit.LoadAtPath<T1>(path, searchPattern);
            ShowInPopupList(DefaultName, DefaultWidth, resources, getDescription, selectValue, onSelected);
        }

        public static void ShowInPopupAsset<T>(string path, string searchPattern, string title, Action<T> onSelected) where T : Object
        {
            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            ShowInPopupList(title, DefaultWidth, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), asset => asset, onSelected);
        }

        public static void ShowInPopupAsset<T>(string path, string searchPattern, string title,
            Func<T, string> getDescription, Action<T> onSelected) where T : Object
        {
            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            ShowInPopupList(title, DefaultWidth, resources, getDescription, asset => asset, onSelected);
        }

        public static void ShowInPopupAsset<T>(string path, string searchPattern, string title, float width, Action<T> onSelected) where T : Object
        {
            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            ShowInPopupList(title, width, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), asset => asset, onSelected);
        }

        public static void ShowInPopupAsset<T>(string path, string searchPattern, string title, float width,
            Func<T, string> getDescription, Action<T> onSelected) where T : Object
        {
            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            ShowInPopupList(title, width, resources, getDescription, asset => asset, onSelected);
        }

        public static void ShowInPopupAsset<T1, T2>(string path, string searchPattern, string title, float width,
            Func<T1, T2> selectValue, Action<T2> onSelected) where T1 : Object
        {
            List<T1> resources = EditorAssetKit.LoadAtPath<T1>(path, searchPattern);
            ShowInPopupList(title, width, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), selectValue, onSelected);
        }

        public static void ShowInPopupAssetName<T>(string path, string searchPattern, Action<string> onSelected) where T : Object
        {
            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            ShowInPopupList(DefaultName, DefaultWidth, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), (asset) => asset.name, onSelected);
        }

        public static void ShowInPopupAssetName<T>(string path, string searchPattern,
            Func<T, string> getDescription, Action<string> onSelected) where T : Object
        {
            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            ShowInPopupList(DefaultName, DefaultWidth, resources, getDescription, (asset) => asset.name, onSelected);
        }

        public static void ShowInPopupAssetName<T>(string path, string searchPattern, string title, Action<string> onSelected) where T : Object
        {
            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            ShowInPopupList(title, DefaultWidth, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), (asset) => asset.name, onSelected);
        }

        public static void ShowInPopupAssetName<T>(string path, string searchPattern, string title,
            Func<T, string> getDescription, Action<string> onSelected) where T : Object
        {
            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            ShowInPopupList(title, DefaultWidth, resources, getDescription, (asset) => asset.name, onSelected);
        }

        public static void ShowInPopupAssetName<T>(string path, string searchPattern, string title, float width,
            Action<string> onSelected) where T : Object
        {
            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            ShowInPopupList(title, width, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), (asset) => asset.name, onSelected);
        }

        public static void ShowInPopupAssetName<T>(string path, string searchPattern, string title, float width,
            Func<T, string> getDescription, Action<string> onSelected) where T : Object
        {
            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            ShowInPopupList(title, width, resources, getDescription, (asset) => asset.name, onSelected);
        }

        public static void ShowInPopupAssetPath<T>(string path, string searchPattern, Action<string> onSelected) where T : Object
        {
            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            ShowInPopupList(DefaultName, DefaultWidth, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), AssetDatabase.GetAssetPath, onSelected);
        }

        public static void ShowInPopupAssetPath<T>(string path, string searchPattern,
            Func<T, string> getDescription, Action<string> onSelected) where T : Object
        {
            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            ShowInPopupList(DefaultName, DefaultWidth, resources, getDescription, AssetDatabase.GetAssetPath, onSelected);
        }

        public static void ShowInPopupAssetPath<T>(string path, string searchPattern, string title, Action<string> onSelected) where T : Object
        {
            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            ShowInPopupList(title, DefaultWidth, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), AssetDatabase.GetAssetPath, onSelected);
        }

        public static void ShowInPopupAssetPath<T>(string path, string searchPattern, string title,
            Func<T, string> getDescription, Action<string> onSelected) where T : Object
        {
            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            ShowInPopupList(title, DefaultWidth, resources, getDescription, AssetDatabase.GetAssetPath, onSelected);
        }

        public static void ShowInPopupAssetPath<T>(string path, string searchPattern, string title, float width,
            Action<string> onSelected) where T : Object
        {
            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            ShowInPopupList(title, width, resources,
                (asset) => ObjectDescriptionUtility.GetDescription(asset), AssetDatabase.GetAssetPath, onSelected);
        }

        public static void ShowInPopupAssetPath<T>(string path, string searchPattern, string title, float width,
            Func<T, string> getDescription, Action<string> onSelected) where T : Object
        {
            List<T> resources = EditorAssetKit.LoadAtPath<T>(path, searchPattern);
            ShowInPopupList(title, width, resources, getDescription, AssetDatabase.GetAssetPath, onSelected);
        }

        private static void ShowInPopupList<TAsset, TOut>(
            string title,
            float width,
            IEnumerable<TAsset> assets,
            Func<TAsset, string> getDescription,
            Func<TAsset, TOut> selectValue,
            Action<TOut> onSelected)
            where TAsset : Object
        {
            OdinMenu menu = new(title);

            foreach (TAsset asset in assets)
            {
                if (asset == null) continue;
                if (HideUtility.IsHide(asset)) continue;

                string displayName = asset.name;
                string description = getDescription?.Invoke(asset) ?? string.Empty;
                if (string.IsNullOrEmpty(description) == false) displayName += $"({description})";

                TOut value = selectValue(asset);

                menu.AddItem(displayName, () => onSelected(value));
            }

            menu.ShowInPopup(width);
        }

        private static string PrefabDefaultDescription(GameObject prefab)
        {
            IObjectDescription descriptionComponent = prefab != null ? prefab.GetComponent<IObjectDescription>() : null;
            return descriptionComponent != null ? descriptionComponent.description : string.Empty;
        }
    }
}
#endif