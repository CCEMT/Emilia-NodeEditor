using System.Collections.Generic;
using Emilia.Kit;

namespace Emilia.Node.Editor
{
    public interface IGraphSelectedDrawer
    {
        void Initialize(EditorGraphView graphView);

        void SelectedUpdate(List<ISelectedHandle> selection);

        void Dispose();
    }
}