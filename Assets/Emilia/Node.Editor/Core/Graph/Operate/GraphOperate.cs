using UnityEngine;

namespace Emilia.Node.Editor
{
    public class GraphOperate
    {
        private EditorGraphView graphView;
        private IGraphOperateHandle handle;

        public void Reset(EditorGraphView graphView)
        {
            this.graphView = graphView;
            if (handle != null) EditorHandleUtility.ReleaseHandle(handle);
            handle = EditorHandleUtility.BuildHandle<IGraphOperateHandle>(graphView.graphAsset.GetType(), graphView);
        }

        /// <summary>
        /// 打开创建节点菜单
        /// </summary>
        public void OpenCreateNodeMenu(Vector2 mousePosition, CreateNodeContext createNodeContext = default)
        {
            handle?.OpenCreateNodeMenu(mousePosition, createNodeContext);
        }

        /// <summary>
        /// 剪切
        /// </summary>
        public void Cut()
        {
            handle?.Cut();
        }

        /// <summary>
        /// 拷贝
        /// </summary>
        public void Copy()
        {
            handle?.Copy();
        }

        /// <summary>
        /// 粘贴
        /// </summary>
        public void Paste()
        {
            handle?.Paste();
        }

        /// <summary>
        /// 删除
        /// </summary>
        public void Delete()
        {
            handle?.Delete();
        }

        /// <summary>
        /// 复制
        /// </summary>
        public void Duplicate()
        {
            handle?.Duplicate();
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void Save()
        {
            handle?.Save();
        }

        public void Dispose()
        {
            if (handle != null)
            {
                EditorHandleUtility.ReleaseHandle(handle);
                handle = null;
            }

            this.graphView = null;
        }
    }
}