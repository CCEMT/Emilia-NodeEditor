using System.Collections.Generic;

namespace Emilia.Node.Editor
{
    /// <summary>
    /// 拷贝粘贴上下文
    /// </summary>
    public struct CopyPasteContext
    {
        public object userData;

        /// <summary>
        /// 依赖的Pack
        /// </summary>
        public List<ICopyPastePack> dependency;
    }
}