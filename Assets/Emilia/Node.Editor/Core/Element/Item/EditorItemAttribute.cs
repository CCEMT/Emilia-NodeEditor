using System;

namespace Emilia.Node.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EditorItemAttribute : Attribute
    {
        public Type itemType;

        public EditorItemAttribute(Type itemType)
        {
            this.itemType = itemType;
        }
    }
}