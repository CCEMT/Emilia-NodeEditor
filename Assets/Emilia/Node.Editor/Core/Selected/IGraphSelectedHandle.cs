using System.Collections.Generic;
using Emilia.Kit;
using UnityEditor.Experimental.GraphView;

namespace Emilia.Node.Editor
{
    public interface IGraphSelectedHandle : IEditorHandle
    {
        /// <summary>
        /// 更新选中的Inspector
        /// </summary>
        void UpdateSelectedInspector(List<ISelectedHandle> selection);

        /// <summary>
        /// 收集选中的Drawer
        /// </summary>
        void CollectSelectedDrawer(List<IGraphSelectedDrawer> drawers);

        /// <summary>
        /// 更新选择前
        /// </summary>
        void BeforeUpdateSelected(List<ISelectedHandle> selection);

        /// <summary>
        /// 更新选择后
        /// </summary>
        void AfterUpdateSelected(List<ISelectedHandle> selection);
    }
}