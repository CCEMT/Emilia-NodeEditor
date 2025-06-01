using System;
using System.Collections.Generic;
using Emilia.Kit;
using Emilia.Kit.Editor;
using Emilia.Node.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Node.Universal.Editor
{
    [EditorHandle(typeof(EditorUniversalGraphAsset))]
    public class UniversalCreateNodeMenuHandle : CreateNodeMenuHandle
    {
        public CreateNodeMenuProvider createNodeMenuProvider { get; private set; }

        public override void Initialize(EditorGraphView graphView)
        {
            base.Initialize(graphView);
            createNodeMenuProvider = ScriptableObject.CreateInstance<CreateNodeMenuProvider>();
        }

        public override void InitializeCache(EditorGraphView graphView, List<ICreateNodeHandle> createNodeHandles)
        {
            InitializeRuntimeNodeCache(graphView, createNodeHandles);
            InitializeEditorNodeCache(graphView, createNodeHandles);
        }

        private void InitializeRuntimeNodeCache(EditorGraphView graphView, List<ICreateNodeHandle> createNodeHandles)
        {
            Type assetType = graphView.graphAsset.GetType();
            NodeToRuntimeAttribute attribute = assetType.GetCustomAttribute<NodeToRuntimeAttribute>(true);
            if (attribute == null) return;

            IList<Type> types = TypeCache.GetTypesDerivedFrom(attribute.baseRuntimeNodeType);
            int amount = types.Count;
            for (int i = 0; i < amount; i++)
            {
                Type type = types[i];
                if (type.IsAbstract || type.IsInterface || type.IsGenericType) continue;

                CreateNodeHandleContext createNodeHandleContext = new CreateNodeHandleContext();
                createNodeHandleContext.nodeType = type;
                createNodeHandleContext.defaultEditorNodeType = attribute.baseEditorNodeType;

                ICreateNodeHandle nodeHandle = EditorHandleUtility.CreateHandle<ICreateNodeHandle>(type);
                if (nodeHandle == null) continue;

                nodeHandle.Initialize(createNodeHandleContext);
                graphView.createNodeMenu.createNodeHandleCacheList.Add(nodeHandle);
            }
        }

        private void InitializeEditorNodeCache(EditorGraphView graphView, List<ICreateNodeHandle> createNodeHandles)
        {
            Type assetType = graphView.graphAsset.GetType();
            NodeToEditorAttribute attribute = assetType.GetCustomAttribute<NodeToEditorAttribute>(true);
            if (attribute == null) return;

            IList<Type> types = TypeCache.GetTypesDerivedFrom(attribute.baseEditorNodeType);
            int amount = types.Count;
            for (int i = 0; i < amount; i++)
            {
                Type type = types[i];
                if (type.IsAbstract || type.IsInterface) continue;

                NodeMenuAttribute nodeMenuAttribute = type.GetCustomAttribute<NodeMenuAttribute>();
                if (nodeMenuAttribute == null) continue;

                CreateNodeHandle createNodeHandle = new CreateNodeHandle();
                createNodeHandle.path = nodeMenuAttribute.path;
                createNodeHandle.priority = nodeMenuAttribute.priority;
                createNodeHandle.editorNodeType = type;

                graphView.createNodeMenu.createNodeHandleCacheList.Add(createNodeHandle);
            }
        }

        public override void ShowCreateNodeMenu(EditorGraphView graphView, CreateNodeContext createNodeContext)
        {
            base.ShowCreateNodeMenu(graphView, createNodeContext);
            createNodeMenuProvider.Initialize(graphView, createNodeContext);
            if (createNodeMenuProvider.createNodeContext.nodeMenu == null) return;
            SearchWindowContext searchWindowContext = new SearchWindowContext(createNodeContext.screenMousePosition);
            SearchWindow_Hook.Open<CreateNodeMenuProvider, SearchWindow_Hook>(searchWindowContext, createNodeMenuProvider);
        }

        public override void CollectAllCreateNodeInfos(EditorGraphView graphView, List<MenuNodeInfo> createNodeInfos, CreateNodeContext createNodeContext)
        {
            base.CollectAllCreateNodeInfos(graphView, createNodeInfos, createNodeContext);
            int amount = graphView.createNodeMenu.createNodeHandleCacheList.Count;
            for (int i = 0; i < amount; i++)
            {
                ICreateNodeHandle nodeHandle = graphView.createNodeMenu.createNodeHandleCacheList[i];
                if (nodeHandle.validity == false) continue;

                MenuNodeInfo menuNodeInfo = new MenuNodeInfo();
                menuNodeInfo.nodeData = nodeHandle.nodeData;
                menuNodeInfo.editorNodeAssetType = nodeHandle.editorNodeType;
                menuNodeInfo.path = nodeHandle.path;
                menuNodeInfo.priority = nodeHandle.priority;
                menuNodeInfo.icon = nodeHandle.icon;
                createNodeInfos.Add(menuNodeInfo);
            }
        }

        public override void Dispose(EditorGraphView graphView)
        {
            base.Dispose(graphView);
            if (createNodeMenuProvider != null)
            {
                Object.DestroyImmediate(createNodeMenuProvider);
                createNodeMenuProvider = null;
            }
        }
    }
}