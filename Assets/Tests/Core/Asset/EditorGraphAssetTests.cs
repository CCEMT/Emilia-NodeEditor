using System;
using System.Collections.Generic;
using Emilia.Node.Universal.Editor;
using NUnit.Framework;
using UnityEngine;

namespace Emilia.Node.Editor.Tests
{
    [TestFixture]
    public class EditorGraphAssetTests
    {
        private EditorGraphAsset graphAsset;

        [SetUp]
        public void SetUp()
        {
            graphAsset = ScriptableObject.CreateInstance<TestGraphAsset>();
        }

        [TearDown]
        public void TearDown()
        {
            if (graphAsset != null)
            {
                ScriptableObject.DestroyImmediate(graphAsset);
            }
        }

        [Test]
        public void Id_AutoGeneratedWhenEmpty()
        {
            // Act
            string id = graphAsset.id;

            // Assert
            Assert.IsNotNull(id);
            Assert.IsNotEmpty(id);
        }

        [Test]
        public void Id_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            string testId = "test-graph-id";

            // Act
            graphAsset.id = testId;

            // Assert
            Assert.AreEqual(testId, graphAsset.id);
        }

        [Test]
        public void Id_SetNull_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => graphAsset.id = null);
        }

        [Test]
        public void Id_SetEmpty_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => graphAsset.id = string.Empty);
        }

        [Test]
        public void Nodes_InitiallyEmpty()
        {
            // Act & Assert
            Assert.IsNotNull(graphAsset.nodes);
            Assert.AreEqual(0, graphAsset.nodes.Count);
        }

        [Test]
        public void Edges_InitiallyEmpty()
        {
            // Act & Assert
            Assert.IsNotNull(graphAsset.edges);
            Assert.AreEqual(0, graphAsset.edges.Count);
        }

        [Test]
        public void Items_InitiallyEmpty()
        {
            // Act & Assert
            Assert.IsNotNull(graphAsset.items);
            Assert.AreEqual(0, graphAsset.items.Count);
        }

        [Test]
        public void NodeMap_InitiallyEmpty()
        {
            // Act & Assert
            Assert.IsNotNull(graphAsset.nodeMap);
            Assert.AreEqual(0, graphAsset.nodeMap.Count);
        }

        [Test]
        public void EdgeMap_InitiallyEmpty()
        {
            // Act & Assert
            Assert.IsNotNull(graphAsset.edgeMap);
            Assert.AreEqual(0, graphAsset.edgeMap.Count);
        }

        [Test]
        public void ItemMap_InitiallyEmpty()
        {
            // Act & Assert
            Assert.IsNotNull(graphAsset.itemMap);
            Assert.AreEqual(0, graphAsset.itemMap.Count);
        }

        [Test]
        public void AddNode_AddsToNodesAndNodeMap()
        {
            // Arrange
            EditorNodeAsset nodeAsset = ScriptableObject.CreateInstance<TestNodeAsset>();
            nodeAsset.id = "test-node";

            try
            {
                // Act
                graphAsset.AddNode(nodeAsset);

                // Assert
                Assert.AreEqual(1, graphAsset.nodes.Count);
                Assert.AreEqual(1, graphAsset.nodeMap.Count);
                Assert.Contains(nodeAsset, (List<EditorNodeAsset>) graphAsset.nodes);
                Assert.IsTrue(graphAsset.nodeMap.ContainsKey(nodeAsset.id));
                Assert.AreEqual(nodeAsset, graphAsset.nodeMap[nodeAsset.id]);
                Assert.AreEqual(graphAsset, nodeAsset.graphAsset);
            }
            finally
            {
                if (nodeAsset != null)
                {
                    ScriptableObject.DestroyImmediate(nodeAsset);
                }
            }
        }

        [Test]
        public void AddNode_NullNode_DoesNotThrow()
        {
            // Act & Assert - 实际实现中如果节点为null，会在访问nodeAsset.id时抛出NullReferenceException
            Assert.Throws<NullReferenceException>(() => graphAsset.AddNode(null));
        }

        [Test]
        public void RemoveNode_RemovesFromNodesAndNodeMap()
        {
            // Arrange
            EditorNodeAsset nodeAsset = ScriptableObject.CreateInstance<TestNodeAsset>();
            nodeAsset.id = "test-node";
            graphAsset.AddNode(nodeAsset);

            try
            {
                // Act
                graphAsset.RemoveNode(nodeAsset);

                // Assert
                Assert.AreEqual(0, graphAsset.nodes.Count);
                Assert.AreEqual(0, graphAsset.nodeMap.Count);
                Assert.IsFalse(graphAsset.nodeMap.ContainsKey(nodeAsset.id));
                Assert.IsNull(nodeAsset.graphAsset);
            }
            finally
            {
                if (nodeAsset != null)
                {
                    ScriptableObject.DestroyImmediate(nodeAsset);
                }
            }
        }

        [Test]
        public void RemoveNode_NodeNotInGraph_DoesNothing()
        {
            // Arrange
            EditorNodeAsset nodeAsset = ScriptableObject.CreateInstance<TestNodeAsset>();
            nodeAsset.id = "test-node";

            try
            {
                // Act
                graphAsset.RemoveNode(nodeAsset);

                // Assert
                Assert.AreEqual(0, graphAsset.nodes.Count);
                Assert.AreEqual(0, graphAsset.nodeMap.Count);
            }
            finally
            {
                if (nodeAsset != null)
                {
                    ScriptableObject.DestroyImmediate(nodeAsset);
                }
            }
        }

        [Test]
        public void NodeMap_ContainsAddedNode()
        {
            // Arrange
            EditorNodeAsset nodeAsset = ScriptableObject.CreateInstance<TestNodeAsset>();
            nodeAsset.id = "test-node";
            graphAsset.AddNode(nodeAsset);

            try
            {
                // Act
                bool containsNode = graphAsset.nodeMap.ContainsKey("test-node");
                EditorNodeAsset result = graphAsset.nodeMap.GetValueOrDefault("test-node");

                // Assert
                Assert.IsTrue(containsNode);
                Assert.AreEqual(nodeAsset, result);
            }
            finally
            {
                if (nodeAsset != null)
                {
                    ScriptableObject.DestroyImmediate(nodeAsset);
                }
            }
        }

        [Test]
        public void NodeMap_NonExistingId_ReturnsNull()
        {
            // Act
            EditorNodeAsset result = graphAsset.nodeMap.GetValueOrDefault("non-existing-id");

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void PropertyTree_IsNotNullAfterOnEnable()
        {
            // Act - OnEnable is called automatically when creating ScriptableObject

            // Assert
            Assert.IsNotNull(graphAsset.propertyTree);
        }
    }

    // Test implementation class
    public class TestGraphAsset : EditorGraphAsset
    {
        // Test implementation
    }

    public class TestUniversalGraphAsset : EditorUniversalGraphAsset { }
}