using System;

namespace Emilia.Node.Editor
{
    [Flags]
    public enum GraphPanelCapabilities
    {
        None = 0,
        Movable = 1,
        Resizable = 2
    }
}