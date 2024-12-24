namespace Emilia.Node.Editor
{
    public interface IOperateMenuAction
    {
        bool isOn { get; }

        OperateMenuActionValidity GetValidity(OperateMenuContext context);

        void Execute(OperateMenuActionContext context);
    }
}