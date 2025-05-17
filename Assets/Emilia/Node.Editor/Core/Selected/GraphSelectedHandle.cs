using System.Collections.Generic;
using Emilia.Kit;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public abstract class GraphSelectedHandle
    {
        public virtual void Initialize(EditorGraphView graphView) { }
        public virtual void UpdateSelectedInspector(EditorGraphView graphView, List<ISelectedHandle> selection) { }
        public virtual void CollectSelectedDrawer(EditorGraphView graphView, List<IGraphSelectedDrawer> drawers) { }
        public virtual void BeforeUpdateSelected(EditorGraphView graphView, List<ISelectedHandle> selection) { }
        public virtual void AfterUpdateSelected(EditorGraphView graphView, List<ISelectedHandle> selection) { }
        public virtual void Dispose(EditorGraphView graphView) { }
    }
}