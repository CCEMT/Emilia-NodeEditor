using Emilia.Kit;
using Emilia.Node.Editor;
using Emilia.Node.Universal.Editor;

namespace Example
{
    [EditorHandle(typeof(CapabilitiesDisplayAsset))]
    public class CapabilitiesDisplayGraphPanelHandle : GraphPanelHandle
    {
        public override void LoadPanel(EditorGraphView graphView, GraphPanelSystem system)
        {
            base.LoadPanel(graphView, system);
            system.OpenDockPanel<CapabilitiesDisplayToolbarView>(20, GraphDockPosition.Top);
            system.OpenDockPanel<LayerView>(20, GraphDockPosition.Top);

            AppendPanel appendPanel = system.OpenDockPanel<AppendPanel>(200, GraphDockPosition.Right);
            appendPanel.AddGraphPanel<CreateNodeView>("创建节点");
            appendPanel.AddGraphPanel<CreateNodeView>("创建节点");

            system.OpenFloatPanel<MiniMapView>();
        }
    }
}