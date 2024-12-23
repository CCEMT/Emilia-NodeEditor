using System;
using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MenuAttribute : Attribute
    {
        public readonly string name;
        public readonly string category;

        public int priority;

        public string isOnExpression;
        public string actionValidityMethod;

        public MenuAttribute(string path, int priority)
        {
            this.priority = priority;
            OperateMenuUtility.PathToNameAndCategory(path, out name, out this.category);
        }
    }
}