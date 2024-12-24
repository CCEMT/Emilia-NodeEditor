using System;

namespace Emilia.Node.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class VariableKeyTypeFilterAttribute : Attribute
    {
        public Type type;

        public VariableKeyTypeFilterAttribute(Type type)
        {
            this.type = type;
        }
    }
}