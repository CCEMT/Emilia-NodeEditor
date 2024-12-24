using System;

namespace Emilia.Node.Editor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ActionAttribute : Attribute
    {
        public readonly string name;
        public readonly string category;

        public int priority;
        public string[] tags;

        public ActionAttribute(string path, int priority = 0, params string[] tags)
        {
            this.priority = priority;
            OperateMenuUtility.PathToNameAndCategory(path, out name, out this.category);
            this.tags = tags;
        }
    }
}