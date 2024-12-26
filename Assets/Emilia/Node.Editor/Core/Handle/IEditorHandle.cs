namespace Emilia.Node.Editor
{
    public interface IEditorHandle
    {
        /// <summary>
        /// 父级
        /// </summary>
        IEditorHandle parent { get; set; }

        /// <summary>
        /// 弱引用智能值
        /// </summary>
        object weakSmartValue { get; }

        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize(object weakSmartValue);
    }
}