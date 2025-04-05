using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Emilia.Node.Editor
{
    public static class GraphSelectedOwner
    {
        private static Dictionary<Object, EditorGraphView> selectedObjectOwnerMap = new Dictionary<Object, EditorGraphView>();

        public static void SetSelectedOwner(Object selectedObject, EditorGraphView owner)
        {
            if (selectedObject == null) return;
            if (owner == null) return;

            selectedObjectOwnerMap[selectedObject] = owner;
        }

        public static EditorGraphView GetSelectedOwner(Object selectedObject)
        {
            if (selectedObject == null) return null;
            return selectedObjectOwnerMap.GetValueOrDefault(selectedObject);
        }

        public static EditorGraphView GetSelectedOwner(InspectorProperty inspectorProperty)
        {
            while (inspectorProperty != null)
            {
                if (inspectorProperty.ValueEntry?.WeakSmartValue is Object selectedObject)
                {
                    if (selectedObject != null)
                    {
                        EditorGraphView owner = GetSelectedOwner(selectedObject);
                        if (owner != null) return owner;
                    }
                }

                inspectorProperty = inspectorProperty.Parent;
            }

            return null;
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

                if (pair.Value.Validate() == false)
                {
                    removeList.Add(pair.Key);
                    continue;
                }
            }

            foreach (Object selectedObject in removeList) selectedObjectOwnerMap.Remove(selectedObject);
        }
    }
}