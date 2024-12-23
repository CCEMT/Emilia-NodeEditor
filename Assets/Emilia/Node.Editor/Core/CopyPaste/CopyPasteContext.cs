using System.Collections.Generic;

namespace Emilia.Node.Editor
{
    public struct CopyPasteContext
    {
        public object userData;
        public List<ICopyPastePack> dependency;
    }
}