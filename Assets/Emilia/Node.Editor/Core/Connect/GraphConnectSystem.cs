using System;
using System.Collections.Generic;
using Emilia.Kit;
using Emilia.Kit.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor
{
    public class GraphConnectSystem : BasicGraphViewModule
    {
        private ConnectSystemHandle handle;
        public EditorEdgeConnectorListener connectorListener { get; private set; }
        public override int order => 1000;

        public override void Initialize(EditorGraphView graphView)
        {
            base.Initialize(graphView);
            handle = EditorHandleUtility.CreateHandle<ConnectSystemHandle>(graphView.graphAsset.GetType());
        }

        public override void AllModuleInitializeSuccess()
        {
            base.AllModuleInitializeSuccess();
            if (this.handle == null) return;

            Type type = handle.GetConnectorListenerType(this.graphView);
            if (type == null) return;

            connectorListener = ReflectUtility.CreateInstance(type) as EditorEdgeConnectorListener;
            connectorListener.Initialize(this.graphView);
        }

        /// <summary>
        /// 通过端口获取Edge类型
        /// </summary>
        public Type GetEdgeTypeByPort(IEditorPortView portView)
        {
            if (this.handle == null) return null;
            return this.handle.GetEdgeTypeByPort(graphView, portView);
        }

        /// <summary>
        /// 是否可以连接
        /// </summary>
        public bool CanConnect(IEditorPortView inputPort, IEditorPortView outputPort)
        {
            if (this.handle == null) return false;
            return this.handle.CanConnect(graphView, inputPort, outputPort);
        }

        /// <summary>
        /// 连接两个端口
        /// </summary>
        public IEditorEdgeView Connect(IEditorPortView input, IEditorPortView output)
        {
            if (handle.CanConnect(graphView, input, output) == false) return null;
            if (handle.BeforeConnect(graphView, input, output)) return null;

            Type edgeType = handle.GetEdgeTypeByPort(graphView, input);
            EditorEdgeAsset edge = ScriptableObject.CreateInstance(edgeType) as EditorEdgeAsset;

            edge.id = Guid.NewGuid().ToString();
            edge.inputNodeId = input.master.asset.id;
            edge.outputNodeId = output.master.asset.id;
            edge.inputPortId = input.info.id;
            edge.outputPortId = output.info.id;

            Undo.RegisterCreatedObjectUndo(edge, "Graph Connect");
            this.graphView.RegisterCompleteObjectUndo("Graph Connect");

            IEditorEdgeView edgeView = graphView.AddEdge(edge);
            handle.AfterConnect(graphView, edgeView);

            return edgeView;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect(IEditorEdgeView edge)
        {
            edge.RemoveView();

            if (edge.asset != null && string.IsNullOrEmpty(edge.asset.id) == false)
            {
                this.graphView.RegisterCompleteObjectUndo("Graph Disconnect");

                this.graphView.graphAsset.RemoveEdge(edge.asset);

                List<Object> assets = edge.asset.CollectAsset();

                int amount = assets.Count;
                for (int i = 0; i < amount; i++)
                {
                    Object asset = assets[i];
                    Undo.DestroyObjectImmediate(asset);
                }
            }
        }

        /// <summary>
        /// 断开连接，不记录Undo
        /// </summary>
        public void DisconnectNoUndo(IEditorEdgeView edge)
        {
            edge.RemoveView();

            if (edge.asset != null && string.IsNullOrEmpty(edge.asset.id) == false)
            {
                this.graphView.graphAsset.RemoveEdge(edge.asset);

                List<Object> assets = edge.asset.CollectAsset();

                int amount = assets.Count;
                for (int i = 0; i < amount; i++)
                {
                    Object asset = assets[i];
                    Object.DestroyImmediate(asset, true);
                }
            }
        }

        public override void Dispose()
        {
            handle = null;
            connectorListener = null;
            base.Dispose();
        }
    }
}