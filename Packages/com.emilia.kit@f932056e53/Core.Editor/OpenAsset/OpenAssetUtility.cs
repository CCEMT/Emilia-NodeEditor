using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using Object = UnityEngine.Object;

namespace Emilia.Kit.Editor
{
    public static class OpenAssetUtility
    {
        public static Func<Object, int, bool> openAssetInvoker = AssetDatabase.OpenAsset;

        public static bool Open(Object asset, OpenAssetOptions options) => Open(asset, 0, options);

        public static bool Open(Object asset, int lineNumber = 0, OpenAssetOptions options = null)
        {
            if (asset == null) return false;

            OpenAssetContext context = BuildContext(asset, lineNumber, options);
            using (OpenAssetPendingContextStore.Push(context))
            {
                return openAssetInvoker(asset, lineNumber);
            }
        }

        public static OpenAssetContext BuildContext(Object source, int lineNumber, OpenAssetOptions options = null) => BuildContextCore(source, lineNumber, options?.parameters);

        public static OpenAssetContext BuildContextFromInstanceId(int instanceId, int lineNumber)
        {
            Object source = EditorUtility.InstanceIDToObject(instanceId);
            return BuildContextCore(source, lineNumber, null);
        }

        private static OpenAssetContext BuildContextCore(Object source, int lineNumber, IReadOnlyDictionary<string, object> parameters)
        {
            string assetPath = source == null ? string.Empty : AssetDatabase.GetAssetPath(source);
            AssetImporter assetImporter = string.IsNullOrEmpty(assetPath) ? null : AssetImporter.GetAtPath(assetPath);
            Object asset = ResolveMainAsset(source, assetPath);
            int instanceId = source == null ? 0 : source.GetInstanceID();

            return new OpenAssetContext(source, asset, assetPath, assetImporter, instanceId, lineNumber, parameters);
        }

        private static Object ResolveMainAsset(Object source, string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return source;

            Object mainAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            return mainAsset == null ? source : mainAsset;
        }
    }

    public static class OpenAssetPendingContextStore
    {
        private static readonly List<PendingContext> pendingContexts = new();

        public static int pendingCount => pendingContexts.Count;

        public static IDisposable Push(OpenAssetContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            PendingContext pendingContext = new(context);
            pendingContexts.Add(pendingContext);
            return new PendingContextScope(pendingContext);
        }

        public static bool TryConsume(int instanceId, int lineNumber, out OpenAssetContext context)
        {
            for (int i = pendingContexts.Count - 1; i >= 0; i--)
            {
                PendingContext pendingContext = pendingContexts[i];
                if (pendingContext.context.instanceId != instanceId) continue;
                if (pendingContext.context.lineNumber != lineNumber) continue;

                pendingContexts.RemoveAt(i);
                pendingContext.isConsumed = true;
                context = pendingContext.context;
                return true;
            }

            context = null;
            return false;
        }

        public static void Clear()
        {
            pendingContexts.Clear();
        }

        private sealed class PendingContext
        {
            public readonly OpenAssetContext context;
            public bool isConsumed;

            public PendingContext(OpenAssetContext context)
            {
                this.context = context;
            }
        }

        private sealed class PendingContextScope : IDisposable
        {
            private PendingContext pendingContext;

            public PendingContextScope(PendingContext pendingContext)
            {
                this.pendingContext = pendingContext;
            }

            public void Dispose()
            {
                if (this.pendingContext == null) return;
                if (this.pendingContext.isConsumed == false) pendingContexts.Remove(this.pendingContext);

                this.pendingContext = null;
            }
        }
    }

    public static class OpenAssetCallbackBridge
    {
        public static Action<Action> delayCallInvoker = action => EditorApplication.delayCall += () => action();

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int lineNumber)
        {
            if (OpenAssetPendingContextStore.TryConsume(instanceId, lineNumber, out OpenAssetContext context) == false)
            {
                context = OpenAssetUtility.BuildContextFromInstanceId(instanceId, lineNumber);
            }

            delayCallInvoker(() => OpenAssetProcessorRunner.Run(context));
            return false;
        }
    }
}