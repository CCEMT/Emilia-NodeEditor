using Emilia.Kit;
using Emilia.Node.Editor;
using Example.Expand.Node;

namespace Example.Expand
{
    //创建节点拖拽
    [EditorHandle(typeof(ExpandAsset))]
    public class ExpandNodeSystemHandle : NodeSystemHandle
    {
        public override void OnCreateNode(EditorGraphView graphView, IEditorNodeView editorNodeView)
        {
            base.OnCreateNode(graphView, editorNodeView);
            EditorExpandNodeView expandNodeView = editorNodeView as EditorExpandNodeView;
            expandNodeView?.OnCreate();
        }
    }
}