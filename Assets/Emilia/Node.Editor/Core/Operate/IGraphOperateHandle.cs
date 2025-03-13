using UnityEngine;

namespace Emilia.Node.Editor
{
    public interface IGraphOperateHandle : IEditorHandle
    {
        /// <summary>
        /// 打开创建节点菜单
        /// </summary>
        void OpenCreateNodeMenu(Vector2 mousePosition, CreateNodeContext createNodeContext = default);

        /// <summary>
        /// 剪切
        /// </summary>
        void Cut();

        /// <summary>
        /// 拷贝
        /// </summary>
        void Copy();

        /// <summary>
        /// 粘贴
        /// </summary>
        void Paste();

        /// <summary>
        /// 删除
        /// </summary>
        void Delete();

        /// <summary>
        /// 复制
        /// </summary>
        void Duplicate();

        /// <summary>
        /// 保存
        /// </summary>
        void Save();
    }
}