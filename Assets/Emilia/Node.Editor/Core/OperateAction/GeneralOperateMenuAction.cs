using System;

namespace Emilia.Node.Editor
{
    public class GeneralOperateMenuAction : OperateMenuAction
    {
        public Func<bool> isOnCallback;
        public Func<OperateMenuContext, OperateMenuActionValidity> validityCallback;
        public Action<OperateMenuActionContext> executeCallback;

        public override bool isOn => this.isOnCallback?.Invoke() ?? base.isOn;

        public override OperateMenuActionValidity GetValidity(OperateMenuContext context)
        {
            return this.validityCallback?.Invoke(context) ?? base.GetValidity(context);
        }

        public override void Execute(OperateMenuActionContext context)
        {
            this.executeCallback?.Invoke(context);
        }

        public OperateMenuActionInfo ToActionInfo(string name, string category, int priority)
        {
            OperateMenuActionInfo actionInfo = new OperateMenuActionInfo();
            actionInfo.name = name;
            actionInfo.category = category;
            actionInfo.action = this;
            actionInfo.priority = priority;
            return actionInfo;
        }
    }
}