using System;
using Emilia.Kit.Editor;
using NUnit.Framework;
using UnityEngine;

namespace Emilia.Node.Editor.Tests
{
    [TestFixture]
    public class GraphNodeSystemTests
    {
        private GraphNodeSystem nodeSystem;
        private EditorGraphView graphView;
        private EditorGraphAsset graphAsset;
        
        [SetUp]
        public void SetUp()
        {
            // Create test graph asset
            graphAsset = ScriptableObject.CreateInstance<TestGraphAsset>();
            
            // Create a minimal graph view for testing
            graphView = new EditorGraphView();
            graphView.Initialize();
            graphView.SimpleReload(graphAsset);
            
            // Create and initialize the node system
            nodeSystem = new GraphNodeSystem();
            nodeSystem.Initialize(graphView);
        }
        
        [TearDown]
        public void TearDown()
        {
            nodeSystem?.Dispose();
            
            if (graphAsset != null)
            {
                ScriptableObject.DestroyImmediate(graphAsset);
            }
        }
        
        [Test]
        public void Order_ReturnsCorrectValue()
        {
            // Act & Assert
            Assert.AreEqual(900, nodeSystem.order);
        }
        
        [Test]
        public void Initialize_SetsGraphViewReference()
        {
            // Arrange
            var testNodeSystem = new GraphNodeSystem();
            
            // Act
            testNodeSystem.Initialize(graphView);
            
            // Assert
            Assert.AreEqual(graphView, ReflectUtility.GetValue(testNodeSystem, "graphView"));
            
            // Cleanup
            testNodeSystem.Dispose();
        }
        
        [Test]
        public void CreateNode_ValidNodeType_ReturnsNodeAsset()
        {
            // Arrange
            Type nodeType = typeof(TestNodeAsset);
            Vector2 position = new Vector2(100, 100);
            
            // Act
            EditorNodeAsset result = nodeSystem.CreateNode(nodeType, position);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<TestNodeAsset>(result);
            Assert.AreEqual(new Rect(position, new Vector2(100, 100)), result.position);
            Assert.IsNotNull(result.id);
            Assert.IsNotEmpty(result.id);
            
            // Cleanup
            if (result != null)
            {
                ScriptableObject.DestroyImmediate(result);
            }
        }
        
        [Test]
        public void CreateNode_InvalidNodeType_ReturnsNull()
        {
            // Arrange
            Type invalidType = typeof(string); // Not a EditorNodeAsset type
            Vector2 position = new Vector2(100, 100);
            
            // Act
            EditorNodeAsset result = nodeSystem.CreateNode(invalidType, position);
            
            // Assert
            Assert.IsNull(result);
        }
        
        [Test]
        public void CreateNode_WithValidTypeAndPosition_SetsCorrectProperties()
        {
            // Arrange
            Type nodeType = typeof(TestNodeAsset);
            Vector2 position = new Vector2(200, 300);
            
            // Act
            EditorNodeAsset result = nodeSystem.CreateNode(nodeType, position);
            
            try
            {
                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(200, result.position.x);
                Assert.AreEqual(300, result.position.y);
                Assert.AreEqual(100, result.position.width);
                Assert.AreEqual(100, result.position.height);
                
                // Verify GUID format
                Assert.IsTrue(Guid.TryParse(result.id, out _));
            }
            finally
            {
                if (result != null)
                {
                    ScriptableObject.DestroyImmediate(result);
                }
            }
        }
        
        [Test]
        public void CreateNode_MultipleNodes_GenerateUniqueIds()
        {
            // Arrange
            Type nodeType = typeof(TestNodeAsset);
            Vector2 position = new Vector2(100, 100);
            
            // Act
            EditorNodeAsset node1 = nodeSystem.CreateNode(nodeType, position);
            EditorNodeAsset node2 = nodeSystem.CreateNode(nodeType, position);
            
            try
            {
                // Assert
                Assert.IsNotNull(node1);
                Assert.IsNotNull(node2);
                Assert.AreNotEqual(node1.id, node2.id);
            }
            finally
            {
                if (node1 != null) ScriptableObject.DestroyImmediate(node1);
                if (node2 != null) ScriptableObject.DestroyImmediate(node2);
            }
        }
    }
} 