using Emilia.Node.Universal.Editor;

namespace Example.Expand.Node
{
    public abstract class EditorExpandNodeAsset : UniversalNodeAsset { }

    public abstract class EditorExpandNodeView : UniversalEditorNodeView
    {
        public virtual void OnCreate() { }
    }
}