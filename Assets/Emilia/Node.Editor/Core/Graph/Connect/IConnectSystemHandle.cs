using System;

namespace Emilia.Node.Editor
{
    public interface IConnectSystemHandle : IEditorHandle
    {
        /// <summary>
        /// IEdgeConnectorListener 的Type
        /// </summary>
        Type connectorListenerType { get; }

        /// <summary>
        /// 通过Port获取Edge的类型
        /// </summary>
        Type GetEdgeTypeByPort(IEditorPortView portView);

        /// <summary>
        /// 是否可以连接
        /// </summary>
        bool CanConnect(IEditorPortView inputPort, IEditorPortView outputPort);

        /// <summary>
        /// 连接之前
        /// </summary>
        bool BeforeConnect(IEditorPortView input, IEditorPortView output);

        /// <summary>
        /// 连接之后
        /// </summary>
        void AfterConnect(IEditorEdgeView edgeView);
    }
}