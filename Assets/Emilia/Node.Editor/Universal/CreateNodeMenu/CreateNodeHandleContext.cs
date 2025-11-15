using System;

namespace Emilia.Node.Editor
{
    /// <summary>
    /// 创建节点适配器上下文
    /// </summary>
    public struct CreateNodeHandleContext
    {
        public Type nodeType;
        public Type defaultEditorNodeType;
    }
}