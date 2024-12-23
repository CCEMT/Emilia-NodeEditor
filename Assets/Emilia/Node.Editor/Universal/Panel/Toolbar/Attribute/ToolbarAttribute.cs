using System;

namespace Emilia.Node.Attributes
{
    public abstract class ToolbarAttribute : Attribute
    {
        public ToolbarViewControlPosition position { get; private set; }

        public ToolbarAttribute(ToolbarViewControlPosition position)
        {
            this.position = position;
        }
    }
}