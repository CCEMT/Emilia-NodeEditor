#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Emilia.Kit
{
    public static class ObjectDescriptionUtility
    {
        private static Dictionary<Type, IObjectDescriptionGetter> descriptionGetterMap = new();

        static ObjectDescriptionUtility()
        {
            IList<Type> types = TypeCache.GetTypesDerivedFrom<IObjectDescriptionGetter>();

            int amount = types.Count;
            for (int i = 0; i < amount; i++)
            {
                Type type = types[i];
                if (type.IsAbstract || type.IsInterface) continue;

                ObjectDescriptionAttribute attribute = type.GetCustomAttribute<ObjectDescriptionAttribute>();
                if (attribute == null) continue;

                IObjectDescriptionGetter getter = null;

                try
                {
                    getter = (IObjectDescriptionGetter) Activator.CreateInstance(type);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToUnityLogString());
                }

                if (getter == null) continue;

                descriptionGetterMap.Add(attribute.objectType, getter);
            }
        }

        public static string GetDescription(object obj, object owner = null, object userData = null)
        {
            if (obj == null) return string.Empty;

            if (obj is Type type)
            {
                TextAttribute typeTextAttribute = type.GetCustomAttribute<TextAttribute>();
                if (typeTextAttribute != null) return typeTextAttribute.text;
                return string.Empty;
            }

            Type objType = obj.GetType();
            IObjectDescriptionGetter getter = descriptionGetterMap.GetValueOrDefault(objType);
            if (getter != null)
            {
                string description = string.Empty;

                try
                {
                    description = getter.GetDescription(obj, owner, userData);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToUnityLogString());
                }

                if (string.IsNullOrEmpty(description) == false) return description;
            }

            IObjectDescription objectDescription = obj as IObjectDescription;
            if (objectDescription != null)
            {
                string description = string.Empty;

                try
                {
                    description = objectDescription.description;
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToUnityLogString());
                }

                return description;
            }

            TextAttribute textAttribute = objType.GetCustomAttribute<TextAttribute>();
            if (textAttribute != null) return textAttribute.text;

            return string.Empty;
        }
    }
}
#endif