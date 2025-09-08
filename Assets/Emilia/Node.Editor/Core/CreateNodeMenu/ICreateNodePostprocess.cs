namespace Emilia.Node.Editor
{
    public interface ICreateNodePostprocess
    {
        void Postprocess(EditorGraphView graphView,IEditorNodeView nodeView, CreateNodeContext createNodeContext);
    }
}