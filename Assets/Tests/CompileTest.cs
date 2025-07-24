using Emilia.Kit.Editor;
using NUnit.Framework;
using Emilia.Node.Editor.Tests;
using UnityEngine;

namespace Emilia.Node.Editor.Tests
{
    [TestFixture]
    public class CompileTest
    {
        [Test]
        public void AllTestClassesCompileSuccessfully()
        {
            // 测试所有测试辅助类是否可以正常实例化
            var testGraphAsset = ScriptableObject.CreateInstance<TestGraphAsset>();
            var testUniversalGraphAsset = ScriptableObject.CreateInstance<TestUniversalGraphAsset>();
            var testNodeAsset = ScriptableObject.CreateInstance<TestNodeAsset>();
            var testEdgeAsset = ScriptableObject.CreateInstance<TestEdgeAsset>();
            var testItemAsset = ScriptableObject.CreateInstance<TestItemAsset>();
            
            // 测试接口实现
            var testSelectable = new TestSelectable("test");
            var testSelectedHandle = new TestSelectedHandle("test");
            var testOperateMenuAction = new TestOperateMenuAction("test", OperateMenuActionValidity.Valid);
            
            // 测试视图类
            var testNodeView = new TestEditorNodeView();
            var testEdgeView = new TestEditorEdgeView();
            var testItemView = new TestEditorItemView();
            var testPortView = new TestEditorPortView();
            
            // 验证基本功能
            Assert.IsNotNull(testGraphAsset);
            Assert.IsNotNull(testUniversalGraphAsset);
            Assert.IsNotNull(testNodeAsset);
            Assert.IsNotNull(testEdgeAsset);
            Assert.IsNotNull(testItemAsset);
            
            Assert.IsNotNull(testSelectable);
            Assert.IsNotNull(testSelectedHandle);
            Assert.IsNotNull(testOperateMenuAction);
            
            Assert.IsNotNull(testNodeView);
            Assert.IsNotNull(testEdgeView);
            Assert.IsNotNull(testItemView);
            Assert.IsNotNull(testPortView);
            
            // 测试资产收集功能
            var portInfos = testNodeAsset.CollectAsset();
            Assert.IsNotNull(portInfos);
            
            // 测试选择功能
            Assert.IsFalse(testSelectable.IsSelected());
            testSelectable.Select();
            Assert.IsTrue(testSelectable.IsSelected());
            
            // 测试工具类
            var createdNodeAsset = TestUtility.CreateTestNodeAsset();
            Assert.IsNotNull(createdNodeAsset);
            Assert.IsNotNull(createdNodeAsset.id);
            
            // 清理资源
            TestUtility.CleanupAssets(
                testGraphAsset, 
                testUniversalGraphAsset, 
                testNodeAsset, 
                testEdgeAsset, 
                testItemAsset, 
                createdNodeAsset
            );
            
            // 如果能执行到这里，说明编译成功
            Assert.Pass("所有测试类编译成功！");
        }
    }
} 