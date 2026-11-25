using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Emilia.Kit.Editor
{
    public readonly struct OpenAssetProcessorEntry
    {
        public readonly IOpenAssetProcessor processor;
        public readonly int order;
        public readonly string typeName;

        public OpenAssetProcessorEntry(IOpenAssetProcessor processor, int order, string typeName)
        {
            this.processor = processor;
            this.order = order;
            this.typeName = typeName;
        }
    }

    public static class OpenAssetProcessorRegistry
    {
        private static readonly List<IOpenAssetProcessor> _processors = new();
        private static bool isInitialized;

        public static IReadOnlyList<IOpenAssetProcessor> processors
        {
            get
            {
                EnsureInitialized();
                return _processors;
            }
        }

        public static void Refresh()
        {
            _processors.Clear();
            isInitialized = false;
            EnsureInitialized();
        }

        private static void EnsureInitialized()
        {
            if (isInitialized) return;

            isInitialized = true;
            List<OpenAssetProcessorEntry> entries = new();
            IList<Type> types = TypeCache.GetTypesDerivedFrom<IOpenAssetProcessor>();

            int count = types.Count;
            for (int i = 0; i < count; i++)
            {
                Type type = types[i];
                if (type.IsAbstract || type.IsInterface) continue;

                OpenAssetProcessorAttribute attribute = type.GetCustomAttribute<OpenAssetProcessorAttribute>();
                if (attribute == null) continue;

                try
                {
                    IOpenAssetProcessor processor = ReflectUtility.CreateInstance(type) as IOpenAssetProcessor;
                    if (processor == null) continue;

                    entries.Add(new OpenAssetProcessorEntry(processor, attribute.order, type.FullName));
                }
                catch (Exception e)
                {
                    Debug.LogError($"OpenAsset EnsureInitialized error type={type.FullName}\n{e}");
                }
            }

            SortEntries(entries);
            for (int i = 0; i < entries.Count; i++) _processors.Add(entries[i].processor);
        }

        public static List<OpenAssetProcessorEntry> CreateEntries(IEnumerable<IOpenAssetProcessor> processorSource)
        {
            List<OpenAssetProcessorEntry> entries = new();
            if (processorSource == null) return entries;

            foreach (IOpenAssetProcessor processor in processorSource)
            {
                if (processor == null) continue;

                Type type = processor.GetType();
                OpenAssetProcessorAttribute attribute = type.GetCustomAttribute<OpenAssetProcessorAttribute>();
                if (attribute == null) continue;

                entries.Add(new OpenAssetProcessorEntry(processor, attribute.order, type.FullName));
            }

            SortEntries(entries);
            return entries;
        }

        private static void SortEntries(List<OpenAssetProcessorEntry> entries)
        {
            entries.Sort((a, b) => {
                int orderCompare = a.order.CompareTo(b.order);
                if (orderCompare != 0) return orderCompare;

                return string.Compare(a.typeName, b.typeName, StringComparison.Ordinal);
            });
        }
    }

    public static class OpenAssetProcessorRunner
    {
        public static void Run(OpenAssetContext context)
        {
            Run(context, OpenAssetProcessorRegistry.processors);
        }

        public static void Run(OpenAssetContext context, IEnumerable<IOpenAssetProcessor> processors)
        {
            if (context == null) return;

            List<OpenAssetProcessorEntry> entries = OpenAssetProcessorRegistry.CreateEntries(processors);
            int count = entries.Count;
            for (int i = 0; i < count; i++)
            {
                OpenAssetProcessorEntry entry = entries[i];
                IOpenAssetProcessor processor = entry.processor;
                if (processor == null) continue;

                bool canProcess;
                try
                {
                    canProcess = processor.CanProcess(context);
                }
                catch (Exception e)
                {
                    Debug.LogError($"OpenAsset Run error type={entry.typeName}, asset={context.assetPath}\n{e}");
                    continue;
                }

                if (canProcess == false) continue;

                try
                {
                    processor.Process(context);
                }
                catch (Exception e)
                {
                    Debug.LogError($"OpenAsset Run error type={entry.typeName}, asset={context.assetPath}\n{e}");
                }
            }
        }
    }
}