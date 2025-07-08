#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Emilia.Kit.Editor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using Action = System.Action;
using Object = UnityEngine.Object;

namespace Emilia.Kit
{
    public class OdinMenu
    {
        private struct MenuItem
        {
            public object userData;
            public Action<object> action;
        }

        private Dictionary<string, MenuItem> items = new Dictionary<string, MenuItem>();

        public string title { get; set; }

        public float defaultWidth { get; set; } = 200f;

        public bool isEnablePinYinSearch { get; set; } = true;

        public OdinMenu(string name = "选择")
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
                    return SearchUtility.Search(target, input);
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
            ShowInPopupObject("选择", onSelected);
        }

        public static void ShowInPopupObject(Type type, Action<object> onSelected)
        {
            ShowInPopupObject(type, "选择", onSelected);
        }

        public static void ShowInPopupObject<T>(string title, Action<T> onSelected)
        {
            ShowInPopupObject(title, 200f, onSelected);
        }

        public static void ShowInPopupObject(Type type, string title, Action<object> onSelected)
        {
            ShowInPopupObject(type, title, 200f, onSelected);
        }

        public static void ShowInPopupObject<T>(string title, float width, Action<T> onSelected)
        {
            ShowInPopupObject(typeof(T), title, width, (obj) => { onSelected?.Invoke((T) obj); });
        }

        public static void ShowInPopupObject(Type objectType, string title, float width, Action<object> onSelected)
        {
            ShowInPopupType(objectType, title, width, (type) => { onSelected?.Invoke(ReflectUtility.CreateInstance(type)); });
        }

        public static void ShowInPopupType<T>(Action<Type> onSelected)
        {
            ShowInPopupType<T>("选择", onSelected);
        }

        public static void ShowInPopupType(Type type, Action<Type> onSelected)
        {
            ShowInPopupType(type, "选择", onSelected);
        }

        public static void ShowInPopupType<T>(string title, Action<Type> onSelected)
        {
            ShowInPopupType<T>(title, 200f, onSelected);
        }

        public static void ShowInPopupType(Type type, string title, Action<Type> onSelected)
        {
            ShowInPopupType(type, title, 200f, onSelected);
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

                string displayName = type.Name;
                TextAttribute textAttribute = type.GetCustomAttribute<TextAttribute>(true);
                if (textAttribute != null) displayName = textAttribute.text;

                menu.AddItem(displayName, () => onSelected(type));
            }

