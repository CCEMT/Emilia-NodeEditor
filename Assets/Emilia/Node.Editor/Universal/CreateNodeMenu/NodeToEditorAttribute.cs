using System;

namespace Emilia.Node.Universal.Editor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeToEditorAttribute : Attribute
    {
        public Type baseEditorNodeType;

        public NodeToEditorAttribute(Type baseEditorNodeType)
        {
            this.baseEditorNodeType = baseEditorNodeType;
        }
    }
}