using System;

namespace Emilia.Node.Editor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EditorEdgeAttribute : Attribute
    {
        public Type edgeType;

        public EditorEdgeAttribute(Type edgeType)
        {
            this.edgeType = edgeType;
        }
    }
}