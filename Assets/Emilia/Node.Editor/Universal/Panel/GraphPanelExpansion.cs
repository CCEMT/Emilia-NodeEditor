using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    public static class GraphPanelExpansion
    {
        public static IGraphPanel SetId(this IGraphPanel graphPanel, string id)
        {
            graphPanel.id = id;
            return graphPanel;
        }
    }
}