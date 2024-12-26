using System;

namespace Emilia.Node.Attributes
{
    /// <summary>
    /// 用于EditorItemView指定EditorItemAsset
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EditorItemAttribute : Attribute
    {
        public Type itemType;

        public EditorItemAttribute(Type itemType)
        {
            this.itemType = itemType;
        }
    }
}