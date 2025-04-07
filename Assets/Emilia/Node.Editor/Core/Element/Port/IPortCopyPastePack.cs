using System.Collections.Generic;
using Emilia.Kit;

namespace Emilia.Node.Editor
{
    public interface IPortCopyPastePack : ICopyPastePack
    {
        List<ICopyPastePack> connectionPacks { get; }
    }
}