using Emilia.Node.Attributes;
using Emilia.Node.Universal.Editor;

namespace Example.Expand.Node
{
    [NodeMenu("Test")]
    public class TestExpandNodeAsset : EditorExpandNodeAsset { }

    [EditorNode(typeof(TestExpandNodeAsset))]
    public class TestExpandNodeView : EditorExpandNodeView { }
}