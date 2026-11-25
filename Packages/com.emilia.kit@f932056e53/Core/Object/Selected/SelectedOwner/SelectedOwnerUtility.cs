#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Kit
{
    public static class SelectedOwnerUtility
    {
        private static Dictionary<Object, object> selectedObjectOwnerMap = new Dictionary<Object, object>();
        private static readonly List<SelectedOwnerScope> selectedOwnerScopes = new List<SelectedOwnerScope>();

        private sealed class SelectedOwnerScope : IDisposable
        {
            public readonly object owner;
            private bool disposed;

            public SelectedOwnerScope(object owner)
            {
                this.owner = owner;
                selectedOwnerScopes.Add(this);
            }

            public void Dispose()
            {
                if (disposed) return;
                disposed = true;

                for (int i = selectedOwnerScopes.Count - 1; i >= 0; i--)
                {
                    if (ReferenceEquals(selectedOwnerScopes[i], this) == false) continue;
                    selectedOwnerScopes.RemoveAt(i);
                    break;
                }
            }
        }

        private sealed class EmptyDisposable : IDisposable
        {
            public static readonly EmptyDisposable instance = new EmptyDisposable();
            public void Dispose() { }
        }

        public static void SetSelectedOwner(Object selectedObject, object owner)
        {
            if (selectedObject == null) return;
            if (owner == null) return;

            selectedObjectOwnerMap[selectedObject] = owner;
        }

        public static object GetSelectedOwner(Object selectedObject)
        {
            if (selectedObject == null) return null;
            return selectedObjectOwnerMap.GetValueOrDefault(selectedObject);
        }

        public static IDisposable PushSelectedOwner(object owner)
        {
            if (owner == null) return EmptyDisposable.instance;
            return new SelectedOwnerScope(owner);
        }

        public static object GetSelectedOwner(InspectorProperty inspectorProperty)
        {
            while (inspectorProperty != null)
            {
                if (inspectorProperty.ValueEntry?.WeakSmartValue is Object selectedObject)
                {
                    if (selectedObject != null)
                    {
                        object owner = GetSelectedOwner(selectedObject);
                        if (owner != null) return owner;
                    }
                }

                inspectorProperty = inspectorProperty.Parent;
            }

            int scopeCount = selectedOwnerScopes.Count;
            return scopeCount > 0 ? selectedOwnerScopes[scopeCount - 1].owner : null;
        }

        public static void Update()
        {
            List<Object> removeList = new List<Object>();

            foreach (var pair in selectedObjectOwnerMap)
            {
                if (pair.Key == null)
                {
                    removeList.Add(pair.Key);
                    continue;
                }

                ISelectedOwner selectedOwner = pair.Value as ISelectedOwner;
                if (selectedOwner != null && selectedOwner.Validate() == false)
                {
                    removeList.Add(pair.Key);
                    continue;
                }
            }

            foreach (Object selectedObject in removeList) selectedObjectOwnerMap.Remove(selectedObject);
        }
    }
}
#endif