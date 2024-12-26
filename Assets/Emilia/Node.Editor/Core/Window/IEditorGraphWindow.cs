namespace Emilia.Node.Editor
{
    public interface IEditorGraphWindow
    {
        /// <summary>
        /// 资源
        /// </summary>
        EditorGraphAsset graphAsset { get; }

        /// <summary>
        /// 设置资源
        /// </summary>
        void SetGraphAsset(EditorGraphAsset graphAsset);
    }
}