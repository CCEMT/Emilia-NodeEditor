using System;
using System.Collections.Generic;
using Emilia.Kit;

namespace Emilia.Node.Editor
{
    public interface IPortCopyPastePack : ICopyPastePack
    {
        string nodeId { get; }
        string portId { get; }
        Type portType { get; }
        EditorPortDirection direction { get; }
        List<IEdgeCopyPastePack> connectionPacks { get; }
    }
}