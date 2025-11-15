namespace Emilia.Node.Universal.Editor
{
    /// <summary>
    /// 工具栏控件
    /// </summary>
    public abstract class ToolbarViewControl : IToolbarViewControl
    {
        public bool isActive { get; set; }
        public abstract void OnDraw();
    }
}