﻿using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    [Action("Paste", 5200, OperateMenuTagDefine.BaseActionTag)]
    public class PasteAction : OperateMenuAction
    {
        public override OperateMenuActionValidity GetValidity(OperateMenuContext context)
        {
            return context.graphView.graphCopyPaste.CanPasteSerializedDataCallback(context.graphView.clipboard_Internal) ? OperateMenuActionValidity.Valid : OperateMenuActionValidity.Invalid;
        }

        public override void Execute(OperateMenuActionContext context)
        {
            context.graphView.graphOperate.Paste();
        }
    }
}