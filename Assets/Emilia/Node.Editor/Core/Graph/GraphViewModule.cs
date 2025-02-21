namespace Emilia.Node.Editor
{
    public abstract class GraphViewModule
    {
        protected EditorGraphView graphView;
        public virtual int order => 0;

        public virtual void Reset(EditorGraphView graphView)
        {
            this.graphView = graphView;
        }

        public virtual void Dispose()
        {
            graphView = null;
        }
    }
}