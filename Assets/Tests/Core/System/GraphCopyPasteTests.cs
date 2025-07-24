using System;
using System.Collections.Generic;
using Emilia.Kit.Editor;
using NUnit.Framework;
using UnityEngine;

namespace Emilia.Node.Editor.Tests
{
    [TestFixture]
    public class GraphCopyPasteTests
    {
        private GraphCopyPaste copyPasteSystem;
        private EditorGraphView graphView;
        private EditorGraphAsset graphAsset;
        
        [SetUp]
        public void SetUp()
        {
            // Create test graph asset
            graphAsset = ScriptableObject.CreateInstance<TestUniversalGraphAsset>();
            
            // Create a minimal graph view for testing
            graphView = new EditorGraphView();
            graphView.Initialize();
            graphView.SimpleReload(graphAsset);
            
            // Create and initialize the copy paste system
            copyPasteSystem = new GraphCopyPaste();
            copyPasteSystem.Initialize(graphView);
        }
        
        [TearDown]
        public void TearDown()
        {
            copyPasteSystem?.Dispose();
            
            if (graphAsset != null)
            {
                ScriptableObject.DestroyImmediate(graphAsset);
            }
        }
        
        [Test]
        public void Initialize_SetsGraphViewReference()
        {
            // Arrange
            var testCopyPasteSystem = new GraphCopyPaste();
            
            // Act
            testCopyPasteSystem.Initialize(graphView);
            
            // Assert
            Assert.AreEqual(graphView, ReflectUtility.GetValue(testCopyPasteSystem, "graphView"));
            
            // Cleanup
            testCopyPasteSystem.Dispose();
        }
        
        [Test]
        public void CreateCopy_ValidObject_ReturnsNonNullCopy()
        {
            // Arrange
            var testData = "test string data";
            
            // Act
            var result = copyPasteSystem.CreateCopy(testData);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(testData, result);
        }
        
        [Test]
        public void CreateCopy_NullObject_ReturnsNull()
        {
            // Act
            var result = copyPasteSystem.CreateCopy(null);
            
            // Assert
            Assert.IsNull(result);
        }
        
        [Test]
        public void CreateCopy_ComplexObject_ReturnsEquivalentCopy()
        {
            // Arrange
            var testData = new TestSerializableData
            {
                IntValue = 42,
                StringValue = "test string",
                BoolValue = true
            };
            
            // Act
            var result = copyPasteSystem.CreateCopy(testData) as TestSerializableData;
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(testData.IntValue, result.IntValue);
            Assert.AreEqual(testData.StringValue, result.StringValue);
            Assert.AreEqual(testData.BoolValue, result.BoolValue);
        }
        
        [Test]
        public void SerializeGraphElements_EmptyList_ReturnsNonNull()
        {
            // Arrange
            var elements = new List<UnityEditor.Experimental.GraphView.GraphElement>();
            
            // Act
            string result = copyPasteSystem.SerializeGraphElementsCallback(elements);
            
            // Assert
            Assert.IsNotNull(result);
        }
        
        [Test]
        public void CanPasteSerializedData_ValidData_ReturnsTrue()
        {
            // Arrange
            var elements = new List<UnityEditor.Experimental.GraphView.GraphElement>();
            string serializedData = copyPasteSystem.SerializeGraphElementsCallback(elements);
            
            // Act
            bool result = copyPasteSystem.CanPasteSerializedDataCallback(serializedData);
            
            // Assert
            Assert.IsTrue(result);
        }
        
        [Test]
        public void CanPasteSerializedData_NullData_ReturnsFalse()
        {
            // Act
            bool result = copyPasteSystem.CanPasteSerializedDataCallback(null);
            
            // Assert
            Assert.IsFalse(result);
        }
        
        [Test]
        public void CanPasteSerializedData_EmptyData_ReturnsFalse()
        {
            // Act
            bool result = copyPasteSystem.CanPasteSerializedDataCallback("");
            
            // Assert
            Assert.IsFalse(result);
        }
        
        [Test]
        public void CanPasteSerializedData_InvalidData_ReturnsFalse()
        {
            // Act
            bool result = copyPasteSystem.CanPasteSerializedDataCallback("invalid json data");
            
            // Assert
            Assert.IsFalse(result);
        }
        
        [Test]
        public void UnserializeAndPasteCallback_ValidData_DoesNotThrow()
        {
            // Arrange
            var elements = new List<UnityEditor.Experimental.GraphView.GraphElement>();
            string serializedData = copyPasteSystem.SerializeGraphElementsCallback(elements);
            
            // Act & Assert
            Assert.DoesNotThrow(() => copyPasteSystem.UnserializeAndPasteCallback(null, serializedData));
        }
    }
    
    // Test data class
    [Serializable]
    public class TestSerializableData
    {
        public int IntValue;
        public string StringValue;
        public bool BoolValue;
    }
} 