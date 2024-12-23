using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Emilia.Node.Editor
{
    public interface IGraphSelectedHandle : IEditorHandle
    {
        void UpdateSelectedInspector(List<ISelectable> selection);
        void CollectSelectedDrawer(List<IGraphSelectedDrawer> drawers);
        void BeforeUpdateSelected(List<ISelectable> selection);
        void AfterUpdateSelected(List<ISelectable> selection);
    }
}