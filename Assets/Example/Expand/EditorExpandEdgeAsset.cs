using Emilia.Node.Editor;
using Emilia.Node.Universal.Editor;

namespace Example.Expand
{
    public class EditorExpandEdgeAsset : UniversalEditorEdgeAsset { }

    [EditorEdge(typeof(EditorExpandEdgeAsset))]
    public class EditorExpandEdgeView : UniversalEditorEdgeView
    {
        public virtual void OnCreate() { }
    }
}