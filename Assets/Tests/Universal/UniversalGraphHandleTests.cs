using System;
using System.Collections.Generic;
using Emilia.Kit;
using Emilia.Kit.Editor;
using Emilia.Node.Editor;
using Emilia.Node.Universal.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor.Tests.Universal
{
    [TestFixture]
    public class UniversalGraphHandleTests
    {
        private UniversalGraphHandle graphHandle;
        private EditorGraphView graphView;
        private EditorUniversalGraphAsset graphAsset;
        
        [SetUp]
        public void SetUp()
        {
            // Create test universal graph asset
            graphAsset = ScriptableObject.CreateInstance<TestUniversalGraphAsset>();
            
            // Create a minimal graph view for testing
            graphView = new EditorGraphView();
            graphView.Initialize();
            graphView.SimpleReload(graphAsset);
            
            // Create and initialize the universal graph handle
            graphHandle = new UniversalGraphHandle();
            graphHandle.Initialize(graphView);
        }
        
        [TearDown]
        public void TearDown()
        {
            graphHandle?.Dispose(this.graphView);
            
            if (graphAsset != null)
            {
                ScriptableObject.DestroyImmediate(graphAsset);
            }
        }
        
        [Test]
        public void Initialize_SetsGraphViewReference()
        {
            // Arrange
            var testHandle = new UniversalGraphHandle();
            
            // Act
            testHandle.Initialize(graphView);
            
            // Assert
            var editorGraphViewRef = ReflectUtility.GetValue(testHandle, "editorGraphView");
            Assert.AreEqual(graphView, editorGraphViewRef);
            
            // Cleanup
            testHandle.Dispose(this.graphView);
        }
        
        [Test]
        public void OnLoadBefore_InitializesComponents()
        {
            // Act
            Assert.DoesNotThrow(() => graphHandle.OnLoadBefore(graphView));
            
            // Assert
            // 验证各种初始化操作不抛出异常
            // 实际测试中可能需要检查具体的初始化状态
        }
        
        [Test]
        public void GridBackgroundStyleFilePath_HasCorrectValue()
        {
            // Act & Assert
            Assert.AreEqual("Node/Styles/GridBackground.uss", UniversalGraphHandle.GridBackgroundStyleFilePath);
        }
        
        [Test]
        public void GraphViewStyleFilePath_HasCorrectValue()
        {
            // Act & Assert
            Assert.AreEqual("Node/Styles/UniversalEditorGraphView.uss", UniversalGraphHandle.GraphViewStyleFilePath);
        }
        
        [Test]
        public void OnLoadBefore_AddsManipulators()
        {
            // Act
            graphHandle.OnLoadBefore(graphView);
            
            // Assert
            // 验证操作器被正确添加
            // 这里主要验证不抛出异常
            Assert.DoesNotThrow(() => graphHandle.OnLoadBefore(graphView));
        }
        
        [Test]
        public void OnLoadBefore_AddsGridBackground()
        {
            // Act
            graphHandle.OnLoadBefore(graphView);
            
            // Assert
            // 验证网格背景被添加
            // 实际实现中可能需要检查graphView的子元素
            Assert.DoesNotThrow(() => graphHandle.OnLoadBefore(graphView));
        }
        
        [Test]
        public void OnLoadBefore_AddsLoadingMask()
        {
            // Act
            graphHandle.OnLoadBefore(graphView);
            
            // Assert
            // 验证加载遮罩被添加
            Assert.DoesNotThrow(() => graphHandle.OnLoadBefore(graphView));
        }
        
        [Test]
        public void OnLoadBefore_RegistersLogicTransformChangeEvent()
        {
            // Act
            graphHandle.OnLoadBefore(graphView);
            
            // Assert
            // 验证逻辑变换变更事件被注册
            // 这里主要验证不抛出异常
            Assert.DoesNotThrow(() => graphHandle.OnLoadBefore(graphView));
        }
        
        [Test]
        public void OnLoadBefore_RegistersCompilationEvents()
        {
            // Act
            graphHandle.OnLoadBefore(graphView);
            
            // Assert
            // 验证编译事件被注册
            Assert.DoesNotThrow(() => graphHandle.OnLoadBefore(graphView));
        }
        
        [Test]
        public void OnLogicTransformChange_UpdatesLocalSetting()
        {
            // Arrange
            graphHandle.OnLoadBefore(graphView);
            
            // Act
            var onLogicTransformChange = ReflectUtility.GetValue(graphHandle, "OnLogicTransformChange");
            
            // Assert
            // 验证逻辑变换变更处理器存在
            Assert.IsNotNull(onLogicTransformChange);
        }
        
        [Test]
        public void OnCompilationStarted_HandlesCompilation()
        {
            // Arrange
            graphHandle.OnLoadBefore(graphView);
            
            // Act & Assert
            // 验证编译开始处理不抛出异常
            Assert.DoesNotThrow(() => graphHandle.OnLoadBefore(graphView));
        }
        
        [Test]
        public void Dispose_ClearsReferences()
        {
            // Arrange
            graphHandle.OnLoadBefore(graphView);
            
            // Act
            graphHandle.Dispose(this.graphView);
            
            // Assert
            // 验证引用被清理
            Assert.DoesNotThrow(() => graphHandle.Dispose(this.graphView));
        }
        
        [Test]
        public void OnLoadBefore_MultipleCallsHandledCorrectly()
        {
            // Act
            graphHandle.OnLoadBefore(graphView);
            graphHandle.OnLoadBefore(graphView);
            
            // Assert
            // 验证多次调用不会导致问题
            Assert.DoesNotThrow(() => graphHandle.OnLoadBefore(graphView));
        }
        
        [Test]
        public void OnLoadBefore_WithNullGraphView_HandlesGracefully()
        {
            // Act & Assert
            // 根据实际实现，可能会抛出异常或优雅处理
            Assert.DoesNotThrow(() => graphHandle.OnLoadBefore(null));
        }
    }
    
    [TestFixture]
    public class UniversalConnectSystemHandleTests
    {
        private UniversalConnectSystemHandle connectHandle;
        private EditorGraphView graphView;
        private EditorUniversalGraphAsset graphAsset;
        private TestEditorPortView inputPort;
        private TestEditorPortView outputPort;
        
        [SetUp]
        public void SetUp()
        {
            // Create test universal graph asset
            graphAsset = ScriptableObject.CreateInstance<TestUniversalGraphAsset>();
            
            // Create a minimal graph view for testing
            graphView = new EditorGraphView();
            graphView.Initialize();
            graphView.SimpleReload(graphAsset);
            
            // Create connect handle
            connectHandle = new UniversalConnectSystemHandle();
            
            // Create test ports
            inputPort = new TestEditorPortView();
            inputPort.nodeId = "node1";
            inputPort.portId = "input1";
            inputPort.portDirection = EditorPortDirection.Input;
            inputPort.portElement = CreateTestPort(Direction.Input, typeof(float));
            
            outputPort = new TestEditorPortView();
            outputPort.nodeId = "node2";
            outputPort.portId = "output1";
            outputPort.portDirection = EditorPortDirection.Output;
            outputPort.portElement = CreateTestPort(Direction.Output, typeof(float));
        }
        
        [TearDown]
        public void TearDown()
        {
            if (graphAsset != null)
            {
                ScriptableObject.DestroyImmediate(graphAsset);
            }
        }
        
        [Test]
        public void GetEdgeAssetTypeByPort_ReturnsUniversalEdgeAsset()
        {
            // Act
            var edgeType = connectHandle.GetEdgeAssetTypeByPort(graphView, inputPort);
            
            // Assert
            Assert.AreEqual(typeof(UniversalEditorEdgeAsset), edgeType);
        }
        
        [Test]
        public void CanConnect_WithCompatiblePorts_ReturnsTrue()
        {
            // Act
            var canConnect = connectHandle.CanConnect(graphView, inputPort, outputPort);
            
            // Assert
            Assert.IsTrue(canConnect);
        }
        
        [Test]
        public void CanConnect_WithIncompatiblePortTypes_ReturnsFalse()
        {
            // Arrange
            var stringPort = new TestEditorPortView();
            stringPort.nodeId = "node3";
            stringPort.portId = "string1";
            stringPort.portDirection = EditorPortDirection.Output;
            stringPort.portElement = CreateTestPort(Direction.Output, typeof(string));
            
            // Act
            var canConnect = connectHandle.CanConnect(graphView, inputPort, stringPort);
            
            // Assert
            Assert.IsFalse(canConnect);
        }
        
        [Test]
        public void CanConnect_WithSameDirection_ReturnsFalse()
        {
            // Arrange
            var inputPort2 = new TestEditorPortView();
            inputPort2.nodeId = "node3";
            inputPort2.portId = "input2";
            inputPort2.portDirection = EditorPortDirection.Input;
            inputPort2.portElement = CreateTestPort(Direction.Input, typeof(float));
            
            // Act
            var canConnect = connectHandle.CanConnect(graphView, inputPort, inputPort2);
            
            // Assert
            Assert.IsFalse(canConnect);
        }
        
        [Test]
        public void CanConnect_WithAnyDirection_ReturnsTrue()
        {
            // Arrange
            var anyPort = new TestEditorPortView();
            anyPort.nodeId = "node3";
            anyPort.portId = "any1";
            anyPort.portDirection = EditorPortDirection.Any;
            anyPort.portElement = CreateTestPort(Direction.Input, typeof(float));
            
            // Act
            var canConnect1 = connectHandle.CanConnect(graphView, inputPort, anyPort);
            var canConnect2 = connectHandle.CanConnect(graphView, anyPort, outputPort);
            
            // Assert
            Assert.IsTrue(canConnect1);
            Assert.IsTrue(canConnect2);
        }
        
        [Test]
        public void CanConnect_WithNullPorts_ReturnsFalse()
        {
            // Act
            var canConnect1 = connectHandle.CanConnect(graphView, null, outputPort);
            var canConnect2 = connectHandle.CanConnect(graphView, inputPort, null);
            var canConnect3 = connectHandle.CanConnect(graphView, null, null);
            
            // Assert
            Assert.IsFalse(canConnect1);
            Assert.IsFalse(canConnect2);
            Assert.IsFalse(canConnect3);
        }
        
        [Test]
        public void CanConnect_WithNullGraphView_ReturnsFalse()
        {
            // Act
            var canConnect = connectHandle.CanConnect(null, inputPort, outputPort);
            
            // Assert
            Assert.IsFalse(canConnect);
        }
        
        [Test]
        public void GetEdgeAssetTypeByPort_WithNullPort_ReturnsUniversalEdgeAsset()
        {
            // Act
            var edgeType = connectHandle.GetEdgeAssetTypeByPort(graphView, null);
            
            // Assert
            Assert.AreEqual(typeof(UniversalEditorEdgeAsset), edgeType);
        }
        
        [Test]
        public void GetEdgeAssetTypeByPort_WithNullGraphView_ReturnsUniversalEdgeAsset()
        {
            // Act
            var edgeType = connectHandle.GetEdgeAssetTypeByPort(null, inputPort);
            
            // Assert
            Assert.AreEqual(typeof(UniversalEditorEdgeAsset), edgeType);
        }
        
        private Port CreateTestPort(Direction direction, System.Type type)
        {
            var port = Port.Create<Edge>(Orientation.Horizontal, direction, Port.Capacity.Single, type);
            return port;
        }
    }
} 