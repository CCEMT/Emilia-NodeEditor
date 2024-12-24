using System;

namespace Emilia.BehaviorTree.Attributes
{
    public class VariableTypeFilterAttribute : Attribute
    {
        public Type type { get; private set; }
        public string getTypeExpression { get; private set; }

        public VariableTypeFilterAttribute(Type type)
        {
            this.type = type;
            getTypeExpression = null;
        }

        public VariableTypeFilterAttribute(string getTypeExpression)
        {
            this.getTypeExpression = getTypeExpression;
            type = null;
        }
    }
}