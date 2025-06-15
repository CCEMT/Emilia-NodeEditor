using System;

namespace Example.RuntimeNode.Runtime
{
    /// <summary>
    /// 节点菜单
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RuntimeNodeMenuAttribute : Attribute
    {
        public string path;
        public int priority;

        public RuntimeNodeMenuAttribute(string path, int priority = 0)
        {
            this.path = path;
            this.priority = priority;
        }
    }
}