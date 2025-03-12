using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Emilia.Node.Editor
{
    public abstract class GraphSelectedDrawer : IGraphSelectedDrawer
    {
        protected EditorGraphView graphView { get; private set; }

        public virtual void Initialize(EditorGraphView graphView)
        {
            this.graphView = graphView;
        }

        public virtual void SelectedUpdate(List<ISelectable> selection) { }

        public virtual void Dispose() { }
    }
}