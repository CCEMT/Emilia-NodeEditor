namespace Emilia.Node.Editor
{
    public interface IOperateMenuAction
    {
        /// <summary>
        /// 是否选中
        /// </summary>
        bool isOn { get; }

        /// <summary>
        /// 获取有效性
        /// </summary>
        OperateMenuActionValidity GetValidity(OperateMenuContext context);

        /// <summary>
        /// 执行
        /// </summary>
        void Execute(OperateMenuActionContext context);
    }
}