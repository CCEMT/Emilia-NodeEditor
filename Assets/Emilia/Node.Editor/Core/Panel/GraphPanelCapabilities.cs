using System;

namespace Emilia.Node.Editor
{
    [Flags]
    public enum GraphPanelCapabilities
    {
        None = 0,

        /// <summary>
        /// 面板可以移动
        /// </summary>
        Movable = 1,

        /// <summary>
        /// 面板可以调整大小
        /// </summary>
        Resizable = 2
    }
}