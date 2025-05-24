#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEditor;

namespace Emilia.Kit
{
    public static class ObjectDescriptionUtility
    {
        private static Dictionary<Type, IObjectDescriptionGetter> descriptionGetterMap = new Dictionary<Type, IObjectDescriptionGetter>();

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

                IObjectDescriptionGetter getter = (IObjectDescriptionGetter) Activator.CreateInstance(type);
                if (getter == null) continue;

                descriptionGetterMap.Add(attribute.objectType, getter);
            }
        }

        public static string GetDescription(object obj, object owner = null, object userData = null)
        {
            if (obj == null) return string.Empty;

            Type type = obj.GetType();
            IObjectDescriptionGetter getter = descriptionGetterMap.GetValueOrDefault(type);
            if (getter != null)
            {
                string description = getter.GetDescription(obj, owner, userData);
                if (string.IsNullOrEmpty(description) == false) return description;
            }

            IObjectDescription objectDescription = obj as IObjectDescription;
            if (objectDescription != null) return objectDescription.description;

            return string.Empty;
        }
    }
}
#endif