#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Emilia.Kit
{
    public sealed class CopyPasteSaveTransaction
    {
        private readonly HashSet<string> _assetPaths = new(StringComparer.OrdinalIgnoreCase);

        public void RegisterAsset(Object asset)
        {
            if (asset == null) return;
            RegisterAssetPath(AssetDatabase.GetAssetPath(asset));
        }

        public void RegisterAssetPath(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath)) return;
            _assetPaths.Add(assetPath);
        }

        public void Flush()
        {
            foreach (string path in _assetPaths)
            {
                GUID guid = AssetDatabase.GUIDFromAssetPath(path);
                if (string.IsNullOrEmpty(guid.ToString())) continue;
                AssetDatabase.SaveAssetIfDirty(guid);
            }
        }
    }
}
#endif
