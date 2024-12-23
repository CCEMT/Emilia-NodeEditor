namespace Emilia.Node.Editor
{
    public interface IEditorGraphWindow
    {
        EditorGraphAsset graphAsset { get; }
        
        void SetGraphAsset(EditorGraphAsset graphAsset);
    }
}