            menu.ShowInPopup(width);
        }

        public static void ShowInPopupScriptableObject<T>(Action<T> onSelected) where T : ScriptableObject
        {
            ShowInPopupScriptableObject("选择", onSelected);
        }

        public static void ShowInPopupScriptableObject(Type type, Action<ScriptableObject> onSelected)
        {
            ShowInPopupScriptableObject(type, "选择", onSelected);
        }

        public static void ShowInPopupScriptableObject<T>(string title, Action<T> onSelected) where T : ScriptableObject
        {
            ShowInPopupScriptableObject(title, 200f, onSelected);
        }

        public static void ShowInPopupScriptableObject(Type type, string title, Action<ScriptableObject> onSelected)
        {
            ShowInPopupScriptableObject(type, title, 200f, onSelected);
        }

        public static void ShowInPopupScriptableObject<T>(string title, float width, Action<T> onSelected) where T : ScriptableObject
        {
            ShowInPopupScriptableObject(typeof(T), title, width, (obj) => { onSelected?.Invoke((T) obj); });
        }

        public static void ShowInPopupScriptableObject(Type objectType, string title, float width, Action<ScriptableObject> onSelected)
        {
            OdinMenu menu = new(title);

            Object[] resources = EditorAssetKit.GetEditorResources(objectType);
            for (int i = 0; i < resources.Length; i++)
            {
                Object asset = resources[i];
                if (asset == null) continue;

                ScriptableObject scriptableObject = asset as ScriptableObject;
                if (scriptableObject == null) continue;

                string displayName = scriptableObject.name;
                string description = ObjectDescriptionUtility.GetDescription(scriptableObject);
                if (string.IsNullOrEmpty(description) == false) displayName += $"({description})";

                menu.AddItem(displayName, () => onSelected(scriptableObject));
            }

            menu.ShowInPopup(width);
        }

        public static void ShowInPopupScriptableObjectPath<T>(string path, Action<T> onSelected) where T : ScriptableObject
        {
            ShowInPopupScriptableObjectPath(path, "选择", onSelected);
        }

        public static void ShowInPopupScriptableObjectPath(string path, Action<ScriptableObject> onSelected)
        {
            ShowInPopupScriptableObjectPath(path, "选择", onSelected);
        }

        public static void ShowInPopupScriptableObjectPath<T>(string path, string title, Action<T> onSelected) where T : ScriptableObject
        {
            ShowInPopupScriptableObjectPath(path, title, 200f, onSelected);
        }

        public static void ShowInPopupScriptableObjectPath(string path, string title, Action<ScriptableObject> onSelected)
        {
            ShowInPopupScriptableObjectPath(path, title, 200f, onSelected);
        }

        public static void ShowInPopupScriptableObjectPath<T>(string path, string title, float width, Action<T> onSelected) where T : ScriptableObject
        {
            ShowInPopupScriptableObjectPath(path, title, width, (obj) => { onSelected?.Invoke((T) obj); });
        }

        public static void ShowInPopupScriptableObjectPath(string path, string title, float width, Action<ScriptableObject> onSelected)
        {
            OdinMenu menu = new(title);

            List<ScriptableObject> resources = EditorAssetKit.LoadAssetAtPath<ScriptableObject>(path);
            for (int i = 0; i < resources.Count; i++)
            {
                ScriptableObject asset = resources[i];
                if (asset == null) continue;

                string displayName = asset.name;
                string description = ObjectDescriptionUtility.GetDescription(asset);
                if (string.IsNullOrEmpty(description) == false) displayName += $"({description})";

                menu.AddItem(displayName, () => onSelected(asset));
            }

            menu.ShowInPopup(width);
        }

        public static void ShowInPopupPrefab(string path, Action<GameObject> onSelected)
        {
            ShowInPopupPrefab(path, "选择", onSelected);
        }

        public static void ShowInPopupPrefab(string path, string title, Action<GameObject> onSelected)
        {
            ShowInPopupPrefab(path, title, 200f, onSelected);
        }

        public static void ShowInPopupPrefab(string path, string title, float width, Action<GameObject> onSelected)
        {
            OdinMenu menu = new(title);

            List<GameObject> resources = EditorAssetKit.LoadAtPath<GameObject>(path, "*.prefab");
            for (int i = 0; i < resources.Count; i++)
            {
                GameObject prefab = resources[i];
                if (prefab == null) continue;

                string displayName = prefab.name;
                IObjectDescription descriptionComponent = prefab.GetComponent<IObjectDescription>();
                if (descriptionComponent != null) displayName += $"({descriptionComponent.description})";

                menu.AddItem(displayName, () => onSelected(prefab));
            }

            menu.ShowInPopup(width);
        }

        public static void ShowInPopupAsset<T>(string path, string searchPattern, Action<T> onSelected) where T : Object
        {
            ShowInPopupAsset(path, searchPattern, "选择", onSelected);
        }

        public static void ShowInPopupAsset(string path, string searchPattern, Action<Object> onSelected)
        {
            ShowInPopupAsset(path, searchPattern, "选择", onSelected);
        }

        public static void ShowInPopupAsset<T>(string path, string searchPattern, string title, Action<T> onSelected) where T : Object
        {
            ShowInPopupAsset(path, searchPattern, title, 200f, onSelected);
        }

        public static void ShowInPopupAsset(string path, string searchPattern, string title, Action<Object> onSelected)
        {
            ShowInPopupAsset(path, searchPattern, title, 200f, onSelected);
        }

        public static void ShowInPopupAsset<T>(string path, string searchPattern, string title, float width, Action<T> onSelected) where T : Object
        {
            ShowInPopupAsset(path, searchPattern, title, width, (obj) => { onSelected?.Invoke((T) obj); });
        }

        public static void ShowInPopupAsset(string path, string searchPattern, string title, float width, Action<Object> onSelected)
        {
            OdinMenu menu = new(title);

            List<Object> resources = EditorAssetKit.LoadAtPath<Object>(path, searchPattern);
            for (int i = 0; i < resources.Count; i++)
            {
                Object asset = resources[i];
                if (asset == null) continue;

                string displayName = asset.name;
                string description = ObjectDescriptionUtility.GetDescription(asset);
                if (string.IsNullOrEmpty(description) == false) displayName += $"({description})";

                menu.AddItem(displayName, () => onSelected(asset));
            }

            menu.ShowInPopup(width);
        }
    }
}
#endif