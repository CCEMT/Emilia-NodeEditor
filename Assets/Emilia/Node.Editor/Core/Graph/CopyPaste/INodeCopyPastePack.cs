using Emilia.Kit;

namespace Emilia.Node.Editor
{
    public interface INodeCopyPastePack : ICopyPastePack
    {
        EditorNodeAsset copyAsset { get; }

        EditorNodeAsset pasteAsset { get; }
    }
}