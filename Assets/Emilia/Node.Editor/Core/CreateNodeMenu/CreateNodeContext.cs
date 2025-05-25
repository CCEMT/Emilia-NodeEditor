using UnityEngine;

namespace Emilia.Node.Editor
{
    public class CreateNodeContext
    {
        /// <summary>
        /// 鼠标位置
        /// </summary>
        public Vector2 screenMousePosition;

        /// <summary>
        /// 节点菜单
        /// </summary>
        public GraphCreateNodeMenu nodeMenu;

        /// <summary>
        /// 收集创建的节点（从所有节点中过滤）
        /// </summary>
        public ICreateNodeCollect nodeCollect;
    }
}