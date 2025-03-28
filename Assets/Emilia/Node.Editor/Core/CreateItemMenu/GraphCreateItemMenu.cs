using System.Collections.Generic;

namespace Emilia.Node.Editor
{
    public class GraphCreateItemMenu : BasicGraphViewModule
    {
        private ICreateItemMenuHandle handle;
        public override int order => 1400;

        public override void Initialize(EditorGraphView graphView)
        {
            base.Initialize(graphView);
            this.handle = EditorHandleUtility.BuildHandle<ICreateItemMenuHandle>(graphView.graphAsset.GetType(), graphView);
        }

        /// <summary>
        /// 收集所有的创建Item菜单
        /// </summary>
        public List<CreateItemMenuInfo> CollectItemMenus()
        {
            List<CreateItemMenuInfo> types = new List<CreateItemMenuInfo>();
            handle.CollectItemMenus(types);
            return types;
        }

        public override void Dispose()
        {
            if (this.handle != null)
            {
                EditorHandleUtility.ReleaseHandle(this.handle);
                this.handle = null;
            }

            base.Dispose();
        }
    }
}