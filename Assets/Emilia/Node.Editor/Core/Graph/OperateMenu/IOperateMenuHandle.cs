using System.Collections.Generic;

namespace Emilia.Node.Editor
{
    public interface IOperateMenuHandle : IEditorHandle
    {
        void InitializeCache();
        void CollectMenuItems(List<OperateMenuItem> menuItems, OperateMenuContext context);
    }
}