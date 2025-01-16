using System;
using System.Collections.Generic;
using Emilia.Node.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Node.Universal.Editor
{
    public class UniversalCreateNodeMenuHandle : CreateNodeMenuHandle<EditorUniversalGraphAsset>
    {
        public CreateNodeMenuProvider createNodeMenuProvider { get; private set; }

        public override void InitializeCache()
        {
            createNodeMenuProvider = ScriptableObject.CreateInstance<CreateNodeMenuProvider>();

            InitializeRuntimeNodeCache();
            InitializeEditorNodeCache();
        }

        private void InitializeRuntimeNodeCache()
        {
            Type assetType = smartValue.graphAsset.GetType();
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

                ICreateNodeHandle nodeHandle = EditorHandleUtility.BuildHandle<ICreateNodeHandle>(type, createNodeHandleContext);
                if (nodeHandle == null) continue;
                smartValue.createNodeMenu.createNodeHandleCacheList.Add(nodeHandle);
            }
        }

        private void InitializeEditorNodeCache()
        {
            Type assetType = smartValue.graphAsset.GetType();
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

                smartValue.createNodeMenu.createNodeHandleCacheList.Add(createNodeHandle);
            }
        }

        public override void MenuCreateInitialize(CreateNodeContext createNodeContext)
        {
            createNodeMenuProvider.Initialize(createNodeContext);
        }

        public override void ShowCreateNodeMenu(NodeCreationContext c)
        {
            SearchWindowContext searchWindowContext = new SearchWindowContext(c.screenMousePosition);
            SearchWindow.Open(searchWindowContext, createNodeMenuProvider);
        }

        public override void CollectAllCreateNodeInfos(List<CreateNodeInfo> createNodeInfos, CreateNodeContext createNodeContext)
        {
            int amount = smartValue.createNodeMenu.createNodeHandleCacheList.Count;
            for (int i = 0; i < amount; i++)
            {
                ICreateNodeHandle nodeHandle = smartValue.createNodeMenu.createNodeHandleCacheList[i];
                if (nodeHandle.validity == false) continue;

                CreateNodeInfo createNodeInfo = new CreateNodeInfo();
                createNodeInfo.nodeData = nodeHandle.nodeData;
                createNodeInfo.editorNodeAssetType = nodeHandle.editorNodeType;
                createNodeInfo.path = nodeHandle.path;
                createNodeInfo.priority = nodeHandle.priority;
                createNodeInfo.icon = nodeHandle.icon;
                createNodeInfos.Add(createNodeInfo);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            if (createNodeMenuProvider != null)
            {
                Object.DestroyImmediate(createNodeMenuProvider);
                createNodeMenuProvider = null;
            }
        }
    }
}