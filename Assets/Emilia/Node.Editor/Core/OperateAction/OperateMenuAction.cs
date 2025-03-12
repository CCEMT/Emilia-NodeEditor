namespace Emilia.Node.Editor
{
    public abstract class OperateMenuAction : IOperateMenuAction
    {
        public virtual bool isOn => false;

        public virtual OperateMenuActionValidity GetValidity(OperateMenuContext context)
        {
            return OperateMenuActionValidity.Valid;
        }

        public abstract void Execute(OperateMenuActionContext context);
    }
}