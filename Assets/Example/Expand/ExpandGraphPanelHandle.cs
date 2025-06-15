using Emilia.Kit;
using Emilia.Node.Editor;

namespace Example.Expand
{
    //Panel拓展
    [EditorHandle(typeof(ExpandAsset))]
    public class ExpandGraphPanelHandle : GraphPanelHandle
    {
        //在初始化时加载Panel
        public override void LoadPanel(EditorGraphView graphView, GraphPanelSystem system)
        {
            base.LoadPanel(graphView, system);

            //加载工具栏
            system.OpenDockPanel<ExpandToolbarView>(20, GraphDockPosition.Top);
        }
    }
}