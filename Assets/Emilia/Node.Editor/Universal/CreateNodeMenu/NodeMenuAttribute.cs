using System;

namespace Emilia.Node.Universal.Editor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeMenuAttribute : Attribute
    {
        public int priority;
        public string path;

        public NodeMenuAttribute(string path, int priority = 0)
        {
            this.path = path;
            this.priority = priority;
        }
    }
}