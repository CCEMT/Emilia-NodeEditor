using System;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Emilia.Node.Editor
{
    public static class EditorGraphOpenAsset
    {
        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceID, int line)
        {
            EditorGraphAsset asset = EditorUtility.InstanceIDToObject(instanceID) as EditorGraphAsset;
            if (asset == null) return false;

            Type windowType = GetWindowType(asset.GetType());
            EditorGraphWindowUtility.OpenGraphWindow(windowType, asset);

            return true;
        }

        private static Type GetWindowType(Type assetType)
        {
            WindowSettingsAttribute settings = assetType.GetAttribute<WindowSettingsAttribute>();
            if (settings == null) return typeof(EditorGraphWindow);
            return settings.windowType ?? typeof(EditorGraphWindow);
        }
    }
}