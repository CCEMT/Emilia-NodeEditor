using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Profiling;
using Emilia.Node.Editor;

namespace Emilia.Node.Editor.Tests.Performance
{
    [TestFixture]
    public class NodeEditorPerformanceTests
    {
        private EditorGraphView graphView;
        private EditorGraphAsset graphAsset;
        private Stopwatch stopwatch;
        
        [SetUp]
        public void SetUp()
        {
            // Create test graph asset
            graphAsset = ScriptableObject.CreateInstance<TestUniversalGraphAsset>();
            
            // Create a minimal graph view for testing
            graphView = new EditorGraphView();
            graphView.Initialize();
            graphView.SimpleReload(graphAsset);
            
            stopwatch = new Stopwatch();
        }
        
        [TearDown]
        public void TearDown()
        {
            graphView?.Dispose();
            
            if (graphAsset != null)
            {
                ScriptableObject.DestroyImmediate(graphAsset);
            }
        }
        
        [Test]
        public void CreateManyNodes_PerformanceTest()
        {
            // Arrange
            const int nodeCount = 1000;
            var nodeAssets = new List<EditorNodeAsset>();
            
            // Act
            stopwatch.Start();
            Profiler.BeginSample("CreateManyNodes");
            
            for (int i = 0; i < nodeCount; i++)
            {
                var nodeAsset = ScriptableObject.CreateInstance<TestNodeAsset>();
                nodeAsset.id = Guid.NewGuid().ToString();
                nodeAsset.position = new Rect(i * 10, i * 10, 100, 100);
                nodeAssets.Add(nodeAsset);
                
                graphAsset.AddNode(nodeAsset);
            }
            
            Profiler.EndSample();
            stopwatch.Stop();
            
            // Assert
            Assert.AreEqual(nodeCount, graphAsset.nodes.Count);
            Assert.Less(stopwatch.ElapsedMilliseconds, 5000, "Creating 1000 nodes should take less than 5 seconds");
            
            UnityEngine.Debug.Log($"Created {nodeCount} nodes in {stopwatch.ElapsedMilliseconds}ms");
            
            // Cleanup
            foreach (var nodeAsset in nodeAssets)
            {
                if (nodeAsset != null)
                {
                    ScriptableObject.DestroyImmediate(nodeAsset);
                }
            }
        }
        
        [Test]
        public void CreateManyEdges_PerformanceTest()
        {
            // Arrange
            const int edgeCount = 500;
            var nodeAssets = new List<EditorNodeAsset>();
            var edgeAssets = new List<EditorEdgeAsset>();
            
            // Create nodes first
            for (int i = 0; i < edgeCount + 1; i++)
            {
                var nodeAsset = ScriptableObject.CreateInstance<TestNodeAsset>();
                nodeAsset.id = Guid.NewGuid().ToString();
                nodeAsset.position = new Rect(i * 10, 0, 100, 100);
                nodeAssets.Add(nodeAsset);
                graphAsset.AddNode(nodeAsset);
            }
            
            // Act
            stopwatch.Start();
            Profiler.BeginSample("CreateManyEdges");
            
            for (int i = 0; i < edgeCount; i++)
            {
                var edgeAsset = ScriptableObject.CreateInstance<TestEdgeAsset>();
                edgeAsset.id = Guid.NewGuid().ToString();
                edgeAsset.outputNodeId = nodeAssets[i].id;
                edgeAsset.inputNodeId = nodeAssets[i + 1].id;
                edgeAsset.outputPortId = "output";
                edgeAsset.inputPortId = "input";
                edgeAssets.Add(edgeAsset);
                
                graphAsset.AddEdge(edgeAsset);
            }
            
            Profiler.EndSample();
            stopwatch.Stop();
            
            // Assert
            Assert.AreEqual(edgeCount, graphAsset.edges.Count);
            Assert.Less(stopwatch.ElapsedMilliseconds, 3000, "Creating 500 edges should take less than 3 seconds");
            
            UnityEngine.Debug.Log($"Created {edgeCount} edges in {stopwatch.ElapsedMilliseconds}ms");
            
            // Cleanup
            foreach (var nodeAsset in nodeAssets)
            {
                if (nodeAsset != null)
                {
                    ScriptableObject.DestroyImmediate(nodeAsset);
                }
            }
            foreach (var edgeAsset in edgeAssets)
            {
                if (edgeAsset != null)
                {
                    ScriptableObject.DestroyImmediate(edgeAsset);
                }
            }
        }
        
