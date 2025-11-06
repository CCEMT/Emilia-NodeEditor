using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Emilia.Reflection.Editor;
using MonoHook;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Emilia.Kit.Editor
{
    public class Edge_Hook : Edge_Internals
    {
        [InitializeOnLoadMethod]
        static void InstallationHook()
        {
            Type edgeType = typeof(Edge);
            Type graphViewHookType = typeof(Edge_Hook);

            HookCtor(edgeType, graphViewHookType);
        }

        private static void HookCtor(Type edgeType, Type graphViewHookType)
        {
            MethodBase methodInfo = edgeType.GetConstructor(new Type[] { });

            MethodInfo hookInfo = graphViewHookType.GetMethod(nameof(Ctor_Hook), BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo proxyInfo = graphViewHookType.GetMethod(nameof(Ctor_Proxy), BindingFlags.Instance | BindingFlags.NonPublic);

            MethodHook hook = new(methodInfo, hookInfo, proxyInfo);
            hook.Install();
        }

        private void Ctor_Hook()
        {
            if (OverrideCtor()) return;
            Ctor_Proxy();
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private void Ctor_Proxy()
        {
            Debug.Log(nameof(Ctor_Proxy));
        }

        protected virtual bool OverrideCtor() => false;
    }
}