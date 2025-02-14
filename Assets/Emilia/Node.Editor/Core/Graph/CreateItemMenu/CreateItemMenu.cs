using System.Collections.Generic;

namespace Emilia.Node.Editor
{
    public class CreateItemMenu : GraphViewModule
    {
        private ICreateItemMenuHandle handle;
        public override int order => 1400;

        public override void Reset(EditorGraphView graphView)
        {
            base.Reset(graphView);

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