using System;

namespace Emilia.Node.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class EditorAssetShowAttribute : Attribute
    {
        public float height = 300;
        public float width = -1f;
    }
}