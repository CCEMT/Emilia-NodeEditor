using System;

namespace Emilia.Node.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeTipsAttribute : Attribute
    {
        public string tips;

        public NodeTipsAttribute(string tips)
        {
            this.tips = tips;
        }
    }
}