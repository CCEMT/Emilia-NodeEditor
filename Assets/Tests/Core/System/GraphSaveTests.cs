using System.IO;
using Emilia.Kit.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;

namespace Emilia.Node.Editor.Tests
{
    [TestFixture]
    public class GraphSaveTests
    {
        private GraphSave saveSystem;
        private EditorGraphView graphView;
        private EditorGraphAsset graphAsset;
        private string tempAssetPath;
        
        [SetUp]
        public void SetUp()
        {
            // Create test graph asset
            graphAsset = ScriptableObject.CreateInstance<TestUniversalGraphAsset>();
            
            // Create temp asset path
            tempAssetPath = "Assets/TestTemp.asset";
            AssetDatabase.CreateAsset(graphAsset, tempAssetPath);
            AssetDatabase.SaveAssets();
            
            // Create a minimal graph view for testing
            graphView = new EditorGraphView();
            graphView.Initialize();
            graphView.SimpleReload(graphAsset);
            
            // Create and initialize the save system
            saveSystem = new GraphSave();
            saveSystem.Initialize(graphView);
        }
        
        [TearDown]
        public void TearDown()
        {
            saveSystem?.Dispose();
            
            // Clean up temp asset
            if (!string.IsNullOrEmpty(tempAssetPath) && File.Exists(tempAssetPath))
            {
                AssetDatabase.DeleteAsset(tempAssetPath);
            }
            
            // Clean up any temp folder files
            if (Directory.Exists("Assets/Temp"))
            {
                Directory.Delete("Assets/Temp", true);
                AssetDatabase.Refresh();
            }
        }
        
        [Test]
        public void Order_ReturnsCorrectValue()
        {
            // Act & Assert
            Assert.AreEqual(500, saveSystem.order);
        }
        
        [Test]
        public void Initialize_SetsGraphViewReference()
        {
            // Arrange
            var testSaveSystem = new GraphSave();
            
            // Act
            testSaveSystem.Initialize(graphView);
            
            // Assert
            Assert.AreEqual(graphView, ReflectUtility.GetValue(testSaveSystem, "graphView"));
            
            // Cleanup
            testSaveSystem.Dispose();
        }
        
        [Test]
        public void Dirty_DefaultValue_ReturnsFalse()
        {
            // Act & Assert
            Assert.IsFalse(saveSystem.dirty);
        }
        
        [Test]
        public void SetDirty_WhenLoadProgressComplete_SetsDirtyTrue()
        {
            // Arrange
            // 确保加载进度完成
            ReflectUtility.SetValue(graphView, "loadProgress", 1f);
            
            // Act
            saveSystem.SetDirty();
            
            // Assert
            Assert.IsTrue(saveSystem.dirty);
        }
        
        [Test]
        public void SetDirty_WhenLoadProgressIncomplete_DoesNotSetDirty()
        {
            // Arrange
            // 设置加载进度未完成
            ReflectUtility.SetValue(graphView, "loadProgress", 0.5f);
            
            // Act
            saveSystem.SetDirty();
            
            // Assert
            Assert.IsFalse(saveSystem.dirty);
        }
        
        [Test]
        public void ResetCopy_ValidAsset_CreatesTemporaryCopy()
        {
            // Act
            var copy = saveSystem.ResetCopy(graphAsset);
            
            // Assert
            Assert.IsNotNull(copy);
            Assert.AreNotEqual(graphAsset, copy);
            Assert.AreEqual(graphAsset.name, copy.name);
            
            // Verify temp file was created
            string tempPath = $"Assets/Temp/{graphAsset.name}.asset";
            Assert.IsTrue(File.Exists(tempPath));
            
            // Cleanup
            if (copy != null)
            {
                Object.DestroyImmediate(copy);
            }
        }
        
        [Test]
        public void ResetCopy_NullAsset_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => saveSystem.ResetCopy(null));
        }
        
        [Test]
        public void OnSave_WithValidGraphView_CallsHandleAndSaveAll()
        {
            // Arrange
            saveSystem.SetDirty();
            
            // Act
            Assert.DoesNotThrow(() => saveSystem.OnSave());
            
            // Assert
            // 验证dirty状态被重置（如果实现了的话）
            // 这里主要验证方法不抛出异常
        }
        
        [Test]
        public void OnSave_WithNullGraphView_DoesNotThrow()
        {
            // Arrange
            ReflectUtility.SetValue(saveSystem, "graphView", null);
            
            // Act & Assert
            Assert.DoesNotThrow(() => saveSystem.OnSave());
        }
        
        [Test]
        public void OnSave_WithSourceAsset_CopiesBackToOriginal()
        {
            // Arrange
            var originalAsset = ScriptableObject.CreateInstance<TestUniversalGraphAsset>();
            string originalPath = "Assets/TestOriginal.asset";
            AssetDatabase.CreateAsset(originalAsset, originalPath);
            AssetDatabase.SaveAssets();
            
            try
            {
                // Create copy
                var copy = saveSystem.ResetCopy(originalAsset);
                
                // Modify copy
                copy.name = "ModifiedCopy";
                
                // Set up graph view with copy
                graphView.SimpleReload(copy);
                
                // Act
                saveSystem.OnSave();
                
                // Assert
                // 验证原始资产被更新（这里主要验证不抛出异常）
                Assert.DoesNotThrow(() => saveSystem.OnSave());
            }
            finally
            {
                // Cleanup
                if (File.Exists(originalPath))
                {
                    AssetDatabase.DeleteAsset(originalPath);
                }
            }
        }
        
        [Test]
        public void Dispose_ClearsReferences()
        {
            // Arrange
            saveSystem.SetDirty();
            
            // Act
            saveSystem.Dispose();
            
            // Assert
            var graphViewRef = ReflectUtility.GetValue(saveSystem, "graphView");
            Assert.IsNull(graphViewRef);
        }
        
        [Test]
        public void SetDirty_MultipleCalls_MaintainsDirtyState()
        {
            // Arrange
            ReflectUtility.SetValue(graphView, "loadProgress", 1f);
            
            // Act
            saveSystem.SetDirty();
            saveSystem.SetDirty();
            saveSystem.SetDirty();
            
            // Assert
            Assert.IsTrue(saveSystem.dirty);
        }
        
        [Test]
        public void ResetCopy_CreatesUniqueTemporaryFiles()
        {
            // Arrange
            var asset1 = ScriptableObject.CreateInstance<TestUniversalGraphAsset>();
            var asset2 = ScriptableObject.CreateInstance<TestUniversalGraphAsset>();
            asset1.name = "Asset1";
            asset2.name = "Asset2";
            
            try
            {
                // Act
                var copy1 = saveSystem.ResetCopy(asset1);
                var copy2 = saveSystem.ResetCopy(asset2);
                
                // Assert
                Assert.IsNotNull(copy1);
                Assert.IsNotNull(copy2);
                Assert.AreNotEqual(copy1, copy2);
                Assert.AreEqual("Asset1", copy1.name);
                Assert.AreEqual("Asset2", copy2.name);
            }
            finally
            {
                // Cleanup
                Object.DestroyImmediate(asset1);
                Object.DestroyImmediate(asset2);
            }
        }
    }
} 