using NUnit.Framework;
using UnityEngine;

namespace Emilia.Node.Editor.Tests
{
    [TestFixture]
    public class EditorEdgeAssetTests
    {
        private EditorEdgeAsset edgeAsset;

        [SetUp]
        public void SetUp()
        {
            edgeAsset = ScriptableObject.CreateInstance<TestEdgeAsset>();
        }

        [TearDown]
        public void TearDown()
        {
            if (edgeAsset != null)
            {
                ScriptableObject.DestroyImmediate(edgeAsset);
            }
        }

        [Test]
        public void Id_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            string testId = "test-edge-id";

            // Act
            edgeAsset.id = testId;

            // Assert
            Assert.AreEqual(testId, edgeAsset.id);
        }

        [Test]
        public void InputNodeId_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            string testInputNodeId = "input-node-id";

            // Act
            edgeAsset.inputNodeId = testInputNodeId;

            // Assert
            Assert.AreEqual(testInputNodeId, edgeAsset.inputNodeId);
        }

        [Test]
        public void OutputNodeId_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            string testOutputNodeId = "output-node-id";

            // Act
            edgeAsset.outputNodeId = testOutputNodeId;

            // Assert
            Assert.AreEqual(testOutputNodeId, edgeAsset.outputNodeId);
        }

        [Test]
        public void InputPortId_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            string testInputPortId = "input-port-id";

            // Act
            edgeAsset.inputPortId = testInputPortId;

            // Assert
            Assert.AreEqual(testInputPortId, edgeAsset.inputPortId);
        }

        [Test]
        public void OutputPortId_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            string testOutputPortId = "output-port-id";

            // Act
            edgeAsset.outputPortId = testOutputPortId;

            // Assert
            Assert.AreEqual(testOutputPortId, edgeAsset.outputPortId);
        }

        [Test]
        public void GraphAsset_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            EditorGraphAsset graphAsset = ScriptableObject.CreateInstance<TestGraphAsset>();

            try
            {
                // Act
                edgeAsset.graphAsset = graphAsset;

                // Assert
                Assert.AreEqual(graphAsset, edgeAsset.graphAsset);
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
            Assert.NotNull(edgeAsset.title);
            Assert.AreNotEqual("", edgeAsset.title);
        }

        [Test]
        public void PropertyTree_IsNotNullAfterOnEnable()
        {
            // Act - OnEnable is called automatically when creating ScriptableObject

            // Assert
            Assert.IsNotNull(edgeAsset.propertyTree);
        }

        [Test]
        public void UserData_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            string testData = "test edge data";

            // Act
            edgeAsset.userData = testData;

            // Assert
            Assert.AreEqual(testData, edgeAsset.userData);
        }
    }

    // Test implementation classes
    public class TestEdgeAsset : EditorEdgeAsset
    {
        public override string title => "Test Edge";
    }

    [EditorEdge(typeof(TestEdgeAsset))]
    public class TestEdgeView : EditorEdgeView { }
}