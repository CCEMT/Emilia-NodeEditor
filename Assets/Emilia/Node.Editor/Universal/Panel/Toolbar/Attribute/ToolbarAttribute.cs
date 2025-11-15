using System;

namespace Emilia.Node.Attributes
{
    /// <summary>
    /// 工具栏特性基类（在EditorGraphAsset中使用）
    /// </summary>
    public abstract class ToolbarAttribute : Attribute
    {
        public ToolbarViewControlPosition position { get; private set; }

        public ToolbarAttribute(ToolbarViewControlPosition position)
        {
            this.position = position;
        }
    }
}