using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Emilia.Node.Editor;
using Emilia.Kit.Editor;

namespace Emilia.Node.Editor.Tests
{
    [TestFixture]
    public class OperateMenuActionUtilityTests
    {
        [Test]
        public void Actions_IsNotNull()
        {
            // Act & Assert
            Assert.IsNotNull(OperateMenuActionUtility.actions);
        }
        
        [Test]
        public void ActionMap_IsNotNull()
        {
            // Act & Assert
            Assert.IsNotNull(OperateMenuActionUtility.actionMap);
        }
        
        [Test]
        public void Actions_ContainsBuiltInActions()
        {
            // Act
            var actions = OperateMenuActionUtility.actions;
            
            // Assert
            Assert.IsTrue(actions.Count > 0, "应该包含至少一个操作");
        }
        
        [Test]
        public void ActionMap_ContainsBuiltInActions()
        {
            // Act
            var actionMap = OperateMenuActionUtility.actionMap;
            
            // Assert
            Assert.IsTrue(actionMap.Count > 0, "操作映射应该包含至少一个操作");
        }
        
        [Test]
        public void Actions_AndActionMap_HaveSameCount()
        {
            // Act
            var actions = OperateMenuActionUtility.actions;
            var actionMap = OperateMenuActionUtility.actionMap;
            
            // Assert
            Assert.AreEqual(actions.Count, actionMap.Count, "操作列表和操作映射应该有相同的数量");
        }
        
        [Test]
        public void Actions_AllHaveValidAction()
        {
            // Act
            var actions = OperateMenuActionUtility.actions;
            
            // Assert
            foreach (var actionInfo in actions)
            {
                Assert.IsNotNull(actionInfo.action, "所有操作信息都应该有有效的操作实例");
                Assert.IsNotNull(actionInfo.name, "所有操作信息都应该有名称");
            }
        }
        
        [Test]
        public void ActionMap_AllKeysAreActionTypes()
        {
            // Act
            var actionMap = OperateMenuActionUtility.actionMap;
            
            // Assert
            foreach (var kvp in actionMap)
            {
                Assert.IsTrue(typeof(IOperateMenuAction).IsAssignableFrom(kvp.Key), 
                    $"映射中的键 {kvp.Key} 应该是 IOperateMenuAction 的实现");
                Assert.IsNotNull(kvp.Value, "映射中的值不应该为null");
                Assert.IsInstanceOf(kvp.Key, kvp.Value.action, 
                    $"操作实例应该是键类型 {kvp.Key} 的实例");
            }
        }
        
        [Test]
        public void Actions_AllHaveValidAttributes()
        {
            // Act
            var actions = OperateMenuActionUtility.actions;
            
            // Assert
            foreach (var actionInfo in actions)
            {
                Assert.IsNotNull(actionInfo.name, "操作名称不应该为null");
                Assert.IsNotEmpty(actionInfo.name, "操作名称不应该为空");
                // 类别可以为null或空，但名称不能为null
                // 优先级可以是任何整数值
                // 标签可以为null或空
            }
        }
        
        [Test]
        public void GetActionGeneric_ExistingType_ReturnsCorrectAction()
        {
            // Arrange
            var actionMap = OperateMenuActionUtility.actionMap;
            if (actionMap.Count == 0)
            {
                Assert.Inconclusive("没有可用的操作来测试");
                return;
            }
            
            // Get first action type
            var firstActionType = actionMap.Keys.First();
            var expectedAction = actionMap[firstActionType];
            
            // Act - use reflection to call generic method
            var method = typeof(OperateMenuActionUtility).GetMethod("GetAction", new Type[0]);
            var genericMethod = method.MakeGenericMethod(firstActionType);
            var result = (OperateMenuActionInfo)genericMethod.Invoke(null, null);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedAction, result);
        }
        
        [Test]
        public void GetActionGeneric_NonExistingType_ThrowsException()
        {
            // Act & Assert - using reflection to test generic method with non-existing type
            var method = typeof(OperateMenuActionUtility).GetMethod("GetAction", new Type[0]);
            var genericMethod = method.MakeGenericMethod(typeof(TestNonExistingAction));
            
            Assert.Throws<System.Reflection.TargetInvocationException>(() => 
                genericMethod.Invoke(null, null));
        }
        
        [Test]
        public void Actions_AreConsistentWithActionMap()
        {
            // Act
            var actions = OperateMenuActionUtility.actions;
            var actionMap = OperateMenuActionUtility.actionMap;
            
            // Assert
            foreach (var actionInfo in actions)
            {
                Type actionType = actionInfo.action.GetType();
                Assert.IsTrue(actionMap.ContainsKey(actionType), 
                    $"操作映射应该包含类型 {actionType}");
                Assert.AreEqual(actionInfo, actionMap[actionType], 
                    $"操作列表和映射中的 {actionType} 应该是同一个实例");
            }
        }
        
        [Test]
        public void Actions_HaveUniqueTypes()
        {
            // Act
            var actions = OperateMenuActionUtility.actions;
            var actionTypes = actions.Select(a => a.action.GetType()).ToList();
            
            // Assert
            var uniqueTypes = actionTypes.Distinct().ToList();
            Assert.AreEqual(actionTypes.Count, uniqueTypes.Count, 
                "操作列表不应该包含重复的操作类型");
        }
        
        [Test]
        public void Actions_AllImplementIOperateMenuAction()
        {
            // Act
            var actions = OperateMenuActionUtility.actions;
            
            // Assert
            foreach (var actionInfo in actions)
            {
                Assert.IsInstanceOf<IOperateMenuAction>(actionInfo.action, 
                    "所有操作都应该实现 IOperateMenuAction 接口");
            }
        }
    }
    
    // Test classes for testing
    public class TestNonExistingAction : IOperateMenuAction
    {
        // 这个类不会被自动发现，因为它没有ActionAttribute
        public bool isOn { get; }
        public OperateMenuActionValidity GetValidity(OperateMenuContext context) => throw new NotImplementedException();

        public void Execute(OperateMenuActionContext context)
        {
            throw new NotImplementedException();
        }
    }
} 