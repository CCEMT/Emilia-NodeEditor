using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Emilia.Kit;
using NUnit.Framework;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor.Tests.Integration
{
    [TestFixture]
    public class CompleteWorkflowTests
    {
        private EditorGraphView graphView;
        private EditorGraphAsset graphAsset;
        private GraphNodeSystem nodeSystem;
        private GraphConnectSystem connectSystem;
        private GraphCopyPaste copyPasteSystem;
        private GraphSelected selectedSystem;
        private GraphSave saveSystem;
        private GraphUndo undoSystem;

        [SetUp]
        public void SetUp()
        {
            // Create test graph asset
            graphAsset = ScriptableObject.CreateInstance<TestUniversalGraphAsset>();

            // Create and initialize graph view
            graphView = new EditorGraphView();
            graphView.Initialize();
            graphView.SimpleReload(graphAsset);

            // Get all systems
            nodeSystem = graphView.nodeSystem;
            connectSystem = graphView.connectSystem;
            copyPasteSystem = graphView.graphCopyPaste;
            selectedSystem = graphView.graphSelected;
            saveSystem = graphView.graphSave;
            undoSystem = graphView.graphUndo;
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
        public void CompleteNodeLifecycle_CreateConnectCopyDelete_WorksCorrectly()
        {
            // Phase 1: Create nodes
            var node1 = nodeSystem.CreateNode(typeof(TestNodeAsset), new Vector2(100, 100));
            var node2 = nodeSystem.CreateNode(typeof(TestNodeAsset), new Vector2(300, 100));

            Assert.IsNotNull(node1);
            Assert.IsNotNull(node2);

            // Phase 2: Create connection (if supported)
            // Note: This would require actual port views and connection logic
            // For now, we'll create edges manually
            var edge = ScriptableObject.CreateInstance<TestEdgeAsset>();
            edge.id = Guid.NewGuid().ToString();
            edge.outputNodeId = node1.id;
            edge.inputNodeId = node2.id;
            edge.outputPortId = "output";
            edge.inputPortId = "input";

            graphAsset.AddEdge(edge);

            Assert.AreEqual(1, graphAsset.edges.Count);

            // Phase 3: Copy nodes
            var originalCount = graphAsset.nodes.Count;
            var copiedNode = copyPasteSystem.CreateCopy(node1);

            Assert.IsNotNull(copiedNode);

            // Phase 4: Select and validate
            var selectables = new List<ISelectedHandle>();
            // Note: In real scenario, we'd select actual graph elements
            selectedSystem.UpdateSelected(selectables);

            // Phase 5: Save
            saveSystem.SetDirty();
            Assert.IsTrue(saveSystem.dirty);

            saveSystem.OnSave();

            // Phase 6: Undo operations (if supported)
            undoSystem.OnUndoRedoPerformed();

            // Phase 7: Delete nodes
            graphAsset.RemoveNode(node1);
            graphAsset.RemoveNode(node2);
            graphAsset.RemoveEdge(edge);

            Assert.AreEqual(0, graphAsset.nodes.Count);
            Assert.AreEqual(0, graphAsset.edges.Count);

            // Cleanup
            if (node1 != null) ScriptableObject.DestroyImmediate(node1);
            if (node2 != null) ScriptableObject.DestroyImmediate(node2);
            if (edge != null) ScriptableObject.DestroyImmediate(edge);
            if ((Object) copiedNode != null) ScriptableObject.DestroyImmediate((Object) copiedNode);
        }

        [Test]
        public void ComplexGraphOperations_MultipleNodesAndEdges_WorksCorrectly()
        {
            // Create a complex graph structure
            var nodes = new List<EditorNodeAsset>();
            var edges = new List<EditorEdgeAsset>();

            // Create 5 nodes
            for (int i = 0; i < 5; i++)
            {
                var node = nodeSystem.CreateNode(typeof(TestNodeAsset), new Vector2(i * 100, 0));
                nodes.Add(node);
                this.graphAsset.AddNode(node);
            }

            // Create edges to form a chain
            for (int i = 0; i < 4; i++)
            {
                var edge = ScriptableObject.CreateInstance<TestEdgeAsset>();
                edge.id = Guid.NewGuid().ToString();
                edge.outputNodeId = nodes[i].id;
                edge.inputNodeId = nodes[i + 1].id;
                edge.outputPortId = "output";
                edge.inputPortId = "input";
                edges.Add(edge);

                graphAsset.AddEdge(edge);
            }

            // Verify graph structure
            Assert.AreEqual(5, graphAsset.nodes.Count);
            Assert.AreEqual(4, graphAsset.edges.Count);

            // Test graph traversal
            var firstNode = nodes[0];
            var connectedEdges = graphAsset.edges.Where(e => e.outputNodeId == firstNode.id).ToList();
            Assert.AreEqual(1, connectedEdges.Count);

            // Test node removal with edge cleanup
            var nodeToRemove = nodes[2]; // Middle node
            var affectedEdges = graphAsset.edges.Where(e =>
                e.outputNodeId == nodeToRemove.id || e.inputNodeId == nodeToRemove.id).ToList();

            // Remove affected edges first
            foreach (var edge in affectedEdges)
            {
                graphAsset.RemoveEdge(edge);
            }

            // Remove the node
            graphAsset.RemoveNode(nodeToRemove);

            Assert.AreEqual(4, graphAsset.nodes.Count);
            Assert.AreEqual(2, graphAsset.edges.Count); // Should have 2 edges left

            // Cleanup
            foreach (var node in nodes)
            {
                if (node != null) ScriptableObject.DestroyImmediate(node);
            }
            foreach (var edge in edges)
            {
                if (edge != null) ScriptableObject.DestroyImmediate(edge);
            }
        }

        [Test]
        public void SystemIntegration_AllSystemsWorkTogether_NoConflicts()
        {
            // Test that all systems can work together without conflicts

            // 1. Create nodes using node system
            var node1 = nodeSystem.CreateNode(typeof(TestNodeAsset), new Vector2(0, 0));
            var node2 = nodeSystem.CreateNode(typeof(TestNodeAsset), new Vector2(100, 0));

            // 2. Use copy-paste system
            var copiedNode = copyPasteSystem.CreateCopy(node1);
            Assert.IsNotNull(copiedNode);

            // 3. Use selection system
            var selectables = new List<ISelectedHandle>();
            selectedSystem.UpdateSelected(selectables);

            // 4. Use save system
            saveSystem.SetDirty();
            Assert.IsTrue(saveSystem.dirty);

            // 5. Use undo system
            undoSystem.OnUndoRedoPerformed();

            // 6. Use connect system (basic test)
            Assert.IsNotNull(connectSystem);

            // Verify no system conflicts
            Assert.AreEqual(2, graphAsset.nodes.Count);
            Assert.IsNotNull(graphView.nodeSystem);
            Assert.IsNotNull(graphView.connectSystem);
            Assert.IsNotNull(graphView.graphCopyPaste);
            Assert.IsNotNull(graphView.graphSelected);
            Assert.IsNotNull(graphView.graphSave);
            Assert.IsNotNull(graphView.graphUndo);

            // Cleanup
            if (node1 != null) ScriptableObject.DestroyImmediate(node1);
            if (node2 != null) ScriptableObject.DestroyImmediate(node2);
            if ((Object)copiedNode != null) ScriptableObject.DestroyImmediate((Object)copiedNode);
        }

        [Test]
        public void GraphReloadWorkflow_PreservesDataIntegrity()
        {
            // Create initial graph state
            var node1 = nodeSystem.CreateNode(typeof(TestNodeAsset), new Vector2(50, 50));
            var node2 = nodeSystem.CreateNode(typeof(TestNodeAsset), new Vector2(150, 50));

            var edge = ScriptableObject.CreateInstance<TestEdgeAsset>();
            edge.id = Guid.NewGuid().ToString();
            edge.outputNodeId = node1.id;
            edge.inputNodeId = node2.id;
            edge.outputPortId = "output";
            edge.inputPortId = "input";
            graphAsset.AddEdge(edge);

            // Store original state
            var originalNodeCount = graphAsset.nodes.Count;
            var originalEdgeCount = graphAsset.edges.Count;
            var originalNode1Id = node1.id;
            var originalNode2Id = node2.id;

            // Reload the graph
            graphView.Reload(graphAsset);

            // Verify data integrity after reload
            Assert.AreEqual(originalNodeCount, graphAsset.nodes.Count);
            Assert.AreEqual(originalEdgeCount, graphAsset.edges.Count);

            // Verify node IDs are preserved
            Assert.IsTrue(graphAsset.nodeMap.ContainsKey(originalNode1Id));
            Assert.IsTrue(graphAsset.nodeMap.ContainsKey(originalNode2Id));

            // Verify edge connections are preserved
            Assert.AreEqual(originalNode1Id, edge.outputNodeId);
            Assert.AreEqual(originalNode2Id, edge.inputNodeId);

            // Cleanup
            if (node1 != null) ScriptableObject.DestroyImmediate(node1);
            if (node2 != null) ScriptableObject.DestroyImmediate(node2);
            if (edge != null) ScriptableObject.DestroyImmediate(edge);
        }

        [Test]
        public void ErrorHandling_InvalidOperations_HandledGracefully()
        {
            // Test various error conditions

            // 1. Try to create node with invalid type
            Assert.DoesNotThrow(() => nodeSystem.CreateNode(null, Vector2.zero));

            // 2. Try to add null node
            Assert.DoesNotThrow(() => graphAsset.AddNode(null));

            // 3. Try to add null edge
            Assert.DoesNotThrow(() => graphAsset.AddEdge(null));

            // 4. Try to remove non-existent node
            var dummyNode = ScriptableObject.CreateInstance<TestNodeAsset>();
            dummyNode.id = "non-existent";
            Assert.DoesNotThrow(() => graphAsset.RemoveNode(dummyNode));

            // 5. Try to copy null object
            Assert.DoesNotThrow(() => copyPasteSystem.CreateCopy(null));

            // 6. Try to save with null graph view
            Assert.DoesNotThrow(() => saveSystem.OnSave());

            // 7. Try to update selection with null list
            Assert.DoesNotThrow(() => selectedSystem.UpdateSelected((List<ISelectedHandle>) null));

            // Verify graph is still in valid state
            Assert.IsNotNull(graphAsset);
            Assert.IsNotNull(graphView);

            // Cleanup
            if (dummyNode != null) ScriptableObject.DestroyImmediate(dummyNode);
        }

        [Test]
        public void PerformanceUnderLoad_ManyOperations_RemainsResponsive()
        {
            var stopwatch = Stopwatch.StartNew();

            // Perform many operations in sequence
            var nodes = new List<EditorNodeAsset>();

            // Create 50 nodes
            for (int i = 0; i < 50; i++)
            {
                var node = nodeSystem.CreateNode(typeof(TestNodeAsset), new Vector2(i * 10, 0));
                nodes.Add(node);
            }

            // Create edges between adjacent nodes
            var edges = new List<EditorEdgeAsset>();
            for (int i = 0; i < 49; i++)
            {
                var edge = ScriptableObject.CreateInstance<TestEdgeAsset>();
                edge.id = Guid.NewGuid().ToString();
                edge.outputNodeId = nodes[i].id;
                edge.inputNodeId = nodes[i + 1].id;
                edge.outputPortId = "output";
                edge.inputPortId = "input";
                edges.Add(edge);

                graphAsset.AddEdge(edge);
            }

            // Perform copy operations
            var copiedNodes = new List<EditorNodeAsset>();
            for (int i = 0; i < 10; i++)
            {
                var copied = copyPasteSystem.CreateCopy(nodes[i]) as EditorNodeAsset;
                if (copied != null)
                {
                    copiedNodes.Add(copied);
                }
            }

            // Mark as dirty and save
            saveSystem.SetDirty();
            saveSystem.OnSave();

            stopwatch.Stop();

            // Assert performance is reasonable
            Assert.Less(stopwatch.ElapsedMilliseconds, 5000, "Complex operations should complete within 5 seconds");

            // Verify final state
            Assert.AreEqual(50, graphAsset.nodes.Count);
            Assert.AreEqual(49, graphAsset.edges.Count);

            Debug.Log($"Complex workflow completed in {stopwatch.ElapsedMilliseconds}ms");

            // Cleanup
            foreach (var node in nodes)
            {
                if (node != null) ScriptableObject.DestroyImmediate(node);
            }
            foreach (var edge in edges)
            {
                if (edge != null) ScriptableObject.DestroyImmediate(edge);
            }
            foreach (var copied in copiedNodes)
            {
                if (copied != null) ScriptableObject.DestroyImmediate(copied);
            }
        }

        [Test]
        public void StateConsistency_AfterMultipleOperations_RemainsValid()
        {
            // Perform a series of operations and verify state consistency

            var initialNodeCount = graphAsset.nodes.Count;
            var initialEdgeCount = graphAsset.edges.Count;

            // Create nodes
            var node1 = nodeSystem.CreateNode(typeof(TestNodeAsset), Vector2.zero);
            var node2 = nodeSystem.CreateNode(typeof(TestNodeAsset), Vector2.one);

            // Verify state
            Assert.AreEqual(initialNodeCount + 2, graphAsset.nodes.Count);
            Assert.IsTrue(graphAsset.nodeMap.ContainsKey(node1.id));
            Assert.IsTrue(graphAsset.nodeMap.ContainsKey(node2.id));

            // Create edge
            var edge = ScriptableObject.CreateInstance<TestEdgeAsset>();
            edge.id = Guid.NewGuid().ToString();
            edge.outputNodeId = node1.id;
            edge.inputNodeId = node2.id;
            edge.outputPortId = "output";
            edge.inputPortId = "input";
            graphAsset.AddEdge(edge);

            // Verify state
            Assert.AreEqual(initialEdgeCount + 1, graphAsset.edges.Count);
            Assert.IsTrue(graphAsset.edgeMap.ContainsKey(edge.id));

            // Copy node
            var copiedNode = copyPasteSystem.CreateCopy(node1) as EditorNodeAsset;

            // Verify copy doesn't affect original
            Assert.IsNotNull(copiedNode);
            Assert.AreNotEqual(node1.id, copiedNode.id);
            Assert.AreEqual(initialNodeCount + 2, graphAsset.nodes.Count); // Still 2 nodes in graph

            // Remove edge
            graphAsset.RemoveEdge(edge);

            // Verify state
            Assert.AreEqual(initialEdgeCount, graphAsset.edges.Count);
            Assert.IsFalse(graphAsset.edgeMap.ContainsKey(edge.id));

            // Remove nodes
            graphAsset.RemoveNode(node1);
            graphAsset.RemoveNode(node2);

            // Verify final state
            Assert.AreEqual(initialNodeCount, graphAsset.nodes.Count);
            Assert.IsFalse(graphAsset.nodeMap.ContainsKey(node1.id));
            Assert.IsFalse(graphAsset.nodeMap.ContainsKey(node2.id));

            // Cleanup
            if (node1 != null) ScriptableObject.DestroyImmediate(node1);
            if (node2 != null) ScriptableObject.DestroyImmediate(node2);
            if (edge != null) ScriptableObject.DestroyImmediate(edge);
            if (copiedNode != null) ScriptableObject.DestroyImmediate(copiedNode);
        }
    }
}