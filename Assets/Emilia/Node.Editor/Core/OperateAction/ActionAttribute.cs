using System;

namespace Emilia.Node.Editor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ActionAttribute : Attribute
    {
        /// <summary>
        /// 名称
        /// </summary>
        public readonly string name;

        /// <summary>
        /// 分组
        /// </summary>
        public readonly string category;

        /// <summary>
        /// 优先级 
        /// </summary>
        public int priority;

        /// <summary>
        /// 标签
        /// </summary>
        public string[] tags;

        public ActionAttribute(string path, int priority = 0, params string[] tags)
        {
            this.priority = priority;
            OperateMenuUtility.PathToNameAndCategory(path, out name, out this.category);
            this.tags = tags;
        }
    }
}