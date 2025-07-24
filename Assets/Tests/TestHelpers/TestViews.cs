using System;
using System.Collections.Generic;
using Emilia.Kit;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor.Tests
{
    // 测试用的视图接口实现
    public class TestEditorNodeView : IEditorNodeView
    {
        public EditorGraphView graphView { get; }
        public EditorNodeAsset asset { get; set; }
        public GraphElement element { get; }
        public List<IEditorPortView> _portViews = new List<IEditorPortView>();
        public IReadOnlyList<IEditorPortView> portViews => _portViews;

        public void Initialize(EditorGraphView graphView, EditorNodeAsset asset) { }

        public List<EditorPortInfo> CollectStaticPortAssets() => new List<EditorPortInfo>();

        public IEditorPortView GetPortView(string portId)
        {
            return _portViews.Find(p => p.info.id == portId);
        }

        public IEditorPortView AddPortView(int index, EditorPortInfo asset)
        {
            TestEditorPortView portView = new TestEditorPortView();
            portView.Initialize(this, asset);
            _portViews.Add(portView);
            return portView;
        }

        public void SetPosition(Rect position) { }
        public void SetPositionNoUndo(Rect position) { }
        public void OnValueChanged(bool isSilent = false) { }
        public void Dispose() { }
        public string nodeId => asset?.id;
        public List<IEditorPortView> inputPorts { get; set; } = new List<IEditorPortView>();
        public List<IEditorPortView> outputPorts { get; set; } = new List<IEditorPortView>();
        public List<IEditorPortView> allPorts { get; set; } = new List<IEditorPortView>();

        public void RemoveView() { }
        public void CollectElements(HashSet<GraphElement> collectedElementSet, Func<GraphElement, bool> conditionFunc) { }
        public bool Validate() => true;
        public bool IsSelected() => false;
        public void Select() { }
        public void Unselect() { }
        public IEnumerable<Object> GetSelectedObjects() => new List<Object>();
        public ICopyPastePack GetPack() => null;
        public void Delete() { }
    }

    public class TestEditorEdgeView : IEditorEdgeView
    {
        private Edge _edgeElement;
        public EditorEdgeAsset asset { get; set; }
        public EditorGraphView graphView { get; }
        public string edgeId => asset?.id;
        public IEditorPortView inputPortView { get; set; }
        public IEditorPortView outputPortView { get; set; }

        // IEditorEdgeView 接口要求的属性
        public bool isDrag { get; set; }
        public GraphElement edgeElement { get; set; }

        // IEditorEdgeView 接口要求的方法
        Edge IEditorEdgeView.edgeElement => this._edgeElement;

        public void Initialize(EditorGraphView graphView, EditorEdgeAsset asset)
        {
            this.asset = asset;
        }

        public void OnValueChanged(bool isSilent = false) { }
        public void Dispose() { }

        public void RemoveView() { }
        public void CollectElements(HashSet<GraphElement> collectedElementSet, Func<GraphElement, bool> conditionFunc) { }
        public bool Validate() => true;
        public bool IsSelected() => false;
        public void Select() { }
        public void Unselect() { }
        public IEnumerable<Object> GetSelectedObjects() => new List<Object>();
        public ICopyPastePack GetPack() => null;

        // IDeleteGraphElement 接口要求的方法
        public void DeleteGraphElement() { }

        public void Delete() { }
    }

    public class TestEditorItemView : IEditorItemView
    {
        public EditorItemAsset asset { get; set; }
        public string itemId => asset?.id;

        // IEditorItemView 接口要求的属性
        public GraphElement element { get; set; }
        public EditorGraphView graphView { get; }

        // IEditorItemView 接口要求的方法
        public void Initialize(EditorGraphView graphView, EditorItemAsset asset)
        {
            this.asset = asset;
        }

        public void SetPosition(Rect rect) { }

        public void SetPositionNoUndo(Rect position) { }

        public void SetPositionNoUndoRedo(Rect rect) { }
        public void OnValueChanged(bool isSilent = false) { }
        public void Dispose() { }

        public void RemoveView() { }
        public void CollectElements(HashSet<GraphElement> collectedElementSet, Func<GraphElement, bool> conditionFunc) { }
        public bool Validate() => true;
        public bool IsSelected() => false;
        public void Select() { }
        public void Unselect() { }
        public IEnumerable<Object> GetSelectedObjects() => new List<Object>();
        public ICopyPastePack GetPack() => null;

        // IDeleteGraphElement 接口要求的方法
        public void DeleteGraphElement() { }
        public void Delete() { }
    }

    public class TestEditorPortView : IEditorPortView
    {
        public string nodeId { get; set; }
        public string portId { get; set; }

        private readonly List<IEditorEdgeView> _edges = new List<IEditorEdgeView>();

        // IEditorPortView 属性
        public EditorPortInfo info { get; set; }
        public IEditorNodeView master { get; set; }
        public EditorPortDirection portDirection { get; set; }
        public EditorOrientation editorOrientation { get; set; }
        public Port portElement { get; set; }
        public IReadOnlyList<IEditorEdgeView> edges => _edges;

        // 事件
        public event Action<IEditorPortView, IEditorEdgeView> onConnected;
        public event Action<IEditorPortView, IEditorEdgeView> OnDisconnected;

        // 辅助方法
        public void AddEdge(IEditorEdgeView edge)
        {
            if (! _edges.Contains(edge))
            {
                _edges.Add(edge);
            }
        }

        public void RemoveEdge(IEditorEdgeView edge)
        {
            _edges.Remove(edge);
        }

        // 方法实现
        public void Initialize(IEditorNodeView master, EditorPortInfo info)
        {
            this.master = master;
            this.info = info;
        }

        public void Dispose() { }
        public void RemoveView() { }
        public bool Validate() => true;
        public bool IsSelected() => false;
        public void Select() { }
        public void Unselect() { }
        public IEnumerable<Object> GetSelectedObjects() => new List<Object>();
        public ICopyPastePack GetPack() => null;
    }
}