using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    [Action("Refresh", 8000, OperateMenuTagDefine.UniversalActionTag)]
    public class RefreshAction : OperateMenuAction
    {
        public override void Execute(OperateMenuActionContext context)
        {
            context.graphView.AllReload();
        }
    }
}