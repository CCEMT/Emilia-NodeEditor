using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace Emilia.Node.Editor
{
    public class GraphSelected
    {
        private EditorGraphView graphView;
        private IGraphSelectedHandle handle;

        private List<IGraphSelectedDrawer> selectedDrawers = new List<IGraphSelectedDrawer>();

        public void Reset(EditorGraphView graphView)
        {
            this.graphView = graphView;

            if (this.handle != null) EditorHandleUtility.ReleaseHandle(this.handle);
            this.handle = EditorHandleUtility.BuildHandle<IGraphSelectedHandle>(graphView.graphAsset.GetType(), graphView);

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

        public void UpdateSelected(List<ISelectable> selection)
        {
            handle?.BeforeUpdateSelected(selection);

            handle?.UpdateSelectedInspector(selection);
            UpdateSelectedDrawer(selection);
            UpdateSelectedElement(selection);

            handle?.AfterUpdateSelected(selection);
        }

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

        public void Dispose()
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

            graphView = null;
        }
    }
}