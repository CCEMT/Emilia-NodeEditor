namespace Emilia.Node.Editor
{
    public abstract class BasicGraphViewModule : GraphViewModule { }

    public abstract class CustomGraphViewModule : GraphViewModule { }

    public abstract class GraphViewModule
    {
        protected EditorGraphView graphView;
        public virtual int order => 0;

        public virtual void Initialize(EditorGraphView graphView)
        {
            this.graphView = graphView;
        }

        public virtual void AllModuleInitializeSuccess() { }

        public virtual void Dispose()
        {
            graphView = null;
        }
    }
}