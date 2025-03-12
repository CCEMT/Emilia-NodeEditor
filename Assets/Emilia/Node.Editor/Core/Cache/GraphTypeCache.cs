using System;
using System.Collections.Generic;
using Emilia.Kit.Editor;
using Emilia.Node.Attributes;
using UnityEditor;

namespace Emilia.Node.Editor
{
    public static class GraphTypeCache
    {
        private static Dictionary<Type, Type> nodeViewTypeCache = new Dictionary<Type, Type>();
        private static Dictionary<Type, Type> edgeViewTypeCache = new Dictionary<Type, Type>();
        private static Dictionary<Type, Type> itemViewTypeCache = new Dictionary<Type, Type>();

        static GraphTypeCache()
        {
            InitNodeViewCache();
            InitEdgeViewCache();
            InitItemViewCache();
        }

        private static void InitNodeViewCache()
        {
            IList<Type> types = TypeCache.GetTypesDerivedFrom<IEditorNodeView>();
            for (var i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                if (type.IsAbstract || type.IsInterface) continue;
                EditorNodeAttribute editorNodeAttribute = ReflectUtility.GetAttribute<EditorNodeAttribute>(type);
                if (editorNodeAttribute == null) continue;
                nodeViewTypeCache.Add(editorNodeAttribute.nodeType, type);
            }
        }

        private static void InitEdgeViewCache()
        {
            IList<Type> types = TypeCache.GetTypesDerivedFrom<IEditorEdgeView>();
            for (var i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                if (type.IsAbstract || type.IsInterface) continue;
                EditorEdgeAttribute editorEdgeAttribute = ReflectUtility.GetAttribute<EditorEdgeAttribute>(type);
                if (editorEdgeAttribute == null) continue;
                edgeViewTypeCache.Add(editorEdgeAttribute.edgeType, type);
            }
        }

        private static void InitItemViewCache()
        {
            IList<Type> types = TypeCache.GetTypesDerivedFrom<IEditorItemView>();
            for (var i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                if (type.IsAbstract || type.IsInterface) continue;
                EditorItemAttribute editorItemAttribute = ReflectUtility.GetAttribute<EditorItemAttribute>(type);
                if (editorItemAttribute == null) continue;
                itemViewTypeCache.Add(editorItemAttribute.itemType, type);
            }
        }

        public static Type GetNodeViewType(Type nodeType)
        {
            if (nodeType == null) return null;
            return nodeViewTypeCache.GetValueOrDefault(nodeType);
        }

        public static Type GetEdgeViewType(Type edgeType)
        {
            if (edgeType == null) return null;
            return edgeViewTypeCache.GetValueOrDefault(edgeType);
        }

        public static Type GetItemViewType(Type itemType)
        {
            if (itemType == null) return null;
            return itemViewTypeCache.GetValueOrDefault(itemType);
        }
    }
}