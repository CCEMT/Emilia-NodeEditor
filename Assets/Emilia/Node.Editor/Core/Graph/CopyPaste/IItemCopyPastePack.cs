using Emilia.Kit;

namespace Emilia.Node.Editor
{
    public interface IItemCopyPastePack : ICopyPastePack
    {
        EditorItemAsset copyAsset { get; }
        EditorItemAsset pasteAsset { get; }
    }
}