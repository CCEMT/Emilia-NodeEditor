using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Emilia.Node.Editor
{
    public interface IGraphSelectedHandle : IEditorHandle
    {
        /// <summary>
        /// 更新选中的Inspector
        /// </summary>
        void UpdateSelectedInspector(List<ISelectable> selection);

        /// <summary>
        /// 收集选中的Drawer
        /// </summary>
        void CollectSelectedDrawer(List<IGraphSelectedDrawer> drawers);

        /// <summary>
        /// 更新选择前
        /// </summary>
        void BeforeUpdateSelected(List<ISelectable> selection);

        /// <summary>
        /// 更新选择后
        /// </summary>
        void AfterUpdateSelected(List<ISelectable> selection);
    }
}