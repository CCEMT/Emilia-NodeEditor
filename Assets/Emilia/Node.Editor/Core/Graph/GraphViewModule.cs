namespace Emilia.Node.Editor
{
    public abstract class BasicGraphViewModule : GraphViewModule { }

    public abstract class CustomGraphViewModule : GraphViewModule { }

    public abstract class GraphViewModule
    {
        protected EditorGraphView graphView;

        /// <summary>
        /// 优先级
        /// </summary>
        public virtual int order => 0;

        /// <summary>
        /// 初始化模块
        /// </summary>
        public virtual void Initialize(EditorGraphView graphView)
        {
            this.graphView = graphView;
        }

        /// <summary>
        /// 所有模块初始化成功时调用
        /// </summary>
        public virtual void AllModuleInitializeSuccess() { }

        /// <summary>
        /// 销毁模块
        /// </summary>
        public virtual void Dispose()
        {
            graphView = null;
        }
    }
}