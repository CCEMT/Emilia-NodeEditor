using System;

namespace Emilia.Node.Attributes
{
    /// <summary>
    /// 设置EditorHandle的优先级
    /// </summary>
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