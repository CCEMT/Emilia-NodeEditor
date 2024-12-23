namespace Emilia.Node.Editor
{
    public interface IEditorHandle
    {
        IEditorHandle parent { get; set; }

        object weakSmartValue { get; }

        void Initialize(object weakSmartValue);
    }
}