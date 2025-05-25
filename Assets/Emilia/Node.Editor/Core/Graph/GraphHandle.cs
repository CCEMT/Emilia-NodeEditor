using System;
using System.Collections.Generic;
using Emilia.Kit;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public abstract class GraphHandle
    {
        /// <summary>
        /// 初始化时
        /// </summary>
        public virtual void Initialize(EditorGraphView graphView) { }

        /// <summary>
        /// 初始化自定义模块
        /// </summary>
        public virtual void InitializeCustomModule(EditorGraphView graphView, Dictionary<Type, CustomGraphViewModule> modules) { }

        /// <summary>
        /// 加载前处理
        /// </summary>
        public virtual void OnLoadBefore(EditorGraphView graphView) { }

        /// <summary>
        /// 加载后处理
        /// </summary>
        public virtual void OnLoadAfter(EditorGraphView graphView) { }

        /// <summary>
        /// 进入聚焦时
        /// </summary>
        public virtual void OnEnterFocus(EditorGraphView graphView) { }

        /// <summary>
        /// 聚焦持续时
        /// </summary>
        public virtual void OnFocus(EditorGraphView graphView) { }

        /// <summary>
        /// 退出聚焦时
        /// </summary>
        public virtual void OnExitFocus(EditorGraphView graphView) { }

        /// <summary>
        /// 更新
        /// </summary>
        public virtual void OnUpdate(EditorGraphView graphView) { }

        /// <summary>
        /// 销毁时
        /// </summary>
        /// <param name="graphView"></param>
        public virtual void Dispose(EditorGraphView graphView) { }

        protected void AddModule<TModule>(Dictionary<Type, CustomGraphViewModule> modules) where TModule : CustomGraphViewModule, new()
        {
            modules.Add(typeof(TModule), new TModule());
        }
    }
}