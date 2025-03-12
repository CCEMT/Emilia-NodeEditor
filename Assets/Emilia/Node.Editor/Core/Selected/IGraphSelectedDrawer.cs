using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Emilia.Node.Editor
{
    public interface IGraphSelectedDrawer
    {
        void Initialize(EditorGraphView graphView);

        void SelectedUpdate(List<ISelectable> selection);

        void Dispose();
    }
}