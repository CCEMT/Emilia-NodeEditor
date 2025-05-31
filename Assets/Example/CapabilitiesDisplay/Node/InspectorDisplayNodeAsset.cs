using Emilia.Node.Attributes;
using Emilia.Node.Universal.Editor;

namespace Example
{
    [NodeMenu("Inspector演示节点")]
    public class InspectorDisplayNodeAsset : PortDisplayNodeAsset
    {
        protected override string defaultDisplayName => "Inspector演示节点";
    }

    [EditorNode(typeof(InspectorDisplayNodeAsset))]
    public class InspectorDisplayNodeView : PortDisplayNodeView
    {
        protected override bool editInNode => true;
    }
}