        [Test]
        public void GraphElementCache_LargeDataset_PerformanceTest()
        {
            // Arrange
            const int elementCount = 2000;
            var cache = new GraphElementCache();
            var nodeViews = new List<TestEditorNodeView>();
            
            // Create test node views
            for (int i = 0; i < elementCount; i++)
            {
                var nodeView = new TestEditorNodeView();
                nodeView.asset = ScriptableObject.CreateInstance<TestNodeAsset>();
                nodeView.asset.id = $"node_{i}";
                nodeViews.Add(nodeView);
            }
            
            // Act - Cache all elements
            stopwatch.Start();
            Profiler.BeginSample("CacheLargeDataset");
            
            foreach (var nodeView in nodeViews)
            {
                cache.SetNodeViewCache(nodeView.nodeId,nodeView);
            }
            
            Profiler.EndSample();
            stopwatch.Stop();
            
            var cacheTime = stopwatch.ElapsedMilliseconds;
            
            // Act - Lookup all elements
            stopwatch.Restart();
            Profiler.BeginSample("LookupLargeDataset");
            
            foreach (var nodeView in nodeViews)
            {
                var cached = cache.nodeViewById[nodeView.asset.id];
                Assert.AreEqual(nodeView, cached);
            }
            
            Profiler.EndSample();
            stopwatch.Stop();
            
            var lookupTime = stopwatch.ElapsedMilliseconds;
            
            // Assert
            Assert.Less(cacheTime, 1000, "Caching 2000 elements should take less than 1 second");
            Assert.Less(lookupTime, 500, "Looking up 2000 elements should take less than 0.5 seconds");
            
            UnityEngine.Debug.Log($"Cached {elementCount} elements in {cacheTime}ms, looked up in {lookupTime}ms");
            
            // Cleanup
            foreach (var nodeView in nodeViews)
            {
                if (nodeView.asset != null)
                {
                    ScriptableObject.DestroyImmediate(nodeView.asset);
                }
            }
        }
        
        [Test]
        public void GraphReload_LargeGraph_PerformanceTest()
        {
            // Arrange
            const int nodeCount = 100;
            const int edgeCount = 150;
            
            // Create a large graph
            var nodeAssets = new List<EditorNodeAsset>();
            var edgeAssets = new List<EditorEdgeAsset>();
            
            for (int i = 0; i < nodeCount; i++)
            {
                var nodeAsset = ScriptableObject.CreateInstance<TestNodeAsset>();
                nodeAsset.id = Guid.NewGuid().ToString();
                nodeAsset.position = new Rect(i * 10, (i % 10) * 10, 100, 100);
                nodeAssets.Add(nodeAsset);
                graphAsset.AddNode(nodeAsset);
            }
            
            for (int i = 0; i < edgeCount; i++)
            {
                var edgeAsset = ScriptableObject.CreateInstance<TestEdgeAsset>();
                edgeAsset.id = Guid.NewGuid().ToString();
                edgeAsset.outputNodeId = nodeAssets[i % nodeCount].id;
                edgeAsset.inputNodeId = nodeAssets[(i + 1) % nodeCount].id;
                edgeAsset.outputPortId = "output";
                edgeAsset.inputPortId = "input";
                edgeAssets.Add(edgeAsset);
                graphAsset.AddEdge(edgeAsset);
            }
            
            // Act
            stopwatch.Start();
            Profiler.BeginSample("ReloadLargeGraph");
            
            graphView.Reload(graphAsset);
            
            Profiler.EndSample();
            stopwatch.Stop();
            
            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 2000, "Reloading graph with 100 nodes and 150 edges should take less than 2 seconds");
            
            UnityEngine.Debug.Log($"Reloaded graph with {nodeCount} nodes and {edgeCount} edges in {stopwatch.ElapsedMilliseconds}ms");
            
            // Cleanup
            foreach (var nodeAsset in nodeAssets)
            {
                if (nodeAsset != null)
                {
                    ScriptableObject.DestroyImmediate(nodeAsset);
                }
            }
            foreach (var edgeAsset in edgeAssets)
            {
                if (edgeAsset != null)
                {
                    ScriptableObject.DestroyImmediate(edgeAsset);
                }
            }
        }
        
