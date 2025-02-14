using System.Linq;
using Emilia.Kit.Editor;
using Emilia.Node.Editor;
using UnityEditor.Experimental.GraphView;

namespace Emilia.Node.Universal.Editor
{
    public interface ISpecifyOpenScriptObject
    {
        public object openScriptObject { get; }
    }

    /// <summary>
    /// 打开脚本
    /// </summary>
    [Action("OpenScript", 8100, OperateMenuTagDefine.UniversalActionTag)]
    public class OpenScriptAction : OperateMenuAction
    {
        public override OperateMenuActionValidity GetValidity(OperateMenuContext context)
        {
            return context.graphView.selection.Count > 0 ? OperateMenuActionValidity.Valid : OperateMenuActionValidity.Invalid;
        }

        public override void Execute(OperateMenuActionContext context)
        {
            ISelectable selectable = context.graphView.selection.FirstOrDefault();
            if (selectable == null) return;

            object openScriptObject = selectable;
            ISpecifyOpenScriptObject specifyOpenScriptObject = selectable as ISpecifyOpenScriptObject;
            if (specifyOpenScriptObject != null) openScriptObject = specifyOpenScriptObject.openScriptObject;

            OpenScriptUtility.OpenScript(openScriptObject);
        }
    }
}