using System;
using System.Collections.Generic;
using Emilia.Kit.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor
{
    public class ConnectSystem : GraphViewModule
    {
        private IConnectSystemHandle handle;
        public EditorEdgeConnectorListener connectorListener { get; private set; }
        public override int order => 1000;

        public override void Reset(EditorGraphView graphView)
        {
            base.Reset(this.graphView);

            if (handle != null) EditorHandleUtility.ReleaseHandle(handle);
            handle = EditorHandleUtility.BuildHandle<IConnectSystemHandle>(graphView.graphAsset.GetType(), graphView);

            if (connectorListener == null && handle != null)
            {
                Type type = handle.connectorListenerType;
                if (type != null)
                {
                    connectorListener = ReflectUtility.CreateInstance(type) as EditorEdgeConnectorListener;
                    connectorListener.Initialize(graphView);
                }
            }
        }

        /// <summary>
        /// 通过端口获取Edge类型
        /// </summary>
        public Type GetEdgeTypeByPort(IEditorPortView portView)
        {
            if (this.handle == null) return null;
            return this.handle.GetEdgeTypeByPort(portView);
        }

        /// <summary>
        /// 是否可以连接
        /// </summary>
        public bool CanConnect(IEditorPortView inputPort, IEditorPortView outputPort)
        {
            if (this.handle == null) return false;
            return this.handle.CanConnect(inputPort, outputPort);
        }

        /// <summary>
        /// 连接两个端口
        /// </summary>
        public IEditorEdgeView Connect(IEditorPortView input, IEditorPortView output)
        {
            if (handle.CanConnect(input, output) == false) return null;
            if (handle.BeforeConnect(input, output)) return null;

            Type edgeType = handle.GetEdgeTypeByPort(input);
            EditorEdgeAsset edge = ScriptableObject.CreateInstance(edgeType) as EditorEdgeAsset;

            edge.id = Guid.NewGuid().ToString();
            edge.inputNodeId = input.master.asset.id;
            edge.outputNodeId = output.master.asset.id;
            edge.inputPortId = input.info.id;
            edge.outputPortId = output.info.id;

            Undo.RegisterCreatedObjectUndo(edge, "Graph Connect");
            this.graphView.RegisterCompleteObjectUndo("Graph Connect");

            IEditorEdgeView edgeView = graphView.AddEdge(edge);
            handle.AfterConnect(edgeView);

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

                List<Object> assets = new List<Object>();
                edge.asset.CollectAsset(assets);

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

                List<Object> assets = new List<Object>();
                edge.asset.CollectAsset(assets);

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
            if (handle != null)
            {
                EditorHandleUtility.ReleaseHandle(handle);
                handle = null;
            }

            base.Dispose();
        }
    }
}