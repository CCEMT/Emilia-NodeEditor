#if UNITY_EDITOR
using System;
using System.Text.RegularExpressions;
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

        public static string GetWholePath(Transform currentGameObject, GameObject target)
        {
            if (currentGameObject.parent == null || currentGameObject.parent.gameObject == target) return currentGameObject.name;
            return GetWholePath(currentGameObject.parent, target) + "/" + currentGameObject.name;
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