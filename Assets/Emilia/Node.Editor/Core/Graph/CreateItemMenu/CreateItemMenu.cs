using System.Collections.Generic;

namespace Emilia.Node.Editor
{
    public class CreateItemMenu
    {
        private EditorGraphView graphView;
        private ICreateItemMenuHandle handle;

        public void Reset(EditorGraphView graphView)
        {
            this.graphView = graphView;

            if (this.handle != null) EditorHandleUtility.ReleaseHandle(this.handle);
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

        public void Dispose()
        {
            if (this.handle != null)
            {
                EditorHandleUtility.ReleaseHandle(this.handle);
                this.handle = null;
            }

            graphView = null;
        }
    }
}