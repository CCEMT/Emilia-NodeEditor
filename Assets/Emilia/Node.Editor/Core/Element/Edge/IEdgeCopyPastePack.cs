using Emilia.Kit;

namespace Emilia.Node.Editor
{
    /// <summary>
    /// Edge拷贝粘贴Pack接口
    /// </summary>
    public interface IEdgeCopyPastePack : ICopyPastePack
    {
        EditorEdgeAsset copyAsset { get; }
        EditorEdgeAsset pasteAsset { get; }
    }
}