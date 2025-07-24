using System;
using System.Collections.Generic;
using Emilia.Node.Attributes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Emilia.Node.Editor;

namespace Emilia.Node.Editor.Tests
{
    [TestFixture]
    public class EditorNodeAssetTests
    {
        private EditorNodeAsset nodeAsset;
        
        [SetUp]
        public void SetUp()
        {
            nodeAsset = ScriptableObject.CreateInstance<TestNodeAsset>();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (nodeAsset != null)
            {
                ScriptableObject.DestroyImmediate(nodeAsset);
            }
        }
        
        [Test]
        public void Id_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            string testId = "test-node-id";
            
            // Act
            nodeAsset.id = testId;
            
            // Assert
            Assert.AreEqual(testId, nodeAsset.id);
        }
        
        [Test]
        public void Position_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            Rect testPosition = new Rect(100, 200, 300, 400);
            
            // Act
            nodeAsset.position = testPosition;
            
            // Assert
            Assert.AreEqual(testPosition, nodeAsset.position);
        }
        
        [Test]
        public void UserData_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            string testData = "test user data";
            
            // Act
            nodeAsset.userData = testData;
            
            // Assert
            Assert.AreEqual(testData, nodeAsset.userData);
        }
        
        [Test]
        public void GraphAsset_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            EditorGraphAsset graphAsset = ScriptableObject.CreateInstance<TestGraphAsset>();
            
            try
            {
                // Act
                nodeAsset.graphAsset = graphAsset;
                
                // Assert
                Assert.AreEqual(graphAsset, nodeAsset.graphAsset);
            }
            finally
            {
                if (graphAsset != null)
                {
                    ScriptableObject.DestroyImmediate(graphAsset);
                }
            }
        }
        
        [Test]
        public void Title_ReturnsDefaultValue()
        {
            // Act & Assert
            Assert.NotNull(nodeAsset.title);
            Assert.AreNotEqual("", nodeAsset.title);
        }
        
        [Test]
        public void PropertyTree_IsNotNullAfterOnEnable()
        {
            // Act - OnEnable is called automatically when creating ScriptableObject
            
            // Assert
            Assert.IsNotNull(nodeAsset.propertyTree);
        }
    }
    
    // Test implementation classes
    public class TestNodeAsset : EditorNodeAsset
    {
        public override string title => "Test Node";
    }
    
    [EditorNode(typeof(TestNodeAsset))]
    public class TestNodeView: EditorNodeView
    {
        public override List<EditorPortInfo> CollectStaticPortAssets()
        {
            var ports = new List<EditorPortInfo>();
            
            // 添加一个输入端口
            var inputPort = new EditorPortInfo();
            inputPort.id = "input";
            inputPort.displayName = "Input";
            inputPort.direction = EditorPortDirection.Input;
            inputPort.portType = typeof(float);
            inputPort.orientation = EditorOrientation.Horizontal;
            inputPort.canMultiConnect = false;
            inputPort.order = 0;
            inputPort.color = Color.white;
            ports.Add(inputPort);
            
            // 添加一个输出端口
            var outputPort = new EditorPortInfo();
            outputPort.id = "output";
            outputPort.displayName = "Output";
            outputPort.direction = EditorPortDirection.Output;
            outputPort.portType = typeof(float);
            outputPort.orientation = EditorOrientation.Horizontal;
            outputPort.canMultiConnect = false;
            outputPort.order = 1;
            outputPort.color = Color.white;
            ports.Add(outputPort);
            
            return ports;
        }
    }
} 