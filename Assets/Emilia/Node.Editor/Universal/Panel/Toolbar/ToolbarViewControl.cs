namespace Emilia.Node.Universal.Editor
{
    public abstract class ToolbarViewControl : IToolbarViewControl
    {
        public bool isActive { get; set; }
        public abstract void OnDraw();
    }
}