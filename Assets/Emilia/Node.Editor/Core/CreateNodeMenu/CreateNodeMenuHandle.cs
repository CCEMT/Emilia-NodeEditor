using System.Collections.Generic;
using Emilia.Kit;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public abstract class CreateNodeMenuHandle
    {
        /// <summary>
        /// 获取菜单标题
        /// </summary>
        public virtual string GetTitle(EditorGraphView graphView) => "Create Node";

        /// <summary>
        /// 初始化时
        /// </summary>
        public virtual void Initialize(EditorGraphView graphView) { }

        /// <summary>
        /// 初始化缓存构建
        /// </summary>
        public virtual void InitializeCache(EditorGraphView graphView, List<ICreateNodeHandle> createNodeHandles) { }

        /// <summary>
        /// 显示创建节点菜单
        /// </summary>
        public virtual void ShowCreateNodeMenu(EditorGraphView graphView, CreateNodeContext createNodeContext) { }

        /// <summary>
        /// 收集所有创建节点信息
        /// </summary>
        public virtual void CollectAllCreateNodeInfos(EditorGraphView graphView, List<MenuNodeInfo> createNodeInfos, CreateNodeContext createNodeContext) { }

        /// <summary>
        /// 释放时
        /// </summary>
        public virtual void Dispose(EditorGraphView graphView) { }
    }
}