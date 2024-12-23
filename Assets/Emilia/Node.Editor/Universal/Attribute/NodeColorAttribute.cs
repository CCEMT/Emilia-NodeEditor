using System;

namespace Emilia.Node.Universal.Editor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeColorAttribute : Attribute
    {
        public float r;
        public float g;
        public float b;

        public NodeColorAttribute(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }
    }
}