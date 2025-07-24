using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor.Tests.Integration
{
    [TestFixture]
    public class NodeEditorWorkflowTests
    {
        private EditorGraphAsset graphAsset;
        private EditorGraphView graphView;
        private GraphNodeSystem nodeSystem;
        private GraphCopyPaste copyPasteSystem;
        private GraphUndo undoSystem;
        private GraphSave saveSystem;

        [SetUp]
        public void SetUp()
        {
            // 创建图形资产
            graphAsset = ScriptableObject.CreateInstance<TestUniversalGraphAsset>();

            // 创建图形视图
            graphView = new EditorGraphView();
            graphView.Initialize();
            graphView.SimpleReload(graphAsset);

            // 初始化各个系统
            nodeSystem = new GraphNodeSystem();
            nodeSystem.Initialize(graphView);

            copyPasteSystem = new GraphCopyPaste();
            copyPasteSystem.Initialize(graphView);

            undoSystem = new GraphUndo();
            undoSystem.Initialize(graphView);

            saveSystem = new GraphSave();
            saveSystem.Initialize(graphView);
        }

        [TearDown]
        public void TearDown()
        {
            // 清理系统
            nodeSystem?.Dispose();
            copyPasteSystem?.Dispose();
            undoSystem?.Dispose();
            saveSystem?.Dispose();

            // 清理资产
            if (graphAsset != null)
            {
                ScriptableObject.DestroyImmediate(graphAsset);
            }
        }

        [Test]
        public void CompleteWorkflow_CreateNodeAddToGraphAndValidate()
        {
            // Act 1: 创建节点
            EditorNodeAsset nodeAsset = nodeSystem.CreateNode(typeof(TestNodeAsset), new Vector2(100, 100));

            try
            {
                // Assert 1: 节点创建成功
                Assert.IsNotNull(nodeAsset);
                Assert.IsInstanceOf<TestNodeAsset>(nodeAsset);

                // Act 2: 添加到图形
                graphAsset.AddNode(nodeAsset);

                // Assert 2: 节点已添加到图形
                Assert.AreEqual(1, graphAsset.nodes.Count);
                Assert.Contains(nodeAsset, (List<EditorNodeAsset>) graphAsset.nodes);
                Assert.IsTrue(graphAsset.nodeMap.ContainsKey(nodeAsset.id));
                Assert.AreEqual(nodeAsset, graphAsset.nodeMap[nodeAsset.id]);
                Assert.AreEqual(graphAsset, nodeAsset.graphAsset);

                // Act 3: 验证节点属性
                Assert.AreEqual(100, nodeAsset.position.x);
                Assert.AreEqual(100, nodeAsset.position.y);
                Assert.IsNotNull(nodeAsset.id);
                Assert.IsNotEmpty(nodeAsset.id);

                // Act 4: 移除节点
                graphAsset.RemoveNode(nodeAsset);

                // Assert 4: 节点已移除
                Assert.AreEqual(0, graphAsset.nodes.Count);
                Assert.IsFalse(graphAsset.nodeMap.ContainsKey(nodeAsset.id));
                Assert.IsNull(nodeAsset.graphAsset);
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
        public void CompleteWorkflow_CreateMultipleNodesAndEdges()
        {
            EditorNodeAsset node1 = null;
            EditorNodeAsset node2 = null;
            EditorEdgeAsset edge = null;

            try
            {
                // Act 1: 创建两个节点
                node1 = nodeSystem.CreateNode(typeof(TestNodeAsset), new Vector2(100, 100));
                node2 = nodeSystem.CreateNode(typeof(TestNodeAsset), new Vector2(300, 100));

                // Assert 1: 节点创建成功
                Assert.IsNotNull(node1);
                Assert.IsNotNull(node2);
                Assert.AreNotEqual(node1.id, node2.id);

                // Act 2: 添加节点到图形
                graphAsset.AddNode(node1);
                graphAsset.AddNode(node2);

                // Assert 2: 节点已添加
                Assert.AreEqual(2, graphAsset.nodes.Count);

                // Act 3: 创建边
                edge = ScriptableObject.CreateInstance<TestEdgeAsset>();
                edge.id = Guid.NewGuid().ToString();
                edge.outputNodeId = node1.id;
                edge.inputNodeId = node2.id;
                edge.outputPortId = "output";
                edge.inputPortId = "input";

                // Act 4: 添加边到图形
                graphAsset.AddEdge(edge);

                // Assert 4: 边已添加
                Assert.AreEqual(1, graphAsset.edges.Count);
                Assert.Contains(edge, (List<EditorEdgeAsset>) graphAsset.edges);
                Assert.IsTrue(graphAsset.edgeMap.ContainsKey(edge.id));
                Assert.AreEqual(edge, graphAsset.edgeMap[edge.id]);

                // Act 5: 验证边连接
                Assert.AreEqual(node1.id, edge.outputNodeId);
                Assert.AreEqual(node2.id, edge.inputNodeId);

                // Act 6: 清理
                graphAsset.RemoveEdge(edge);
                graphAsset.RemoveNode(node1);
                graphAsset.RemoveNode(node2);

                // Assert 6: 清理成功
                Assert.AreEqual(0, graphAsset.edges.Count);
                Assert.AreEqual(0, graphAsset.nodes.Count);
            }
            finally
            {
                if (node1 != null) ScriptableObject.DestroyImmediate(node1);
                if (node2 != null) ScriptableObject.DestroyImmediate(node2);
                if (edge != null) ScriptableObject.DestroyImmediate(edge);
            }
        }

        [Test]
        public void CompleteWorkflow_NodeCopyPasteOperations()
        {
            EditorNodeAsset originalNode = null;

            try
            {
                // Act 1: 创建原始节点
                originalNode = nodeSystem.CreateNode(typeof(TestNodeAsset), new Vector2(100, 100));
                originalNode.userData = "test data";
                graphAsset.AddNode(originalNode);

                // Assert 1: 原始节点创建成功
                Assert.IsNotNull(originalNode);
                Assert.AreEqual("test data", originalNode.userData);

                // Act 2: 复制节点数据
                var copiedData = copyPasteSystem.CreateCopy(originalNode.userData);

                // Assert 2: 复制成功
                Assert.IsNotNull(copiedData);
                Assert.AreEqual("test data", copiedData);

                // Act 3: 创建新节点并设置复制的数据
                var newNode = nodeSystem.CreateNode(typeof(TestNodeAsset), new Vector2(200, 200));
                newNode.userData = copiedData;

                try
                {
                    // Assert 3: 新节点创建成功且数据正确
                    Assert.IsNotNull(newNode);
                    Assert.AreEqual(copiedData, newNode.userData);
                    Assert.AreNotEqual(originalNode.id, newNode.id);
                }
                finally
                {
                    if (newNode != null) ScriptableObject.DestroyImmediate(newNode);
                }
            }
            finally
            {
                if (originalNode != null)
                {
                    ScriptableObject.DestroyImmediate(originalNode);
                }
            }
        }

        [Test]
        public void CompleteWorkflow_UndoRedoOperations()
        {
            EditorNodeAsset nodeAsset = null;

            try
            {
                // Act 1: 创建节点并添加到图形
                nodeAsset = nodeSystem.CreateNode(typeof(TestNodeAsset), new Vector2(100, 100));
                graphAsset.AddNode(nodeAsset);

                // Assert 1: 节点已添加
                Assert.AreEqual(1, graphAsset.nodes.Count);

                // Act 2: 执行撤销重做操作
                undoSystem.OnUndoRedoPerformed(true);

                // Assert 2: 撤销重做操作不应该抛出异常
                Assert.DoesNotThrow(() => undoSystem.OnUndoRedoPerformed(true));

                // Act 3: 多次撤销重做
                for (int i = 0; i < 3; i++)
                {
                    undoSystem.OnUndoRedoPerformed(true);
                }

                // Assert 3: 应该正常完成
                Assert.Pass("撤销重做操作成功完成");
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
        public void CompleteWorkflow_SaveOperations()
        {
            EditorNodeAsset nodeAsset = null;

            try
            {
                // Act 1: 创建节点并添加到图形
                nodeAsset = nodeSystem.CreateNode(typeof(TestNodeAsset), new Vector2(100, 100));
                graphAsset.AddNode(nodeAsset);

                // Assert 1: 节点已添加
                Assert.AreEqual(1, graphAsset.nodes.Count);

                // Act 2: 标记为脏数据
                saveSystem.SetDirty();

                // Assert 2: 应该标记为脏数据
                Assert.IsTrue(saveSystem.dirty);

                // Act 3: 保存
                saveSystem.OnSave();

                // Assert 3: 保存后应该不再是脏数据
                Assert.IsFalse(saveSystem.dirty);

                // Act 4: 多次保存操作
                saveSystem.SetDirty();
                saveSystem.OnSave();
                saveSystem.SetDirty();
                saveSystem.OnSave();

                // Assert 4: 应该正常完成
                Assert.IsFalse(saveSystem.dirty);
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
        public void CompleteWorkflow_GraphAssetChildrenManagement()
        {
            EditorNodeAsset nodeAsset = null;
            EditorEdgeAsset edgeAsset = null;
            EditorItemAsset itemAsset = null;

            try
            {
                // Act 1: 创建各种资产
                nodeAsset = nodeSystem.CreateNode(typeof(TestNodeAsset), new Vector2(100, 100));
                edgeAsset = ScriptableObject.CreateInstance<TestEdgeAsset>();
                edgeAsset.id = Guid.NewGuid().ToString();
                itemAsset = ScriptableObject.CreateInstance<TestItemAsset>();
                itemAsset.id = Guid.NewGuid().ToString();

                // Act 2: 添加到图形
                graphAsset.AddNode(nodeAsset);
                graphAsset.AddEdge(edgeAsset);
                graphAsset.AddItem(itemAsset);

                // Assert 2: 所有资产已添加
                Assert.AreEqual(1, graphAsset.nodes.Count);
                Assert.AreEqual(1, graphAsset.edges.Count);
                Assert.AreEqual(1, graphAsset.items.Count);

                // Act 3: 获取子资产
                var children = graphAsset.GetChildren();

                // Assert 3: 子资产包含所有元素
                Assert.AreEqual(3, children.Count);
                Assert.Contains(nodeAsset, children);
                Assert.Contains(edgeAsset, children);
                Assert.Contains(itemAsset, children);

                // Act 4: 设置子资产
                var newChildren = new List<Object> {nodeAsset};
                graphAsset.SetChildren(newChildren);

                // Assert 4: 只有节点保留
                Assert.AreEqual(1, graphAsset.nodes.Count);
                Assert.AreEqual(0, graphAsset.edges.Count);
                Assert.AreEqual(0, graphAsset.items.Count);
                Assert.Contains(nodeAsset, (List<EditorNodeAsset>) graphAsset.nodes);
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