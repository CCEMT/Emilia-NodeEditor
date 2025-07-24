using System;
using System.Collections.Generic;
using Emilia.Kit;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor.Tests
{
    // 测试用的选择接口实现
    public class TestSelectable : ISelectable
    {
        public string name { get; private set; }
        public bool selected { get; set; }

        public TestSelectable(string name)
        {
            this.name = name;
        }

        public bool IsSelected() => selected;
        public void Select() => selected = true;
        public void Unselect() => selected = false;

        // ISelectable 接口要求的方法
        public void UnselectVisualElement() => selected = false;
        public bool IsSelectedVisualElement() => selected;
        public bool IsSelectable() => selected;

        public bool HitTest(Vector2 localPoint) => true;

        public bool Overlaps(Rect rectangle) => true;

        public void Select(VisualElement selectionContainer, bool additive) { }
        public void Unselect(VisualElement selectionContainer) { }

        public bool IsSelected(VisualElement selectionContainer) => selected;
    }

    public class TestSelectedHandle : ISelectedHandle
    {
        public string name { get; private set; }
        public bool isSelected { get; set; }

        public TestSelectedHandle(string name)
        {
            this.name = name;
        }

        // ISelectedHandle 接口方法实现
        public bool Validate() => true;
        public bool IsSelected() => isSelected;
        public void Select() => isSelected = true;
        public void Unselect() => isSelected = false;

        public IEnumerable<Object> GetSelectedObjects()
        {
            // 返回一个测试对象
            yield return ScriptableObject.CreateInstance<TestNodeAsset>();
        }
    }

    // 测试用的操作菜单动作
    public class TestOperateMenuAction : IOperateMenuAction
    {
        public string name { get; private set; }
        public OperateMenuActionValidity validity { get; private set; }
        public bool isOnValue { get; set; }

        public TestOperateMenuAction(string name, OperateMenuActionValidity validity)
        {
            this.name = name;
            this.validity = validity;
        }

        public bool isOn => isOnValue;

        public OperateMenuActionValidity GetValidity(OperateMenuContext context) => validity;

        public void Execute(OperateMenuActionContext context)
        {
            // 测试执行逻辑
        }
    }

    // 测试工具类
    public static class TestUtility
    {
        /// <summary>
        /// 创建测试端口
        /// </summary>
        public static Port CreateTestPort(Direction direction, Type type) => Port.Create<Edge>(Orientation.Horizontal, direction, Port.Capacity.Single, type);

        /// <summary>
        /// 创建测试节点资产
        /// </summary>
        public static TestNodeAsset CreateTestNodeAsset(string id = null, Vector2? position = null)
        {
            var nodeAsset = ScriptableObject.CreateInstance<TestNodeAsset>();
            nodeAsset.id = id ?? Guid.NewGuid().ToString();
            nodeAsset.position = new Rect(position ?? Vector2.zero, new Vector2(100, 100));
            return nodeAsset;
        }

        /// <summary>
        /// 创建测试边资产
        /// </summary>
        public static TestEdgeAsset CreateTestEdgeAsset(string outputNodeId, string inputNodeId, string outputPortId = "output", string inputPortId = "input")
        {
            var edgeAsset = ScriptableObject.CreateInstance<TestEdgeAsset>();
            edgeAsset.id = Guid.NewGuid().ToString();
            edgeAsset.outputNodeId = outputNodeId;
            edgeAsset.inputNodeId = inputNodeId;
            edgeAsset.outputPortId = outputPortId;
            edgeAsset.inputPortId = inputPortId;
            return edgeAsset;
        }

        /// <summary>
        /// 创建测试Item资产
        /// </summary>
        public static TestItemAsset CreateTestItemAsset(string id = null, Vector2? position = null)
        {
            var itemAsset = ScriptableObject.CreateInstance<TestItemAsset>();
            itemAsset.id = id ?? Guid.NewGuid().ToString();
            itemAsset.position = new Rect(position ?? Vector2.zero, new Vector2(100, 100));
            return itemAsset;
        }

        /// <summary>
        /// 清理测试资产
        /// </summary>
        public static void CleanupAssets(params Object[] assets)
        {
            foreach (var asset in assets)
            {
                if (asset != null)
                {
                    Object.DestroyImmediate(asset);
                }
            }
        }

        /// <summary>
        /// 清理测试资产列表
        /// </summary>
        public static void CleanupAssetList<T>(List<T> assets) where T : Object
        {
            foreach (var asset in assets)
            {
                if (asset != null)
                {
                    Object.DestroyImmediate(asset);
                }
            }
            assets.Clear();
        }
    }
}