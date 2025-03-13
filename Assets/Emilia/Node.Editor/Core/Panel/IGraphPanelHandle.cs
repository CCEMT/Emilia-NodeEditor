namespace Emilia.Node.Editor
{
    public interface IGraphPanelHandle : IEditorHandle
    {
        /// <summary>
        /// 加载面板
        /// </summary>
        void LoadPanel(GraphPanelSystem system);
    }
}