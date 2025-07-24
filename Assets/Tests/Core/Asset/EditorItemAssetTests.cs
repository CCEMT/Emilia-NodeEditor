using System.Collections.Generic;
using Emilia.Kit;
using Emilia.Node.Attributes;
using NUnit.Framework;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor.Tests
{
    [TestFixture]
    public class EditorItemAssetTests
    {
        private EditorItemAsset itemAsset;

        [SetUp]
        public void SetUp()
        {
            itemAsset = ScriptableObject.CreateInstance<TestItemAsset>();
        }

        [TearDown]
        public void TearDown()
        {
            if (itemAsset != null)
            {
                ScriptableObject.DestroyImmediate(itemAsset);
            }
        }

        [Test]
        public void Id_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            string testId = "test-item-id";

            // Act
            itemAsset.id = testId;

            // Assert
            Assert.AreEqual(testId, itemAsset.id);
        }

        [Test]
        public void Position_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            Rect testPosition = new Rect(50, 100, 200, 150);

            // Act
            itemAsset.position = testPosition;

            // Assert
            Assert.AreEqual(testPosition, itemAsset.position);
        }

        [Test]
        public void GraphAsset_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            EditorGraphAsset graphAsset = ScriptableObject.CreateInstance<TestGraphAsset>();

            try
            {
                // Act
                itemAsset.graphAsset = graphAsset;

                // Assert
                Assert.AreEqual(graphAsset, itemAsset.graphAsset);
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
            Assert.AreEqual("Test Item", itemAsset.title);
        }

        [Test]
        public void PropertyTree_IsNotNullAfterOnEnable()
        {
            // Act - OnEnable is called automatically when creating ScriptableObject

            // Assert
            Assert.IsNotNull(itemAsset.propertyTree);
        }

        [Test]
        public void GetChildren_ReturnsNull()
        {
            // Act
            var children = itemAsset.GetChildren();

            // Assert
            Assert.IsNull(children);
        }

        [Test]
        public void SetChildren_DoesNotThrow()
        {
            // Arrange
            var children = new List<Object>();

            // Act & Assert
            Assert.DoesNotThrow(() => itemAsset.SetChildren(children));
        }
    }

    // Test implementation classes
    public class TestItemAsset : EditorItemAsset
    {
        public override string title => "Test Item";
    }

    [EditorItem(typeof(TestItemAsset))]
    public class TestItemView : GraphElement, IEditorItemView
    {
        public EditorItemAsset asset { get; }
        public GraphElement element => this;
        public EditorGraphView graphView { get; }
        public bool isSelected { get; protected set; }
        public void Initialize(EditorGraphView graphView, EditorItemAsset asset) { }

        public void Delete() { }
        public void RemoveView() { }
        public ICopyPastePack GetPack() => null;

        public bool Validate() => true;

        public bool IsSelected() => isSelected;

        public void Select()
        {
            isSelected = true;
        }

        public void Unselect()
        {
            isSelected = false;
        }

        public IEnumerable<Object> GetSelectedObjects() => null;

        public void SetPositionNoUndo(Rect position) { }
        public void OnValueChanged(bool isSilent = false) { }
        public void Dispose() { }
    }
}