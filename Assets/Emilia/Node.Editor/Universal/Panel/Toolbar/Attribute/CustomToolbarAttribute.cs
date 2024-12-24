using System;

namespace Emilia.Node.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CustomToolbarAttribute : ToolbarAttribute
    {
        public CustomToolbarAttribute(ToolbarViewControlPosition position) : base(position) { }
    }
}