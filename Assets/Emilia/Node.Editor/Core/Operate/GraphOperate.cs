using UnityEngine;

namespace Emilia.Node.Editor
{
    public class GraphOperate : BasicGraphViewModule
    {
        private IGraphOperateHandle handle;
        public override int order => 200;

        public override void Initialize(EditorGraphView graphView)
        {
            base.Initialize(graphView);
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
        public void Paste(Vector2? mousePosition = null)
        {
            handle?.Paste(mousePosition);
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

        public override void Dispose()
        {
            if (handle != null)
            {
                EditorHandleUtility.ReleaseHandle(handle);
                handle = null;
            }

            base.Dispose();
        }
    }
}