using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Emilia.Node.Editor
{
    public interface IEditorNodeView : IDeleteGraphElement, IRemoveViewElement, IGraphCopyPasteElement, IGraphSelectable
    {
        EditorGraphView graphView { get; }
        EditorNodeAsset asset { get; }
        GraphElement element { get; }

        IReadOnlyList<IEditorPortView> portViews { get; }

        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize(EditorGraphView graphView, EditorNodeAsset asset);

        /// <summary>
        /// 收集静态端口资源
        /// </summary>
        List<EditorPortInfo> CollectStaticPortAssets();

        /// <summary>
        /// 根据id获取IEditorPortView
        /// </summary>
        IEditorPortView GetPortView(string portId);

        /// <summary>
        /// 添加IEditorPortView
        /// </summary>
        IEditorPortView AddPortView(int index,EditorPortInfo asset);

        /// <summary>
        /// 设置位置
        /// </summary>
        void SetPosition(Rect position);

        /// <summary>
        /// 设置位置，不记录撤销
        /// </summary>
        void SetPositionNoUndo(Rect position);

        /// <summary>
        /// 值改变
        /// </summary>
        void OnValueChanged(bool isSilent = false);

        /// <summary>
        /// 释放
        /// </summary>
        void Dispose();
    }
}