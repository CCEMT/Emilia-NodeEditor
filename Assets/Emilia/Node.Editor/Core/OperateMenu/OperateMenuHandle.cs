using System.Collections.Generic;
using Emilia.Kit;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public abstract class OperateMenuHandle
    {
        /// <summary>
        /// 初始化缓存
        /// </summary>
        public virtual void InitializeCache(EditorGraphView graphView, List<OperateMenuActionInfo> actionInfos) { }
        
        /// <summary>
        /// 收集操作菜单项
        /// </summary>
        public virtual void CollectMenuItems(EditorGraphView graphView, List<OperateMenuItem> menuItems, OperateMenuContext context) { }
    }
}