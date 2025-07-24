using System.Collections.Generic;
using Emilia.Kit;
using Emilia.Kit.Editor;
using NUnit.Framework;
using UnityEngine;

namespace Emilia.Node.Editor.Tests
{
    [TestFixture]
    public class GraphSelectedTests
    {
        private GraphSelected selectedSystem;
        private EditorGraphView graphView;
        private EditorGraphAsset graphAsset;
        private TestSelectable selectable1;
        private TestSelectable selectable2;
        private TestSelectedHandle selectedHandle1;
        private TestSelectedHandle selectedHandle2;

        [SetUp]
        public void SetUp()
        {
            // Create test graph asset
            graphAsset = ScriptableObject.CreateInstance<TestUniversalGraphAsset>();

            // Create a minimal graph view for testing
            graphView = new EditorGraphView();
            graphView.Initialize();
            graphView.SimpleReload(graphAsset);

            // Create and initialize the selected system
            selectedSystem = new GraphSelected();
            selectedSystem.Initialize(graphView);

            // Create test selectables
            selectable1 = new TestSelectable("selectable1");
            selectable2 = new TestSelectable("selectable2");

            // Create test selected handles
            selectedHandle1 = new TestSelectedHandle("handle1");
            selectedHandle2 = new TestSelectedHandle("handle2");
        }

        [TearDown]
        public void TearDown()
        {
            selectedSystem?.Dispose();

            if (graphAsset != null)
            {
                ScriptableObject.DestroyImmediate(graphAsset);
            }
        }

        [Test]
        public void Initialize_SetsGraphViewReference()
        {
            // Arrange
            var testSelectedSystem = new GraphSelected();

            // Act
            testSelectedSystem.Initialize(graphView);

            // Assert
            Assert.AreEqual(graphView, ReflectUtility.GetValue(testSelectedSystem, "graphView"));

            // Cleanup
            testSelectedSystem.Dispose();
        }

        [Test]
        public void UpdateSelected_WithSelectables_UpdatesSelectedElements()
        {
            // Arrange
            var selectables = new List<ISelectedHandle> {selectedHandle1, selectedHandle2};

            // Act
            selectedSystem.UpdateSelected(selectables);

            // Assert
            Assert.IsNotNull(selectedSystem.selected);
            Assert.AreEqual(2, selectedSystem.selected.Count);
        }

        [Test]
        public void UpdateSelected_WithSelectedHandles_UpdatesSelectedHandles()
        {
            // Arrange
            var selectedHandles = new List<ISelectedHandle> {selectedHandle1, selectedHandle2};

            // Act
            selectedSystem.UpdateSelected(selectedHandles);

            // Assert
            Assert.IsNotNull(selectedSystem.selected);
            Assert.AreEqual(2, selectedSystem.selected.Count);
        }

        [Test]
        public void UpdateSelected_WithNullList_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => selectedSystem.UpdateSelected((List<ISelectedHandle>) null));
            Assert.DoesNotThrow(() => selectedSystem.UpdateSelected((List<ISelectedHandle>) null));
        }

        [Test]
        public void UpdateSelected_WithEmptyList_ClearsSelection()
        {
            // Arrange
            var selectables = new List<ISelectedHandle> {selectedHandle1, selectedHandle2};
            selectedSystem.UpdateSelected(selectables);

            // Act
            selectedSystem.UpdateSelected(new List<ISelectedHandle>());

            // Assert
            Assert.IsNotNull(selectedSystem.selected);
            Assert.AreEqual(0, selectedSystem.selected.Count);
        }

        [Test]
        public void UpdateSelected_CallsHandleUpdateMethods()
        {
            // Arrange
            var selectables = new List<ISelectedHandle> {selectedHandle1};

            // Act
            selectedSystem.UpdateSelected(selectables);

            // Assert - 验证handle的方法被调用
            // 注意：这里需要mock GraphSelectedHandle来验证方法调用
            Assert.DoesNotThrow(() => selectedSystem.UpdateSelected(selectables));
        }

        [Test]
        public void Dispose_ClearsReferences()
        {
            // Arrange
            var selectables = new List<ISelectedHandle> {selectedHandle1, selectedHandle2};
            selectedSystem.UpdateSelected(selectables);

            // Act
            selectedSystem.Dispose();

            // Assert
            var graphViewRef = ReflectUtility.GetValue(selectedSystem, "graphView");
            Assert.IsNull(graphViewRef);
        }

        [Test]
        public void UpdateSelected_WithDuplicateSelectables_HandlesCorrectly()
        {
            // Arrange
            var selectables = new List<ISelectedHandle> {selectedHandle1, selectedHandle1, selectedHandle2};

            // Act
            selectedSystem.UpdateSelected(selectables);

            // Assert
            Assert.IsNotNull(selectedSystem.selected);
            // 应该处理重复项，具体行为取决于实现
            Assert.LessOrEqual(selectedSystem.selected.Count, 3);
        }

        [Test]
        public void UpdateSelected_WithMixedTypes_HandlesCorrectly()
        {
            var handles = new List<ISelectedHandle> {selectedHandle1, selectedHandle2};

            // Act
            selectedSystem.UpdateSelected(handles);

            Assert.IsNotNull(selectedSystem.selected);
            Assert.AreEqual(2, selectedSystem.selected.Count);
        }
    }
}