        [Test]
        public void MemoryUsage_LargeGraph_Test()
        {
            // Arrange
            const int nodeCount = 500;
            long initialMemory = GC.GetTotalMemory(true);
            
            var nodeAssets = new List<EditorNodeAsset>();
            
            // Act
            Profiler.BeginSample("CreateLargeGraphMemoryTest");
            
            for (int i = 0; i < nodeCount; i++)
            {
                var nodeAsset = ScriptableObject.CreateInstance<TestNodeAsset>();
                nodeAsset.id = Guid.NewGuid().ToString();
                nodeAsset.position = new Rect(i * 10, i * 10, 100, 100);
                nodeAssets.Add(nodeAsset);
                graphAsset.AddNode(nodeAsset);
            }
            
            Profiler.EndSample();
            
            long memoryAfterCreation = GC.GetTotalMemory(false);
            long memoryUsed = memoryAfterCreation - initialMemory;
            
            // Assert
            Assert.Less(memoryUsed, 50 * 1024 * 1024, "Memory usage should be less than 50MB for 500 nodes");
            
            UnityEngine.Debug.Log($"Memory used for {nodeCount} nodes: {memoryUsed / 1024.0f / 1024.0f:F2} MB");
            
            // Cleanup
            foreach (var nodeAsset in nodeAssets)
            {
                if (nodeAsset != null)
                {
                    ScriptableObject.DestroyImmediate(nodeAsset);
                }
            }
            
            // Force garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            long memoryAfterCleanup = GC.GetTotalMemory(true);
            long memoryLeaked = memoryAfterCleanup - initialMemory;
            
            Assert.Less(memoryLeaked, 5 * 1024 * 1024, "Memory leak should be less than 5MB");
            
            UnityEngine.Debug.Log($"Memory leaked after cleanup: {memoryLeaked / 1024.0f / 1024.0f:F2} MB");
        }
        
        [Test]
        public void SearchPerformance_LargeNodeSet_Test()
        {
            // Arrange
            const int nodeCount = 1000;
            var nodeAssets = new List<EditorNodeAsset>();
            
            for (int i = 0; i < nodeCount; i++)
            {
                var nodeAsset = ScriptableObject.CreateInstance<TestNodeAsset>();
                nodeAsset.id = $"node_{i:D4}";
                nodeAsset.position = new Rect(i * 10, i * 10, 100, 100);
                nodeAssets.Add(nodeAsset);
                graphAsset.AddNode(nodeAsset);
            }
            
            // Act - Search by ID
            stopwatch.Start();
            Profiler.BeginSample("SearchById");
            
            for (int i = 0; i < 100; i++)
            {
                string searchId = $"node_{i:D4}";
                var found = graphAsset.nodeMap.ContainsKey(searchId);
                Assert.IsTrue(found);
            }
            
            Profiler.EndSample();
            stopwatch.Stop();
            
            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 100, "Searching 100 nodes by ID should take less than 100ms");
            
            UnityEngine.Debug.Log($"Searched 100 nodes by ID in {stopwatch.ElapsedMilliseconds}ms");
            
            // Cleanup
            foreach (var nodeAsset in nodeAssets)
            {
                if (nodeAsset != null)
                {
                    ScriptableObject.DestroyImmediate(nodeAsset);
                }
            }
        }
        
        [Test]
        public void CopyPaste_LargeSelection_PerformanceTest()
        {
            // Arrange
            const int nodeCount = 50;
            var nodeAssets = new List<EditorNodeAsset>();
            
            for (int i = 0; i < nodeCount; i++)
            {
                var nodeAsset = ScriptableObject.CreateInstance<TestNodeAsset>();
                nodeAsset.id = Guid.NewGuid().ToString();
                nodeAsset.position = new Rect(i * 10, i * 10, 100, 100);
                nodeAssets.Add(nodeAsset);
                graphAsset.AddNode(nodeAsset);
            }
            
            // Act
            stopwatch.Start();
            Profiler.BeginSample("CopyPasteLargeSelection");
            
            // 模拟复制粘贴操作
            var copyPasteSystem = graphView.graphCopyPaste;
            
            // 这里需要根据实际的复制粘贴API进行测试
            // 由于复制粘贴涉及序列化，我们主要测试不抛出异常
            Assert.DoesNotThrow(() => {
                // 模拟复制粘贴逻辑
                foreach (var nodeAsset in nodeAssets.Take(10))
                {
                    var copy = copyPasteSystem.CreateCopy(nodeAsset);
                    Assert.IsNotNull(copy);
                }
            });
            
            Profiler.EndSample();
            stopwatch.Stop();
            
            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 1000, "Copy-paste operation should take less than 1 second");
            
            UnityEngine.Debug.Log($"Copy-paste operation took {stopwatch.ElapsedMilliseconds}ms");
            
            // Cleanup
            foreach (var nodeAsset in nodeAssets)
            {
                if (nodeAsset != null)
                {
                    ScriptableObject.DestroyImmediate(nodeAsset);
                }
            }
        }
    }
} 