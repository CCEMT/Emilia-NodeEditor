using Emilia.Kit;
using UnityEngine;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public abstract class GraphOperateHandle
    {
        public virtual void OpenCreateNodeMenu(EditorGraphView graphView, Vector2 mousePosition, CreateNodeContext createNodeContext = default) { }

        public virtual void Cut(EditorGraphView graphView) { }

        public virtual void Copy(EditorGraphView graphView) { }

        public virtual void Paste(EditorGraphView graphView, Vector2? mousePosition = null) { }

        public virtual void Delete(EditorGraphView graphView) { }

        public virtual void Duplicate(EditorGraphView graphView) { }

        public virtual void Save(EditorGraphView graphView) { }
    }
}