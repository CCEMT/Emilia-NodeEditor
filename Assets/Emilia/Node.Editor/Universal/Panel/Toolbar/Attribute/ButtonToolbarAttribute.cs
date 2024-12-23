using System;

namespace Emilia.Node.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonToolbarAttribute : ToolbarAttribute
    {
        public string displayName;

        public ButtonToolbarAttribute(string displayName, ToolbarViewControlPosition position) : base(position)
        {
            this.displayName = displayName;
        }
    }
}