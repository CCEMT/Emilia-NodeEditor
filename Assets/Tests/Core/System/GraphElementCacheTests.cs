using NUnit.Framework;
using UnityEngine;

namespace Emilia.Node.Editor.Tests
{
    [TestFixture]
    public class GraphElementCacheTests
    {
        private GraphElementCache cache;
        private TestEditorNodeView nodeView;
        private TestEditorEdgeView edgeView;
        private TestEditorItemView itemView;
        private TestEditorPortView portView;

        [SetUp]
        public void SetUp()
        {
            cache = new GraphElementCache();

            // 创建测试视图对象
            nodeView = new TestEditorNodeView();
            nodeView.asset = ScriptableObject.CreateInstance<TestNodeAsset>();
            nodeView.asset.id = "node1";

            edgeView = new TestEditorEdgeView();
            edgeView.asset = ScriptableObject.CreateInstance<TestEdgeAsset>();
            edgeView.asset.id = "edge1";

            itemView = new TestEditorItemView();
            itemView.asset = ScriptableObject.CreateInstance<TestItemAsset>();
            itemView.asset.id = "item1";

            portView = new TestEditorPortView();
            portView.nodeId = "node1";
            portView.portId = "port1";
        }

        [TearDown]
        public void TearDown()
        {
            cache?.Clear();

            if (nodeView?.asset != null) ScriptableObject.DestroyImmediate(nodeView.asset);
            if (edgeView?.asset != null) ScriptableObject.DestroyImmediate(edgeView.asset);
            if (itemView?.asset != null) ScriptableObject.DestroyImmediate(itemView.asset);
        }

        [Test]
        public void SetNodeViewCache_ValidNodeView_CachesCorrectly()
        {
            // Act
            cache.SetNodeViewCache("node1", nodeView);

            // Assert
            Assert.AreEqual(nodeView, cache.nodeViewById["node1"]);
        }

        [Test]
        public void SetEdgeViewCache_ValidEdgeView_CachesCorrectly()
        {
            // Act
            cache.SetEdgeViewCache("edge1", edgeView);

            // Assert
            Assert.AreEqual(edgeView, cache.edgeViewById["edge1"]);
        }

        [Test]
        public void SetItemViewCache_ValidItemView_CachesCorrectly()
        {
            // Act
            cache.SetItemViewCache("item1", itemView);

            // Assert
            Assert.AreEqual(itemView, cache.itemViewById["item1"]);
        }

        [Test]
        public void RemoveNodeViewCache_ExistingNodeView_RemovesCorrectly()
        {
            // Arrange
            cache.SetNodeViewCache("node1", nodeView);

            // Act
            cache.RemoveNodeViewCache("node1");

            // Assert
            Assert.IsFalse(cache.nodeViewById.ContainsKey("node1"));
        }

        [Test]
        public void RemoveEdgeViewCache_ExistingEdgeView_RemovesCorrectly()
        {
            // Arrange
            cache.SetEdgeViewCache("edge1", edgeView);

            // Act
            cache.RemoveEdgeViewCache("edge1");

            // Assert
            Assert.IsFalse(cache.edgeViewById.ContainsKey("edge1"));
        }

        [Test]
        public void RemoveItemViewCache_ExistingItemView_RemovesCorrectly()
        {
            // Arrange
            cache.SetItemViewCache("item1", itemView);

            // Act
            cache.RemoveItemViewCache("item1");

            // Assert
            Assert.IsFalse(cache.itemViewById.ContainsKey("item1"));
        }


        [Test]
        public void Clear_WithCachedItems_ClearsAllCaches()
        {
            // Arrange
            cache.SetNodeViewCache("node1",nodeView);
            cache.SetEdgeViewCache("edge1",edgeView);
            cache.SetItemViewCache("item1",itemView);

            // Act
            cache.Clear();

            // Assert
            Assert.AreEqual(0, cache.nodeViewById.Count);
            Assert.AreEqual(0, cache.edgeViewById.Count);
            Assert.AreEqual(0, cache.itemViewById.Count);
        }

        [Test]
        public void SetNodeViewCache_NullNodeView_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => cache.SetNodeViewCache(null,null));
        }

        [Test]
        public void SetNodeViewCache_NodeViewWithNullAsset_DoesNotThrow()
        {
            // Arrange
            var nodeViewWithNullAsset = new TestEditorNodeView();
            nodeViewWithNullAsset.asset = null;

            // Act & Assert
            Assert.DoesNotThrow(() => cache.SetNodeViewCache("node1",nodeViewWithNullAsset));
        }

        [Test]
        public void SetNodeViewCache_NodeViewWithNullId_DoesNotThrow()
        {
            // Arrange
            var nodeViewWithNullId = new TestEditorNodeView();
            nodeViewWithNullId.asset = ScriptableObject.CreateInstance<TestNodeAsset>();

            // Act & Assert
            Assert.DoesNotThrow(() => cache.SetNodeViewCache(null,nodeViewWithNullId));

            // Cleanup
            if (nodeViewWithNullId.asset != null) ScriptableObject.DestroyImmediate(nodeViewWithNullId.asset);
        }
    }
}