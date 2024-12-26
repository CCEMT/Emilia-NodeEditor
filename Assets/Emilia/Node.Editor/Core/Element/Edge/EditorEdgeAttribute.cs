using System;

namespace Emilia.Node.Editor
{
    /// <summary>
    /// 用于EditorEdgeView指定EditorEdgeAsset
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EditorEdgeAttribute : Attribute
    {
        public Type edgeType;

        public EditorEdgeAttribute(Type edgeType)
        {
            this.edgeType = edgeType;
        }
    }
}