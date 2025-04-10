using System;
using System.Collections.Generic;
using Emilia.Kit.Editor;
using Emilia.Node.Attributes;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Emilia.Node.Editor
{
    public static class EditorHandleUtility
    {
        private static List<Type> _handleTypes;

        private static Dictionary<Type, Type[]> handleTypeCache;

        private static Dictionary<Type, Queue<IEditorHandle>> handlePool;

        public static IReadOnlyList<Type> handleTypes => _handleTypes;

        static EditorHandleUtility()
        {
            handlePool = new Dictionary<Type, Queue<IEditorHandle>>();

            IList<Type> types = TypeCache.GetTypesDerivedFrom<IEditorHandle>();
            _handleTypes = new List<Type>();

            for (var i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                if (type.IsAbstract || type.IsInterface) continue;
                _handleTypes.Add(type);
            }

            Dictionary<Type, List<EditorHandlePriorityPair>> typeCache = new Dictionary<Type, List<EditorHandlePriorityPair>>();

            int amount = _handleTypes.Count;
            for (int i = 0; i < amount; i++)
            {
                Type type = _handleTypes[i];

                Type[] openGenericClass = null;

                IList<Type> genericHandleTypes = TypeCache.GetTypesWithAttribute(typeof(GenericHandleAttribute));

                int genericHandleTypeAmount = genericHandleTypes.Count;
                for (int j = 0; j < genericHandleTypeAmount; j++)
                {
                    Type genericHandleType = genericHandleTypes[j];
                    openGenericClass = type.GetArgumentsOfInheritedOpenGenericClass(genericHandleType);
                    if (openGenericClass != null) break;
                }

                if (openGenericClass == null) continue;

                int openGenericClassLength = openGenericClass.Length;
                for (int j = 0; j < openGenericClassLength; j++)
                {
                    Type openGeneric = openGenericClass[j];
                    if (typeCache.TryGetValue(openGeneric, out List<EditorHandlePriorityPair> list) == false) list = typeCache[openGeneric] = new List<EditorHandlePriorityPair>();
                    EditorHandlePriorityPair pair = new EditorHandlePriorityPair();
                    pair.type = type;

                    pair.priority = GetHandlePriority(type);
                    Insert(list, pair);
                }
            }

            handleTypeCache = new Dictionary<Type, Type[]>();
            foreach (var pair in typeCache) handleTypeCache[pair.Key] = pair.Value.ConvertAll(p => p.type).ToArray();
        }

        private static void Insert(List<EditorHandlePriorityPair> list, EditorHandlePriorityPair item)
        {
            int left = 0;
            int right = list.Count - 1;
            while (left <= right)
            {
                int mid = (left + right) / 2;
                if (list[mid].priority.value < item.priority.value) left = mid + 1;
                else right = mid - 1;
            }

            list.Insert(left, item);
        }

        /// <summary>
        /// 获取Handle优先级
        /// </summary>
        public static EditorHandlePriority GetHandlePriority(Type handleType)
        {
            EditorHandlePriorityAttribute priorityAttribute = handleType.GetCustomAttribute<EditorHandlePriorityAttribute>();

            EditorHandlePriority result = new EditorHandlePriority();

            if (priorityAttribute != null) result.value = priorityAttribute.priority;
            else
            {
                result.value = 0;

                IList<Type> subTypes = TypeCache.GetTypesDerivedFrom(handleType);

                int maxLevel = 0;
                for (var i = 0; i < subTypes.Count; i++)
                {
                    Type subType = subTypes[i];
                    int depth = 0;
                    Type currentType = subType;
                    while (currentType != handleType)
                    {
                        currentType = currentType.BaseType;
                        depth++;
                    }

                    if (depth > maxLevel) maxLevel = depth;
                }

                result.value = maxLevel * 0.01f;
            }

            return result;
        }

        /// <summary>
        /// 构建Handle
        /// </summary>
        public static T BuildHandle<T>(Type type, object value)
        {
            Type filterType = typeof(T);
            List<IEditorHandle> handles = new List<IEditorHandle>();

            Type currentType = type;

            while (currentType != null)
            {
                Type[] types = null;

                Type[] interfaces = ReflectUtility.GetDirectInterfaces(currentType);
                int interfaceCount = interfaces.Length;
                for (int i = 0; i < interfaceCount; i++)
                {
                    Type handleType = interfaces[i];
                    types = handleTypeCache.GetValueOrDefault(handleType);
                    if (types != null) break;
                }
                
                if (handleTypeCache.TryGetValue(currentType, out types) == false)
                {
                    currentType = currentType.BaseType;
                    continue;
                }

                int amount = types.Length;
                for (int i = 0; i < amount; i++)
                {
                    Type handleType = types[i];
                    if (filterType.IsAssignableFrom(handleType) == false) continue;

                    IEditorHandle handle = GetHandle(handleType);
                    if (handles.Count > 0) handles[handles.Count - 1].parent = handle;
                    handles.Add(handle);
                }

                currentType = currentType.BaseType;
            }

            handles.ForEach((h) => h.Initialize(value));

            if (handles.Count > 0) return (T) handles[0];
            return default;
        }

        /// <summary>
        /// 获取Handle
        /// </summary>
        private static IEditorHandle GetHandle(Type type)
        {
            if (handlePool.TryGetValue(type, out Queue<IEditorHandle> queue) == false) queue = handlePool[type] = new Queue<IEditorHandle>();
            if (queue.Count > 0) return queue.Dequeue();

            if (type.IsSubclassOf(typeof(ScriptableObject))) return (IEditorHandle) ScriptableObject.CreateInstance(type);
            return (IEditorHandle) ReflectUtility.CreateInstance(type);
        }

        /// <summary>
        /// 释放Handle
        /// </summary>
        public static void ReleaseHandle(IEditorHandle handle)
        {
            Type type = handle.GetType();
            if (handlePool.TryGetValue(type, out Queue<IEditorHandle> queue) == false) handlePool[type] = new Queue<IEditorHandle>();

            while (handle.parent != null)
            {
                queue.Enqueue(handle);
                handle = handle.parent;

                IDisposable disposable = handle as IDisposable;
                disposable?.Dispose();
            }
        }
    }
}