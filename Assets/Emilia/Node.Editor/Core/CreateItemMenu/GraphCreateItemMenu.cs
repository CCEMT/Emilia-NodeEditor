using System.Collections.Generic;
using Emilia.Kit;

namespace Emilia.Node.Editor
{
    public class GraphCreateItemMenu : BasicGraphViewModule
    {
        private CreateItemMenuHandle handle;
        public override int order => 1400;

        public override void Initialize(EditorGraphView graphView)
        {
            base.Initialize(graphView);
            this.handle = EditorHandleUtility.CreateHandle<CreateItemMenuHandle>(graphView.graphAsset.GetType());
        }

        /// <summary>
        /// 收集所有的创建Item菜单
        /// </summary>
        public List<CreateItemMenuInfo> CollectItemMenus()
        {
            List<CreateItemMenuInfo> types = new List<CreateItemMenuInfo>();
            handle.CollectItemMenus(this.graphView, types);
            return types;
        }

        public override void Dispose()
        {
            this.handle = null;
            base.Dispose();
        }
    }
}