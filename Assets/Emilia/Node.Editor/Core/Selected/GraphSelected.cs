using System;
using System.Collections.Generic;
using Emilia.Kit;

namespace Emilia.Node.Editor
{
    public class GraphSelected : BasicGraphViewModule
    {
        private IGraphSelectedHandle handle;

        private List<ISelectedHandle> _selected = new List<ISelectedHandle>();
        private List<IGraphSelectedDrawer> selectedDrawers = new List<IGraphSelectedDrawer>();

        public override int order => 600;
        public IReadOnlyList<ISelectedHandle> selected => this._selected;

        public event Action<IReadOnlyList<ISelectedHandle>> onSelectedChanged;

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
        public void UpdateSelected(List<ISelectedHandle> selection)
        {
            handle?.BeforeUpdateSelected(this._selected);

            UnSelected(this._selected);

            this._selected.Clear();
            this._selected.AddRange(selection);

            handle?.UpdateSelectedInspector(_selected);
            UpdateSelectedDrawer(_selected);

            Selected(this._selected);

            handle?.AfterUpdateSelected(_selected);

            onSelectedChanged?.Invoke(_selected);
        }

        private void Selected(List<ISelectedHandle> selectables)
        {
            int amount = selectables.Count;
            for (int i = 0; i < amount; i++)
            {
                ISelectedHandle selectable = selectables[i];
                if (selectable == null) continue;
                selectable.Select();
            }
        }

        private void UnSelected(List<ISelectedHandle> selection)
        {
            int amount = selection.Count;
            for (int i = 0; i < amount; i++)
            {
                ISelectedHandle selectable = selection[i];
                if (selectable == null) continue;
                selectable.Unselect();
            }
        }

        public void UpdateSelected()
        {
            UpdateSelected(this._selected);
        }

        /// <summary>
        /// 更新选中的绘制
        /// </summary>
        private void UpdateSelectedDrawer(List<ISelectedHandle> selection)
        {
            int amount = this.selectedDrawers.Count;
            for (int i = 0; i < amount; i++)
            {
                IGraphSelectedDrawer drawer = this.selectedDrawers[i];
                drawer.SelectedUpdate(selection);
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