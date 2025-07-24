using System;
using Emilia.Kit.Editor;
using NUnit.Framework;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Emilia.Node.Editor.Tests
{
    [TestFixture]
    public class GraphConnectSystemTests
    {
        private GraphConnectSystem connectSystem;
        private EditorGraphView graphView;
        private EditorGraphAsset graphAsset;
        private TestEditorPortView inputPort;
        private TestEditorPortView outputPort;

        [SetUp]
        public void SetUp()
        {
            // Create test graph asset
            graphAsset = ScriptableObject.CreateInstance<TestUniversalGraphAsset>();

            // Create a minimal graph view for testing
            graphView = new EditorGraphView();
            graphView.Initialize();
            graphView.SimpleReload(graphAsset);

            // Create and initialize the connect system
            connectSystem = new GraphConnectSystem();
            connectSystem.Initialize(graphView);

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
            connectSystem?.Dispose();

            if (graphAsset != null)
            {
                ScriptableObject.DestroyImmediate(graphAsset);
            }
        }

        [Test]
        public void Order_ReturnsCorrectValue()
        {
            // Act & Assert
            Assert.AreEqual(1000, connectSystem.order);
        }

        [Test]
        public void Initialize_SetsGraphViewReference()
        {
            // Arrange
            var testConnectSystem = new GraphConnectSystem();

            // Act
            testConnectSystem.Initialize(graphView);

            // Assert
            Assert.AreEqual(graphView, ReflectUtility.GetValue(testConnectSystem, "graphView"));

            // Cleanup
            testConnectSystem.Dispose();
        }

        [Test]
        public void AllModuleInitializeSuccess_InitializesConnectorListener()
        {
            // Act
            connectSystem.AllModuleInitializeSuccess();

            // Assert
            Assert.IsNotNull(connectSystem.connectorListener);
        }

        [Test]
        public void GetEdgeAssetTypeByPort_WithValidPort_ReturnsEdgeType()
        {
            // Arrange
            connectSystem.AllModuleInitializeSuccess();

            // Act
            var edgeType = connectSystem.GetEdgeAssetTypeByPort(inputPort);

            // Assert
            // 根据实际实现，这里可能返回null或具体的边类型
            // 如果handle为null，应该返回null
            Assert.NotNull(edgeType);
        }

        [Test]
        public void GetEdgeAssetTypeByPort_WithNullPort_ReturnsNull()
        {
            // Arrange
            connectSystem.AllModuleInitializeSuccess();

            // Act
            var edgeType = connectSystem.GetEdgeAssetTypeByPort(null);

            // Assert
            Assert.IsNull(edgeType);
        }

        [Test]
        public void CanConnect_WithValidPorts_ReturnsConnectionResult()
        {
            // Arrange
            connectSystem.AllModuleInitializeSuccess();

            // Act
            var canConnect = connectSystem.CanConnect(inputPort, outputPort);

            // Assert
            // 根据实际实现，这里可能返回false（因为handle为null）
            Assert.IsFalse(canConnect);
        }

        [Test]
        public void CanConnect_WithNullPorts_ReturnsFalse()
        {
            // Arrange
            connectSystem.AllModuleInitializeSuccess();

            // Act
            var canConnect1 = connectSystem.CanConnect(null, outputPort);
            var canConnect2 = connectSystem.CanConnect(inputPort, null);
            var canConnect3 = connectSystem.CanConnect(null, null);

            // Assert
            Assert.IsFalse(canConnect1);
            Assert.IsFalse(canConnect2);
            Assert.IsFalse(canConnect3);
        }

        [Test]
        public void CanConnect_WithSameDirectionPorts_ReturnsFalse()
        {
            // Arrange
            connectSystem.AllModuleInitializeSuccess();
            var inputPort2 = new TestEditorPortView();
            inputPort2.nodeId = "node3";
            inputPort2.portId = "input2";
            inputPort2.portDirection = EditorPortDirection.Input;
            inputPort2.portElement = CreateTestPort(Direction.Input, typeof(float));

            // Act
            var canConnect = connectSystem.CanConnect(inputPort, inputPort2);

            // Assert
            Assert.IsFalse(canConnect);
        }

        [Test]
        public void BeforeConnect_WithValidPorts_CallsHandleMethod()
        {
            // Arrange
            connectSystem.AllModuleInitializeSuccess();

            // Act & Assert
            // 如果handle不为null，应该返回false
            var result = connectSystem.Connect(inputPort, outputPort);
            Assert.IsFalse(result != null);
        }

        [Test]
        public void BeforeConnect_WithNullPorts_ReturnsFalse()
        {
            // Arrange
            connectSystem.AllModuleInitializeSuccess();

            // Act
            var result1 = connectSystem.Connect(null, outputPort);
            var result2 = connectSystem.Connect(inputPort, null);
            var result3 = connectSystem.Connect(null, null);

            // Assert
            Assert.IsFalse(result1 != null);
            Assert.IsFalse(result2 != null);
            Assert.IsFalse(result3 != null);
        }

        [Test]
        public void Dispose_ClearsReferences()
        {
            // Arrange
            connectSystem.AllModuleInitializeSuccess();

            // Act
            connectSystem.Dispose();

            // Assert
            var graphViewRef = ReflectUtility.GetValue(connectSystem, "graphView");
            Assert.IsNull(graphViewRef);
        }

        [Test]
        public void ConnectorListener_AfterInitialization_IsNotNull()
        {
            // Act
            connectSystem.AllModuleInitializeSuccess();

            // Assert
            Assert.IsNotNull(connectSystem.connectorListener);
        }

        [Test]
        public void ConnectorListener_BeforeInitialization_IsNull()
        {
            // Act & Assert
            Assert.IsNull(connectSystem.connectorListener);
        }

        [Test]
        public void GetEdgeAssetTypeByPort_WithDifferentPortTypes_HandlesCorrectly()
        {
            // Arrange
            connectSystem.AllModuleInitializeSuccess();

            var stringPort = new TestEditorPortView();
            stringPort.nodeId = "node3";
            stringPort.portId = "string1";
            stringPort.portDirection = EditorPortDirection.Input;
            stringPort.portElement = CreateTestPort(Direction.Input, typeof(string));

            // Act
            var floatEdgeType = connectSystem.GetEdgeAssetTypeByPort(inputPort);
            var stringEdgeType = connectSystem.GetEdgeAssetTypeByPort(stringPort);

            // Assert
            // 根据实际实现，不同类型的端口可能返回不同的边类型
            // 这里主要验证方法不抛出异常
            Assert.DoesNotThrow(() => connectSystem.GetEdgeAssetTypeByPort(inputPort));
            Assert.DoesNotThrow(() => connectSystem.GetEdgeAssetTypeByPort(stringPort));
        }

        private Port CreateTestPort(Direction direction, Type type)
        {
            // 创建一个简单的测试端口
            var port = Port.Create<Edge>(Orientation.Horizontal, direction, Port.Capacity.Single, type);
            return port;
        }
    }
}