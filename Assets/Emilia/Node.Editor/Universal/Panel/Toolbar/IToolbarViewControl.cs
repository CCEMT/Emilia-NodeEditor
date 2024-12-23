namespace Emilia.Node.Universal.Editor
{
    public interface IToolbarViewControl
    {
        bool isActive { get; set; }
        void OnDraw();
    }
}