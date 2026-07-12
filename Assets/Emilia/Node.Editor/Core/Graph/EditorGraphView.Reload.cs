using System;
using System.Collections;
using Emilia.Kit;
using Emilia.Kit.Editor;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace Emilia.Node.Editor
{
    public partial class EditorGraphView
    {
        /// <summary>
        /// 重新加载
        /// </summary>
        public void Reload(EditorGraphAsset asset)
        {
            if (asset == null)
            {
                Debug.LogError("Reload asset 为空");
                return;
            }

            graphViews[asset] = this;

            onGraphAssetChange?.Invoke(asset);
            loadProgress = 0;
            isInitialized = false;

            if (loadElementCoroutine != null) EditorCoroutineUtility.StopCoroutine(loadElementCoroutine);
            loadElementCoroutine = null;

            bool allReload = graphAsset == null || graphAsset.GetType() != asset.GetType();
            graphAsset = asset;

            ReloadData();

            schedule.Execute(OnReload).ExecuteLater(1);

            void OnReload()
            {
                if (allReload) AllReload();
                else ElementReload();
            }
        }

        /// <summary>
        /// 简单加载
        /// </summary>
        public void SimpleReload(EditorGraphAsset asset)
        {
            if (asset == null)
            {
                Debug.LogError("SimpleLoad asset 为空");
                return;
            }

            graphAsset = asset;

            ReloadData();
            ReloadHandle();
            ReloadModule();
            RemoveAllElement();

            graphElementCache.BuildCache(this);
            loadProgress = 1;
            isInitialized = true;
        }

        private void AllReload()
        {
            ReloadHandle();
            ReloadModule();

            RemoveAllElement();
            loadElementCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(LoadElement());
        }

        private void ReloadHandle()
        {
            if (this.graphHandle != null) graphHandle.Dispose(this);
            this.graphHandle = EditorHandleUtility.CreateHandle<GraphHandle>(graphAsset.GetType());
            this.graphHandle?.Initialize(this);
        }

        private void ReloadData()
        {
            Type currentType = graphAsset.GetType();

            datas.Clear();

            while (currentType != null)
            {
                Type graphType = GraphDataCache.GetGraphDataType(currentType);
                if (graphType != null)
                {
                    GraphData graphData = ReflectUtility.CreateInstance(graphType) as GraphData;
                    if (graphData != null)
                    {
                        graphData.OnCreate(this);
                        this.datas.Add(graphData);
                    }
                }

                if (currentType == typeof(EditorGraphAsset)) break;
                currentType = currentType.BaseType;
            }
        }

        private void ReloadModule()
        {
            foreach (CustomGraphViewModule customModule in this.customModules.Values) customModule.Dispose();
            this.customModules.Clear();

            foreach (BasicGraphViewModule module in this.modules.Values) module.Dispose();

            foreach (BasicGraphViewModule module in this.modules.Values) module.Initialize(this);

            graphHandle?.InitializeCustomModule(this, customModules);

            foreach (CustomGraphViewModule customModule in this.customModules.Values) customModule.Initialize(this);

            foreach (BasicGraphViewModule module in this.modules.Values) module.AllModuleInitializeSuccess();
            foreach (CustomGraphViewModule customModule in this.customModules.Values) customModule.AllModuleInitializeSuccess();

            graphHandle?.AllModuleInitializeSuccess(this);
        }

        private void ElementReload()
        {
            RemoveAllElement();
            loadElementCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(LoadElement());
        }

        private IEnumerator LoadElement()
        {
            graphElementCache.BuildCache(this);

            this.graphHandle?.OnLoadBefore(this);

            yield return LoadNodeView();
            yield return LoadEdge();
            yield return LoadItem();

            LoadSuccess();
        }

        private IEnumerator LoadNodeView()
        {
            if (graphAsset == null) yield break;

            int amount = graphAsset.nodes.Count;
            for (int i = 0; i < amount; i++)
            {
                if (graphAsset == null) yield break;

                EditorNodeAsset node = graphAsset.nodes[i];
                AddNodeView(node);

                loadProgress = (i + 1) / (float) (graphAsset.nodes.Count + graphAsset.edges.Count + graphAsset.items.Count);
                if (EditorApplication.timeSinceStartup - lastUpdateTime > maxLoadTimeMs) yield return 0;
            }
        }

        private IEnumerator LoadEdge()
        {
            if (graphAsset == null) yield break;

            int amount = graphAsset.edges.Count;
            for (int i = 0; i < amount; i++)
            {
                if (graphAsset == null) yield break;

                EditorEdgeAsset edge = graphAsset.edges[i];
                AddEdgeView(edge);

                loadProgress = (graphAsset.nodes.Count + i + 1) / (float) (graphAsset.nodes.Count + graphAsset.edges.Count + graphAsset.items.Count);
                if (EditorApplication.timeSinceStartup - lastUpdateTime > maxLoadTimeMs) yield return 0;
            }
        }

        private IEnumerator LoadItem()
        {
            if (graphAsset == null) yield break;

            int amount = graphAsset.items.Count;
            for (int i = 0; i < amount; i++)
            {
                if (graphAsset == null) yield break;

                EditorItemAsset itemAsset = graphAsset.items[i];
                AddItemView(itemAsset);

                loadProgress = (graphAsset.nodes.Count + graphAsset.edges.Count + i + 1) / (float) (graphAsset.nodes.Count + graphAsset.edges.Count + graphAsset.items.Count);
                if (EditorApplication.timeSinceStartup - lastUpdateTime > maxLoadTimeMs) yield return 0;
            }
        }

        private void LoadSuccess()
        {
            if (graphAsset == null)
            {
                Debug.LogError("加载失败 graphAsset 为空");
                return;
            }

            loadElementCoroutine = null;
            loadProgress = 1;
            isInitialized = true;

            this.graphHandle?.OnLoadAfter(this);
        }
    }
}
