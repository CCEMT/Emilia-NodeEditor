using Emilia.Kit;
using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public abstract class GraphHotKeysHandle
    {
        public virtual void Initialize(EditorGraphView graphView) { }
        public virtual void OnKeyDown(EditorGraphView graphView, KeyDownEvent evt) { }
        public virtual void Dispose() { }
    }
}