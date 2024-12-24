using System;

namespace Emilia.Node.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ToggleToolbarAttribute : ToolbarAttribute
    {
        public string displayName { get; private set; }

        public ToggleToolbarAttribute(string displayName, ToolbarViewControlPosition position) : base(position)
        {
            this.displayName = displayName;
        }
    }
}