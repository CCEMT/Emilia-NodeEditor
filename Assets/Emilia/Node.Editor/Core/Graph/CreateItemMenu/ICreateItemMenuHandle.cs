using System.Collections.Generic;

namespace Emilia.Node.Editor
{
    public interface ICreateItemMenuHandle : IEditorHandle
    {
        /// <summary>
        /// 收集所有的创建Item菜单
        /// </summary>
        void CollectItemMenus(List<CreateItemMenuInfo> itemTypes);
    }
}