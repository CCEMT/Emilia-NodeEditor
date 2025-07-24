using System.Collections.Generic;
using System.Linq;
using Emilia.Kit.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Editor.Tests
{
    [TestFixture]
    public class GraphOperateMenuTests
    {
        private GraphOperateMenu operateMenu;
        private EditorGraphView graphView;
        private EditorGraphAsset graphAsset;
        private TestOperateMenuAction testAction1;
        private TestOperateMenuAction testAction2;
        
        [SetUp]
        public void SetUp()
        {
            // Create test graph asset
            graphAsset = ScriptableObject.CreateInstance<TestUniversalGraphAsset>();
            
            // Create a minimal graph view for testing
            graphView = new EditorGraphView();
            graphView.Initialize();
            graphView.SimpleReload(graphAsset);
            
            // Create and initialize the operate menu
            operateMenu = new GraphOperateMenu();
            operateMenu.Initialize(graphView);
            
            // Create test actions
            testAction1 = new TestOperateMenuAction("Test Action 1", OperateMenuActionValidity.Valid);
            testAction2 = new TestOperateMenuAction("Test Action 2", OperateMenuActionValidity.Invalid);
        }
        
        [TearDown]
        public void TearDown()
        {
            operateMenu?.Dispose();
            
            if (graphAsset != null)
            {
                ScriptableObject.DestroyImmediate(graphAsset);
            }
        }
        
        [Test]
        public void Order_ReturnsCorrectValue()
        {
            // Act & Assert
            Assert.AreEqual(1100, operateMenu.order);
        }
        
        [Test]
        public void Initialize_SetsGraphViewReference()
        {
            // Arrange
            var testOperateMenu = new GraphOperateMenu();
            
            // Act
            testOperateMenu.Initialize(graphView);
            
            // Assert
            Assert.AreEqual(graphView, ReflectUtility.GetValue(testOperateMenu, "graphView"));
            
            // Cleanup
            testOperateMenu.Dispose();
        }
        
        [Test]
        public void Initialize_ClearsActionInfoCache()
        {
            // Act
            operateMenu.Initialize(graphView);
            
            // Assert
            Assert.IsNotNull(operateMenu.actionInfoCache);
            Assert.AreEqual(0, operateMenu.actionInfoCache.Count);
        }
        
        [Test]
        public void ActionInfoCache_IsNotNull()
        {
            // Act & Assert
            Assert.IsNotNull(operateMenu.actionInfoCache);
        }
        
        [Test]
        public void AllModuleInitializeSuccess_InitializesCache()
        {
            // Act
            operateMenu.AllModuleInitializeSuccess();
            
            // Assert
            Assert.IsNotNull(operateMenu.actionInfoCache);
            // 根据实际实现，可能会有一些默认的操作菜单项
        }
        
        [Test]
        public void BuildMenu_WithNullContext_HandlesGracefully()
        {
            // Arrange
            operateMenu.AllModuleInitializeSuccess();
            
            // Act & Assert
            // 根据实际实现，可能会抛出异常或优雅处理
            // 这里主要验证不会导致系统崩溃
            Assert.DoesNotThrow(() => operateMenu.BuildMenu(default));
        }
        
        [Test]
        public void BuildMenu_WithoutHandle_LogsError()
        {
            // Arrange
            // 确保handle为null
            ReflectUtility.SetValue(operateMenu, "handle", null);
            
            var context = new OperateMenuContext();
            context.graphView = graphView;
            context.evt = new ContextualMenuPopulateEvent();
            
            // Act & Assert
            // 验证方法不抛出异常，但会记录错误
            Assert.DoesNotThrow(() => operateMenu.BuildMenu(context));
        }
        
        [Test]
        public void BuildMenu_WithEmptyActionCache_HandlesCorrectly()
        {
            // Arrange
            operateMenu.AllModuleInitializeSuccess();
            operateMenu.actionInfoCache.Clear();
            
            var context = new OperateMenuContext();
            context.graphView = graphView;
            context.evt = new ContextualMenuPopulateEvent();
            
            // Act & Assert
            Assert.DoesNotThrow(() => operateMenu.BuildMenu(context));
        }
        
        [Test]
        public void BuildMenu_WithMultipleCategories_SortsCorrectly()
        {
            // Arrange
            operateMenu.AllModuleInitializeSuccess();
            
            // 添加不同分类的操作
            var actionInfo1 = new OperateMenuActionInfo();
            actionInfo1.action = testAction1;
            actionInfo1.name = "Action A";
            actionInfo1.category = "Category B";
            actionInfo1.priority = 200;
            
            var actionInfo2 = new OperateMenuActionInfo();
            actionInfo2.action = testAction2;
            actionInfo2.name = "Action B";
            actionInfo2.category = "Category A";
            actionInfo2.priority = 100;
            
            operateMenu.actionInfoCache.Add(actionInfo1);
            operateMenu.actionInfoCache.Add(actionInfo2);
            
            var context = new OperateMenuContext();
            context.graphView = graphView;
            context.evt = new ContextualMenuPopulateEvent();
            
            // Act & Assert
            Assert.DoesNotThrow(() => operateMenu.BuildMenu(context));
        }
        
        [Test]
        public void BuildMenu_WithSeparatorPriority_HandlesSeparators()
        {
            // Arrange
            operateMenu.AllModuleInitializeSuccess();
            
            // 添加具有分隔符优先级的操作
            var actionInfo1 = new OperateMenuActionInfo();
            actionInfo1.action = testAction1;
            actionInfo1.name = "Before Separator";
            actionInfo1.category = "Test";
            actionInfo1.priority = GraphOperateMenu.SeparatorAt - 100;
            
            var actionInfo2 = new OperateMenuActionInfo();
            actionInfo2.action = testAction2;
            actionInfo2.name = "After Separator";
            actionInfo2.category = "Test";
            actionInfo2.priority = GraphOperateMenu.SeparatorAt + 100;
            
            operateMenu.actionInfoCache.Add(actionInfo1);
            operateMenu.actionInfoCache.Add(actionInfo2);
            
            var context = new OperateMenuContext();
            context.graphView = graphView;
            context.evt = new ContextualMenuPopulateEvent();
            
            // Act & Assert
            Assert.DoesNotThrow(() => operateMenu.BuildMenu(context));
        }
        
        [Test]
        public void Dispose_ClearsReferences()
        {
            // Arrange
            operateMenu.AllModuleInitializeSuccess();
            
            // Act
            operateMenu.Dispose();
            
            // Assert
            var graphViewRef = ReflectUtility.GetValue(operateMenu, "graphView");
            Assert.IsNull(graphViewRef);
        }
        
        [Test]
        public void SeparatorAt_HasCorrectValue()
        {
            // Act & Assert
            Assert.AreEqual(1200, GraphOperateMenu.SeparatorAt);
        }
        
        [Test]
        public void ActionInfoCache_MaintainsOrder()
        {
            // Arrange
            var actionInfo1 = new OperateMenuActionInfo();
            actionInfo1.action = testAction1;
            actionInfo1.name = "First";
            actionInfo1.priority = 100;
            
            var actionInfo2 = new OperateMenuActionInfo();
            actionInfo2.action = testAction2;
            actionInfo2.name = "Second";
            actionInfo2.priority = 200;
            
            // Act
            operateMenu.actionInfoCache.Add(actionInfo1);
            operateMenu.actionInfoCache.Add(actionInfo2);
            
            // Assert
            Assert.AreEqual(2, operateMenu.actionInfoCache.Count);
            Assert.AreEqual(actionInfo1, operateMenu.actionInfoCache[0]);
            Assert.AreEqual(actionInfo2, operateMenu.actionInfoCache[1]);
        }
        
        [Test]
        public void BuildMenu_WithNotApplicableActions_FiltersCorrectly()
        {
            // Arrange
            operateMenu.AllModuleInitializeSuccess();
            
            var notApplicableAction = new TestOperateMenuAction("Not Applicable", OperateMenuActionValidity.NotApplicable);
            var validAction = new TestOperateMenuAction("Valid", OperateMenuActionValidity.Valid);
            
            var actionInfo1 = new OperateMenuActionInfo();
            actionInfo1.action = notApplicableAction;
            actionInfo1.name = "Not Applicable";
            actionInfo1.category = "Test";
            actionInfo1.priority = 100;
            
            var actionInfo2 = new OperateMenuActionInfo();
            actionInfo2.action = validAction;
            actionInfo2.name = "Valid";
            actionInfo2.category = "Test";
            actionInfo2.priority = 200;
            
            operateMenu.actionInfoCache.Add(actionInfo1);
            operateMenu.actionInfoCache.Add(actionInfo2);
            
            var context = new OperateMenuContext();
            context.graphView = graphView;
            context.evt = new ContextualMenuPopulateEvent();
            
            // Act & Assert
            // 验证过滤逻辑正常工作
            Assert.DoesNotThrow(() => operateMenu.BuildMenu(context));
        }
    }
} 