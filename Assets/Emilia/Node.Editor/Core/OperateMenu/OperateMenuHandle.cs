using System.Collections.Generic;
using Emilia.Kit;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public abstract class OperateMenuHandle
    {
        public virtual void InitializeCache(EditorGraphView graphView, List<OperateMenuActionInfo> actionInfos) { }
        public virtual void CollectMenuItems(EditorGraphView graphView, List<OperateMenuItem> menuItems, OperateMenuContext context) { }
    }
}