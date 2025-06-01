using Emilia.Kit;

namespace Emilia.Node.Editor
{
    public interface IItemCopyPastePack : ICopyPastePack
    {
        /// <summary>
        /// 拷贝的资源
        /// </summary>
        EditorItemAsset copyAsset { get; }
        
        /// <summary>
        /// 粘贴的资源
        /// </summary>
        EditorItemAsset pasteAsset { get; }
    }
}