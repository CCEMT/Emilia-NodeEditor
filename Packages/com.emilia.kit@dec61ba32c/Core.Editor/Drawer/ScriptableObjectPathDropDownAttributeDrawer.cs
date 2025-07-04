using System;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Kit.Editor
{
    public class ScriptableObjectPathDropDownAttributeDrawer : OdinAttributeDrawer<ScriptableObjectPathDropDownAttribute>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            Type valueType = Property.ValueEntry.TypeOfValue;
            if (valueType == null) return;
            if (valueType != typeof(string)) return;

            Type scriptableObjectType = this.Attribute.type;
            if (scriptableObjectType == null) return;
            if (scriptableObjectType.IsSubclassOf(typeof(ScriptableObject)) == false) return;

            GUILayout.BeginHorizontal();

            if (label != null) GUILayout.Label(label);

            string valueString = Property.ValueEntry.WeakSmartValue as string;
            string name = GetFileName(valueString);
            if (GUILayout.Button(name, "MiniPopup"))
            {
                OdinMenu odinMenu = new OdinMenu();

                Object[] objects = EditorAssetKit.GetEditorResources(scriptableObjectType);
                foreach (Object obj in objects)
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    string itemName = obj.ToString();
                    odinMenu.AddItem(itemName, () => { Property.ValueEntry.WeakSmartValue = path; });
                }

                odinMenu.ShowInPopup();
            }

            GUILayout.EndHorizontal();

        }

        private string GetFileName(string path)
        {
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (obj == null) return "";
            return obj.ToString();
        }
    }
}