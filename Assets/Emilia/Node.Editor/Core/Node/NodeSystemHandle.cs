using Emilia.Kit;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public abstract class NodeSystemHandle
    {
        /// <summary>
        /// 创建节点时的处理
        /// </summary>
        public virtual void OnCreateNode(EditorGraphView graphView, IEditorNodeView editorNodeView) { }
    }
}