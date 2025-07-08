#if UNITY_EDITOR
using System;
using System.Text.RegularExpressions;
using Emilia.Reflection.Editor;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Emilia.Kit
{
    public static class EditorKit
    {
        public static void UnityInvoke(Action action)
        {
            bool waitFrameEnd = false;

            EditorApplication.update += Invoke;

            void Invoke()
            {
                if (waitFrameEnd == false)
                {
                    waitFrameEnd = true;
                    return;
                }

                action?.Invoke();
                EditorApplication.update -= Invoke;
            }
        }

        public static void SetSelection(object target, string disposeName = null)
        {
            if (string.IsNullOrEmpty(disposeName)) disposeName = target.ToString();
            SelectionContainer selectionContainer = ScriptableObject.CreateInstance<SelectionContainer>();
            selectionContainer.target = target;
            selectionContainer.displayName = disposeName;
            Selection.activeObject = selectionContainer;
        }

        public static string GetNumberAlpha(string source)
        {
            if (string.IsNullOrEmpty(source)) return null;
            string pattern = "[A-Za-z0-9_]";
            string strRet = "";
            MatchCollection results = Regex.Matches(source, pattern);
            foreach (Match v in results) strRet += v.ToString();
            return strRet;
        }

        public static string GetWholePath(Transform current, Transform target)
        {
            if (current.parent == null || current.parent == target) return current.name;
            return GetWholePath(current.parent, target) + "/" + current.name;
        }

        public static void ClearScene(Scene scene)
        {
            GameObject[] previewGameObjects = scene.GetRootGameObjects();
            int previewAmount = previewGameObjects.Length;
            for (int i = 0; i < previewAmount; i++)
            {
                GameObject previewGameObject = previewGameObjects[i];
                Object.DestroyImmediate(previewGameObject);
            }
        }

        public static void SetScenePicking(GameObject[] gameObjects, bool isEnable, bool includeDescendants)
        {
            if (isEnable) SceneVisibilityManager.instance.EnablePicking(gameObjects, includeDescendants);
            else SceneVisibilityManager.instance.DisablePicking(gameObjects, includeDescendants);
        }

        public static void SetSceneExpanded(GameObject[] gameObjects, bool expand, bool includeDescendants)
        {
            int count = gameObjects.Length;

            for (int i = 0; i < count; i++)
            {
                GameObject gameObject = gameObjects[i];
                if (gameObject == null) continue;

                int id = gameObject.GetInstanceID();

                if (includeDescendants) SceneHierarchyWindow_Internals.SetExpandedRecursive_Internals(id, expand);
                else SceneHierarchyWindow_Internals.SetExpanded_Internals(id, expand);
            }
        }

        [HideMonoScript]
        private class SelectionContainer : TitleAsset
        {
            [HideInInspector, SerializeField]
            public string displayName;

            [NonSerialized, OdinSerialize, HideReferenceObjectPicker, HideLabel]
            public object target;

            public override string title => displayName;
        }
    }
}
#endif