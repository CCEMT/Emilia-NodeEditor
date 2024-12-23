using System;
using System.Collections.Generic;
using Emilia.Kit.Editor;
using UnityEngine;

namespace Emilia.Node.Editor
{
    public class GraphElementCache
    {
        private EditorGraphView editorGraphView;

        private Dictionary<string, IEditorNodeView> _nodeViewById = new Dictionary<string, IEditorNodeView>();
        private Dictionary<string, IEditorEdgeView> _edgeViewById = new Dictionary<string, IEditorEdgeView>();
        private Dictionary<string, IEditorItemView> _itemViewById = new Dictionary<string, IEditorItemView>();

        private List<NodeCache> _nodeViewCache = new List<NodeCache>();

        public IReadOnlyDictionary<string, IEditorNodeView> nodeViewById => this._nodeViewById;
        public IReadOnlyDictionary<string, IEditorEdgeView> edgeViewById => this._edgeViewById;
        public IReadOnlyDictionary<string, IEditorItemView> itemViewById => this._itemViewById;

        public void BuildCache(EditorGraphView graphView)
        {
            this.editorGraphView = graphView;

            this._nodeViewById.Clear();
            this._edgeViewById.Clear();
            this._nodeViewCache.Clear();

            int amount = this.editorGraphView.createNodeMenu.createNodeHandleCacheList.Count;
            for (int i = 0; i < amount; i++)
            {
                ICreateNodeHandle createNodeHandle = this.editorGraphView.createNodeMenu.createNodeHandleCacheList[i];

                EditorNodeAsset nodeAsset = this.editorGraphView.nodeSystem.CreateNode(createNodeHandle.editorNodeType, Vector2.zero);

                object userData = createNodeHandle.userData;
                nodeAsset.userData = this.editorGraphView.graphCopyPaste.CreateCopy(userData);

                Type nodeViewType = GraphTypeCache.GetNodeViewType(nodeAsset.GetType());
                IEditorNodeView nodeView = ReflectUtility.CreateInstance(nodeViewType) as IEditorNodeView;
                nodeView.Initialize(this.editorGraphView, nodeAsset);

                NodeCache nodeCache = new NodeCache(userData, nodeView);
                this._nodeViewCache.Add(nodeCache);
            }
        }

        public void SetNodeViewCache(string id, IEditorNodeView nodeView)
        {
            this._nodeViewById[id] = nodeView;
        }

        public void SetEdgeViewCache(string id, IEditorEdgeView edgeView)
        {
            this._edgeViewById[id] = edgeView;
        }

        public void SetItemViewCache(string id, IEditorItemView itemView)
        {
            this._itemViewById[id] = itemView;
        }

        public void RemoveNodeViewCache(string id)
        {
            if (this._nodeViewById.ContainsKey(id)) this._nodeViewById.Remove(id);
        }

        public void RemoveEdgeViewCache(string id)
        {
            if (_edgeViewById.ContainsKey(id)) this._edgeViewById.Remove(id);
        }

        public void RemoveItemViewCache(string id)
        {
            if (this._itemViewById.ContainsKey(id)) this._itemViewById.Remove(id);
        }

        public IEditorNodeView GetEditorNodeView(string id)
        {
            return nodeViewById.GetValueOrDefault(id);
        }

        public List<PortInfo> GetUserDataTypeByPortType(IEditorPortView form)
        {
            List<PortInfo> userDataTypeList = new List<PortInfo>();

            int nodeViewAmount = this._nodeViewCache.Count;
            for (int i = 0; i < nodeViewAmount; i++)
            {
                NodeCache nodeCache = this._nodeViewCache[i];

                int portViewAmount = nodeCache.nodeView.portViews.Count;
                for (var j = 0; j < portViewAmount; j++)
                {
                    IEditorPortView portView = nodeCache.nodeView.portViews[j];
                    bool canConnect = this.editorGraphView.connectSystem.CanConnect(form, portView);
                    if (canConnect == false) continue;
                    PortInfo portInfo = new PortInfo();
                    portInfo.nodeAssetType = nodeCache.nodeView.asset.GetType();
                    portInfo.userData = nodeCache.userData;
                    portInfo.portId = portView.info.id;
                    portInfo.displayName = portView.info.displayName;
                    userDataTypeList.Add(portInfo);
                }
            }

            return userDataTypeList;
        }

        public IEditorEdgeView GetEdgeView(IEditorPortView xPort, IEditorPortView yPort)
        {
            int edgeAmount = this.editorGraphView.edgeViews.Count;
            for (int i = 0; i < edgeAmount; i++)
            {
                IEditorEdgeView edge = this.editorGraphView.edgeViews[i];
                bool hasInputNode = edge.inputPortView.master.asset.id == xPort.master.asset.id;
                bool hasOutputNode = edge.outputPortView.master.asset.id == yPort.master.asset.id;
                bool hasInputPort = edge.inputPortView.info.id == xPort.info.id;
                bool hasOutputPort = edge.outputPortView.info.id == yPort.info.id;
                if (hasInputNode && hasOutputNode && hasInputPort && hasOutputPort) return edge;

                hasInputNode = edge.inputPortView.master.asset.id == yPort.master.asset.id;
                hasOutputNode = edge.outputPortView.master.asset.id == xPort.master.asset.id;
                hasInputPort = edge.inputPortView.info.id == yPort.info.id;
                hasOutputPort = edge.outputPortView.info.id == xPort.info.id;
                if (hasInputNode && hasOutputNode && hasInputPort && hasOutputPort) return edge;
            }

            return default;
        }

        public void Dispose()
        {
            this.editorGraphView = null;
            this._nodeViewById.Clear();
            this._edgeViewById.Clear();
            this._itemViewById.Clear();
            this._nodeViewCache.Clear();
        }
    }
}