using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Emilia.Node.Editor
{
    public interface IEditorPortView : IRemoveViewElement, IGraphSelectable, IGraphCopyPasteElement
    {
        /// <summary>
        /// 端口创建信息
        /// </summary>
        EditorPortInfo info { get; }

        /// <summary>
        /// 端口所属节点
        /// </summary>
        IEditorNodeView master { get; }

        /// <summary>
        /// 端口方向
        /// </summary>
        EditorPortDirection portDirection { get; }

        /// <summary>
        /// 端口取向
        /// </summary>
        EditorOrientation editorOrientation { get; }

        Port portElement { get; }

        IReadOnlyList<IEditorEdgeView> edges { get; }

        /// <summary>
        /// 连接事件
        /// </summary>
        event Action<IEditorPortView, IEditorEdgeView> onConnected;

        /// <summary>
        /// 断开连接事件
        /// </summary>
        event Action<IEditorPortView, IEditorEdgeView> OnDisconnected;

        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize(IEditorNodeView master, EditorPortInfo info);

        /// <summary>
        /// 释放
        /// </summary>
        void Dispose();
    }
}