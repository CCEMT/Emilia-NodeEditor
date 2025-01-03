using Emilia.Kit;

namespace Emilia.Node.Editor
{
    public interface IGraphCopyPasteElement
    {
        /// <summary>
        /// 获取复制粘贴包
        /// </summary>
        ICopyPastePack GetPack();
    }
}