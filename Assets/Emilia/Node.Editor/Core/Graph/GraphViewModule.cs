namespace Emilia.Node.Editor
{
    public abstract class GraphViewModule
    {
        protected EditorGraphView graphView;
        public abstract int order { get; }

        public virtual void Reset(EditorGraphView graphView)
        {
            this.graphView = graphView;
        }

        public virtual void Dispose()
        {
            graphView = default;
        }
    }
}