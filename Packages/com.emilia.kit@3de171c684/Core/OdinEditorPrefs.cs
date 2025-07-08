#if UNITY_EDITOR
using UnityEditor;

namespace Emilia.Kit
{
    public static class OdinEditorPrefs
    {
        public static T GetValue<T>(string key)
        {
            string json = EditorPrefs.GetString(key);
            if (string.IsNullOrEmpty(json)) return default;
            return JsonSerializableUtility.FromJson<T>(json);
        }

        public static void SetValue<T>(string key, T value)
        {
            string json = JsonSerializableUtility.ToJson(value);
            EditorPrefs.SetString(key, json);
        }

        public static bool HasValue(string key)
        {
            return EditorPrefs.HasKey(key);
        }
    }
}
#endif