using System;

namespace Emilia.Node.Attributes
{
    /// <summary>
    /// 用于EditorNodeView指定EditorNodeAsset
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EditorNodeAttribute : Attribute
    {
        public Type nodeType;

        public EditorNodeAttribute(Type nodeType)
        {
            this.nodeType = nodeType;
        }
    }
}