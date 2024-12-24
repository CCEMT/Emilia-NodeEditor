using System;
using System.Collections.Generic;
using Sirenix.Serialization;

namespace Emilia.Node.Editor
{
    [Serializable]
    public class CopyPasteNode
    {
        [OdinSerialize, NonSerialized]
        public ICopyPastePack pack;

        [OdinSerialize, NonSerialized]
        public List<CopyPasteNode> output;

        [OdinSerialize, NonSerialized]
        public List<CopyPasteNode> input;

        public CopyPasteNode(ICopyPastePack pack)
        {
            this.pack = pack;

            this.output = new List<CopyPasteNode>();
            this.input = new List<CopyPasteNode>();
        }
    }
}