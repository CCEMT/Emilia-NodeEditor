using Emilia.Kit;
using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public abstract class GraphHotKeysHandle
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Initialize(EditorGraphView graphView) { }

        /// <summary>
        /// 处理按键点击事件
        /// </summary>
        public virtual void OnKeyDown(EditorGraphView graphView, KeyDownEvent evt) { }

        /// <summary>
        /// 释放
        /// </summary>
        public virtual void Dispose() { }
    }
}