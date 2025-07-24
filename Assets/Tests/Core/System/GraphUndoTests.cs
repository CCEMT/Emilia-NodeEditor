using Emilia.Kit.Editor;
using NUnit.Framework;
using UnityEngine;

namespace Emilia.Node.Editor.Tests
{
    [TestFixture]
    public class GraphUndoTests
    {
        private GraphUndo undoSystem;
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

            // Create and initialize the undo system
            undoSystem = new GraphUndo();
            undoSystem.Initialize(graphView);
        }

        [TearDown]
        public void TearDown()
        {
            undoSystem?.Dispose();

            if (graphAsset != null)
            {
                ScriptableObject.DestroyImmediate(graphAsset);
            }
        }

        [Test]
        public void Order_ReturnsCorrectValue()
        {
            // Act & Assert
            Assert.AreEqual(400, undoSystem.order);
        }

        [Test]
        public void Initialize_SetsGraphViewReference()
        {
            // Arrange
            var testUndoSystem = new GraphUndo();

            // Act
            testUndoSystem.Initialize(graphView);

            // Assert
            Assert.AreEqual(graphView, ReflectUtility.GetValue(testUndoSystem, "graphView"));

            // Cleanup
            testUndoSystem.Dispose();
        }

        [Test]
        public void OnUndoRedoPerformed_DefaultSilent_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => undoSystem.OnUndoRedoPerformed());
        }

        [Test]
        public void OnUndoRedoPerformed_SilentTrue_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => undoSystem.OnUndoRedoPerformed(true));
        }

        [Test]
        public void OnUndoRedoPerformed_SilentFalse_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => undoSystem.OnUndoRedoPerformed(false));
        }

        [Test]
        public void OnUndoRedoPerformed_WithNodesInGraph_DoesNotThrow()
        {
            // Arrange
            var nodeAsset = ScriptableObject.CreateInstance<TestNodeAsset>();
            nodeAsset.id = "test-node";
            graphAsset.AddNode(nodeAsset);

            try
            {
                // Act & Assert
                Assert.DoesNotThrow(() => undoSystem.OnUndoRedoPerformed(true));
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
        public void OnUndoRedoPerformed_WithEdgesInGraph_DoesNotThrow()
        {
            // Arrange
            var edgeAsset = ScriptableObject.CreateInstance<TestEdgeAsset>();
            edgeAsset.id = "test-edge";
            graphAsset.AddEdge(edgeAsset);

            try
            {
                // Act & Assert
                Assert.DoesNotThrow(() => undoSystem.OnUndoRedoPerformed(true));
            }
            finally
            {
                if (edgeAsset != null)
                {
                    ScriptableObject.DestroyImmediate(edgeAsset);
                }
            }
        }

        [Test]
        public void OnUndoRedoPerformed_WithItemsInGraph_DoesNotThrow()
        {
            // Arrange
            var itemAsset = ScriptableObject.CreateInstance<TestItemAsset>();
            itemAsset.id = "test-item";
            graphAsset.AddItem(itemAsset);

            try
            {
                // Act & Assert
                Assert.DoesNotThrow(() => undoSystem.OnUndoRedoPerformed(true));
            }
            finally
            {
                if (itemAsset != null)
                {
                    ScriptableObject.DestroyImmediate(itemAsset);
                }
            }
        }

        [Test]
        public void OnUndoRedoPerformed_MultipleElements_DoesNotThrow()
        {
            // Arrange
            var nodeAsset = ScriptableObject.CreateInstance<TestNodeAsset>();
            nodeAsset.id = "test-node";
            graphAsset.AddNode(nodeAsset);

            var edgeAsset = ScriptableObject.CreateInstance<TestEdgeAsset>();
            edgeAsset.id = "test-edge";
            graphAsset.AddEdge(edgeAsset);

            var itemAsset = ScriptableObject.CreateInstance<TestItemAsset>();
            itemAsset.id = "test-item";
            graphAsset.AddItem(itemAsset);

            try
            {
                // Act & Assert
                Assert.DoesNotThrow(() => undoSystem.OnUndoRedoPerformed(true));
            }
            finally
            {
                if (nodeAsset != null) ScriptableObject.DestroyImmediate(nodeAsset);
                if (edgeAsset != null) ScriptableObject.DestroyImmediate(edgeAsset);
                if (itemAsset != null) ScriptableObject.DestroyImmediate(itemAsset);
            }
        }
    }
}