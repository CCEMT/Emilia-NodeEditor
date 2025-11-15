using System;
using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    /// <summary>
    /// 操作菜单特性（在EditorGraphAsset中的函数使用）
    /// </summary>
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