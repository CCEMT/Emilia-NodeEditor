using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Emilia.Node.Editor
{
    public interface ICreateNodeMenuHandle : IEditorHandle
    {
        /// <summary>
        /// 菜单标题
        /// </summary>
        string title { get; }

        /// <summary>
        /// 初始化缓存
        /// </summary>
        void InitializeCache();

        /// <summary>
        /// 初始化创建节点菜单
        /// </summary>
        void MenuCreateInitialize(CreateNodeContext createNodeContext);

        /// <summary>
        /// 显示创建节点菜单
        /// </summary>
        void ShowCreateNodeMenu(NodeCreationContext nodeCreationContext);

        /// <summary>
        /// 收集所有创建节点信息
        /// </summary>
        void CollectAllCreateNodeInfos(List<CreateNodeInfo> createNodeInfos, CreateNodeContext createNodeContext);
    }
}