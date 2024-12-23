using System;

namespace Emilia.Node.Universal.Editor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeToRuntimeAttribute : Attribute
    {
        public Type baseRuntimeNodeType;
        public Type baseEditorNodeType;

        public NodeToRuntimeAttribute(Type baseRuntimeNodeType, Type baseEditorNodeType)
        {
            this.baseRuntimeNodeType = baseRuntimeNodeType;
            this.baseEditorNodeType = baseEditorNodeType;
        }
    }
}