using System.Collections.Generic;

namespace Emilia.Node.Editor
{
    public interface IOperateMenuHandle : IEditorHandle
    {
        /// <summary>
        /// 初始化缓存
        /// </summary>
        void InitializeCache();
        
        /// <summary>
        /// 收集菜单项
        /// </summary>
        void CollectMenuItems(List<OperateMenuItem> menuItems, OperateMenuContext context);
    }
}