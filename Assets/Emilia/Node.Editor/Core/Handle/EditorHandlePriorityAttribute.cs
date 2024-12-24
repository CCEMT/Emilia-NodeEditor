using System;

namespace Emilia.Node.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EditorHandlePriorityAttribute : Attribute
    {
        public double priority;

        public EditorHandlePriorityAttribute(double priority)
        {
            this.priority = priority;
        }
    }
}