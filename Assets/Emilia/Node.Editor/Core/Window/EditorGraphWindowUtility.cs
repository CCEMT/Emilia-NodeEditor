using System;
using System.Collections.Generic;
using Emilia.Node.Attributes;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Emilia.Node.Editor
{
    public static class EditorGraphWindowUtility
    {
        public static Dictionary<EditorGraphAsset, IEditorGraphWindow> graphWindows = new Dictionary<EditorGraphAsset, IEditorGraphWindow>();

        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            EditorWindow[] windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            int amount = windows.Length;
            for (int i = 0; i < amount; i++)
            {
                EditorWindow window = windows[i];
                IEditorGraphWindow graphWindow = window as IEditorGraphWindow;
                if (graphWindow == null) continue;
                if (graphWindow.graphAsset == null) continue;
                graphWindows.Add(graphWindow.graphAsset, graphWindow);
            }
        }

        public static IEditorGraphWindow OpenGraphWindow(Type type, EditorGraphAsset graphAsset)
        {
            if (graphWindows.TryGetValue(graphAsset, out var graphWindow))
            {
                EditorWindow window = graphWindow as EditorWindow;
                if (window != null)
                {
                    window.Focus();
                    return graphWindow;
                }
            }

            EditorWindow createWindow = CreateWindow(type);
            createWindow.position = GetPosition(graphAsset);

            string title = GetTitle(graphAsset);
            createWindow.titleContent = new GUIContent(title);

            IEditorGraphWindow editorGraphWindow = createWindow as IEditorGraphWindow;
            editorGraphWindow.SetGraphAsset(graphAsset);

            graphWindows[graphAsset] = editorGraphWindow;
            return editorGraphWindow;
        }

        private static Rect GetPosition(EditorGraphAsset graphAsset)
        {
            WindowSettingsAttribute settings = graphAsset.GetType().GetAttribute<WindowSettingsAttribute>();
            if (settings == null) return GUIHelper.GetEditorWindowRect().AlignCenter(850, 600);

            if (string.IsNullOrEmpty(settings.getStartSizeExpression)) return GUIHelper.GetEditorWindowRect().AlignCenter(settings.startSize.x, settings.startSize.y);

            ValueResolver<Vector2> valueResolver = ValueResolver.Get<Vector2>(graphAsset.propertyTree.RootProperty, settings.getStartSizeExpression);
            if (valueResolver.HasError)
            {
                Debug.LogError($"GetPosition Error: {valueResolver.ErrorMessage}");
                return GUIHelper.GetEditorWindowRect().AlignCenter(850, 600);
            }

            Vector2 size = valueResolver.GetValue();

            return GUIHelper.GetEditorWindowRect().AlignCenter(size.x, size.y);
        }

        private static string GetTitle(EditorGraphAsset graphAsset)
        {
            WindowSettingsAttribute settings = graphAsset.GetType().GetAttribute<WindowSettingsAttribute>();
            if (settings == null) return graphAsset.name;

            if (string.IsNullOrEmpty(settings.titleExpression)) return settings.title;

            ValueResolver<string> valueResolver = ValueResolver.Get<string>(graphAsset.propertyTree.RootProperty, settings.titleExpression);
            if (valueResolver.HasError)
            {
                Debug.LogError($"GetTitle Error: {valueResolver.ErrorMessage}");
                return graphAsset.name;
            }

            string title = valueResolver.GetValue();
            return title;
        }

        private static EditorWindow CreateWindow(Type type)
        {
            ScriptableObject instance = ScriptableObject.CreateInstance(type);
            EditorWindow window = instance as EditorWindow;
            window.Show();
            return window;
        }
    }
}