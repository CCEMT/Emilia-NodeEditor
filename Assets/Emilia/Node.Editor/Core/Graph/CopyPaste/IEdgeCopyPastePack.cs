using Emilia.Kit;

namespace Emilia.Node.Editor
{
    public interface IEdgeCopyPastePack : ICopyPastePack
    {
        EditorEdgeAsset copyAsset { get; }
        EditorEdgeAsset pasteAsset { get; }
    }
}