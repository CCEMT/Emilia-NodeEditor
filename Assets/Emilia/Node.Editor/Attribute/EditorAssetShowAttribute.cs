using System;

namespace Emilia.Node.Attributes
{
    /// <summary>
    /// 以编辑方式展示资源
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class EditorAssetShowAttribute : Attribute
    {
        public float height = 300;
        public float width = -1f;
    }
}