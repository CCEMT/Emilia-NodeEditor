using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    [Action("Duplicate", 5300, OperateMenuTagDefine.BaseActionTag)]
    public class DuplicateAction : OperateMenuAction
    {
        public override OperateMenuActionValidity GetValidity(OperateMenuContext context)
        {
            return context.graphView.selection.Count > 0 ? OperateMenuActionValidity.Valid : OperateMenuActionValidity.Invalid;
        }

        public override void Execute(OperateMenuActionContext context)
        {
            context.graphView.graphOperate.Duplicate();
        }
    }
}