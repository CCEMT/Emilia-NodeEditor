using System.Collections.Generic;
using Emilia.Kit;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public abstract class CreateItemMenuHandle
    {
        /// <summary>
        /// 收集Item的菜单项
        /// </summary>
        public virtual void CollectItemMenus(EditorGraphView graphView, List<CreateItemMenuInfo> itemTypes) { }
    }
}