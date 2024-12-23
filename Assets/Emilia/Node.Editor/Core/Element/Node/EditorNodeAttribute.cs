using System;

namespace Emilia.Node.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EditorNodeAttribute : Attribute
    {
        public Type nodeType;

        public EditorNodeAttribute(Type nodeType)
        {
            this.nodeType = nodeType;
        }
    }
}