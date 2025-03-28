using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Emilia.Node.Editor
{
    public class GraphSelected : BasicGraphViewModule
    {
        private IGraphSelectedHandle handle;

        private List<IGraphSelectedDrawer> selectedDrawers = new List<IGraphSelectedDrawer>();
        public override int order => 600;

        public override void Initialize(EditorGraphView graphView)
        {
            base.Initialize(graphView);
            this.handle = EditorHandleUtility.BuildHandle<IGraphSelectedHandle>(graphView.graphAsset.GetType(), graphView);
        }

        public override void AllModuleInitializeSuccess()
        {
            base.AllModuleInitializeSuccess();
            
            int oldAmount = selectedDrawers.Count;
            for (int i = 0; i < oldAmount; i++)
            {
                IGraphSelectedDrawer drawer = selectedDrawers[i];
                drawer.Dispose();
            }

            selectedDrawers.Clear();

            handle?.CollectSelectedDrawer(this.selectedDrawers);

            int newAmount = selectedDrawers.Count;
            for (int i = 0; i < newAmount; i++)
            {
                IGraphSelectedDrawer drawer = selectedDrawers[i];
                drawer.Initialize(graphView);
            }
        }

        /// <summary>
        /// 更新选中
        /// </summary>
        public void UpdateSelected(List<ISelectable> selection)
        {
            handle?.BeforeUpdateSelected(selection);

            handle?.UpdateSelectedInspector(selection);
            UpdateSelectedDrawer(selection);
            UpdateSelectedElement(selection);

            handle?.AfterUpdateSelected(selection);
        }

        /// <summary>
        /// 更新选中的绘制
        /// </summary>
        private void UpdateSelectedDrawer(List<ISelectable> selection)
        {
            int amount = this.selectedDrawers.Count;
            for (int i = 0; i < amount; i++)
            {
                IGraphSelectedDrawer drawer = this.selectedDrawers[i];
                drawer.SelectedUpdate(selection);
            }
        }

        private void UpdateSelectedElement(List<ISelectable> selection)
        {
            int amount = selection.Count;
            for (int i = 0; i < amount; i++)
            {
                ISelectable selectable = selection[i];
                ISelectableGraphElement selectableElement = selectable as ISelectableGraphElement;
                if (selectableElement == null) continue;
                selectableElement.UpdateSelected();
            }
        }

        public override void Dispose()
        {
            int amount = selectedDrawers.Count;
            for (int i = 0; i < amount; i++)
            {
                IGraphSelectedDrawer drawer = selectedDrawers[i];
                drawer.Dispose();
            }

            if (handle != null)
            {
                EditorHandleUtility.ReleaseHandle(handle);
                handle = null;
            }

            base.Dispose();
        }
    }
}