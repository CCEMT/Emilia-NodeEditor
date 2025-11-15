using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    /// <summary>
    /// 面板实用函数拓展
    /// </summary>
    public static class GraphPanelExpansion
    {
        public static IGraphPanel SetId(this IGraphPanel graphPanel, string id)
        {
            graphPanel.id = id;
            return graphPanel;
        }
    }
}