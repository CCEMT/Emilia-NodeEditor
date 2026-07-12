using System;
using Emilia.Kit.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Emilia.Node.Editor
{
    public partial class EditorGraphView
    {
        /// <summary>
        /// 添加IEditorNodeView并添加到Asset中
        /// </summary>
        public IEditorNodeView AddNode(EditorNodeAsset nodeAsset)
        {
            graphAsset.AddNode(nodeAsset);
            IEditorNodeView nodeView = AddNodeView(nodeAsset);
            return nodeView;
        }

        /// <summary>
        /// 添加IEditorNodeView
        /// </summary>
        public IEditorNodeView AddNodeView(EditorNodeAsset nodeAsset)
        {
            Type nodeViewType = GraphTypeCache.GetNodeViewType(nodeAsset.GetType());
            if (nodeViewType == null)
            {
                Debug.LogError($"AddNodeView {nodeAsset.GetType()}找不到IEditorNodeView，请在找不到IEditorNodeView使用EditorNodeAttribute指定");
                return null;
            }

            IEditorNodeView nodeView = null;

            try
            {
                nodeView = ReflectUtility.CreateInstance(nodeViewType) as IEditorNodeView;
                nodeView.Initialize(this, nodeAsset);
            }
            catch (Exception e)
            {
                Debug.LogError($"AddNodeView {nodeViewType.FullName} 创建失败 {e}");
                return null;
            }

            AddElement(nodeView.element);

            this._nodeViews.Add(nodeView);
            graphElementCache.SetNodeViewCache(nodeAsset.id, nodeView);
            return nodeView;
        }

        /// <summary>
        /// 移除IEditorNodeView
        /// </summary>
        public void RemoveNodeView(IEditorNodeView nodeView)
        {
            if (nodeView == null)
            {
                Debug.LogError("RemoveNodeView nodeView 为空");
                return;
            }

            if (nodeView.asset != null) graphElementCache.RemoveNodeViewCache(nodeView.asset.id);

            try
            {
                nodeView.Dispose();
            }
            catch (Exception e)
            {
                Debug.LogError($"RemoveNodeView nodeView 异常 {e}");
            }

            if (nodeView.element != null) RemoveElement(nodeView.element);
            this._nodeViews.Remove(nodeView);
        }

        /// <summary>
        /// 添加IEditorEdgeView并添加到Asset中
        /// </summary>
        public IEditorEdgeView AddEdge(EditorEdgeAsset asset)
        {
            graphAsset.AddEdge(asset);
            IEditorEdgeView edgeView = AddEdgeView(asset);
            return edgeView;
        }

        /// <summary>
        /// 添加IEditorEdgeView
        /// </summary>
        public IEditorEdgeView AddEdgeView(EditorEdgeAsset asset)
        {
            IEditorNodeView inputNode = graphElementCache.GetEditorNodeView(asset.inputNodeId);
            IEditorNodeView outputNode = graphElementCache.GetEditorNodeView(asset.outputNodeId);

            if (inputNode == null || outputNode == null) return null;

            IEditorPortView inputPort = inputNode.GetPortView(asset.inputPortId);
            IEditorPortView outputPort = outputNode.GetPortView(asset.outputPortId);

            if (inputPort == null || outputPort == null) return null;

            Type edgeViewType = GraphTypeCache.GetEdgeViewType(asset.GetType());
            if (edgeViewType == null)
            {
                Debug.LogError($"AddEdgeView时 {asset.GetType()}找不到IEditorEdgeView，请在找不到IEditorEdgeView使用EditorEdgeAttribute指定");
                return null;
            }

            IEditorEdgeView edgeView;

            try
            {
                edgeView = ReflectUtility.CreateInstance(edgeViewType) as IEditorEdgeView;
                edgeView.Initialize(this, asset);
            }
            catch (Exception e)
            {
                Debug.LogError($"AddEdgeView {edgeViewType.FullName} 创建失败 {e}");
                return null;
            }

            AddElement(edgeView.edgeElement);

            this._edgeViews.Add(edgeView);
            graphElementCache.SetEdgeViewCache(asset.id, edgeView);
            return edgeView;
        }

        /// <summary>
        /// 移除IEditorEdgeView
        /// </summary>
        public void RemoveEdgeView(IEditorEdgeView edge)
        {
            if (edge == null)
            {
                Debug.LogError("RemoveEdgeView edge 为空");
                return;
            }

            if (edge.asset != null) graphElementCache.RemoveEdgeViewCache(edge.asset.id);

            try
            {
                edge.Dispose();
            }
            catch (Exception e)
            {
                Debug.LogError($"RemoveEdgeView edge 异常 {e}");
            }

            if (edge.edgeElement != null)
            {
                RemoveElement(edge.edgeElement);
                edge.inputPortView?.portElement.Disconnect(edge.edgeElement);
                edge.outputPortView?.portElement.Disconnect(edge.edgeElement);
            }

            this._edgeViews.Remove(edge);
        }

        /// <summary>
        /// 添加IEditorItemView并添加到Asset中
        /// </summary>
        public IEditorItemView AddItem(EditorItemAsset asset)
        {
            graphAsset.AddItem(asset);
            IEditorItemView itemView = AddItemView(asset);
            return itemView;
        }

        /// <summary>
        /// 添加IEditorItemView
        /// </summary>
        public IEditorItemView AddItemView(EditorItemAsset asset)
        {
            Type itemViewType = GraphTypeCache.GetItemViewType(asset.GetType());
            if (itemViewType == null)
            {
                Debug.LogError($"AddNodeView {asset.GetType()}找不到IEditorItemView，请在找不到IEditorItemView使用EditorItemAttribute指定");
                return null;
            }

            IEditorItemView itemView;
            try
            {
                itemView = ReflectUtility.CreateInstance(itemViewType) as IEditorItemView;
                itemView.Initialize(this, asset);
            }
            catch (Exception e)
            {
                Debug.LogError($"AddItemView {itemViewType.FullName} 创建失败 {e}");
                return null;
            }

            if (itemView.element == null)
            {
                Debug.LogError($"AddItemView {itemViewType.FullName} element 为空");
                return null;
            }

            AddElement(itemView.element);

            this._itemViews.Add(itemView);
            graphElementCache.SetItemViewCache(asset.id, itemView);
            return itemView;
        }

        /// <summary>
        /// 移除IEditorItemView
        /// </summary>
        public void RemoveItemView(IEditorItemView item)
        {
            if (item == null)
            {
                Debug.LogError("RemoveItemView item 为空");
                return;
            }
            if (item.asset != null) graphElementCache.RemoveItemViewCache(item.asset.id);

            try
            {
                item.Dispose();
            }
            catch (Exception e)
            {
                Debug.LogError($"RemoveItemView item 异常 {e}");
            }

            if (item.element != null) RemoveElement(item.element);
            this._itemViews.Remove(item);
        }

        private void RemoveAllElement()
        {
            foreach (GraphElement graphElement in graphElements)
            {
                IRemoveViewElement removeViewElement = graphElement as IRemoveViewElement;
                removeViewElement?.RemoveView();
            }
        }
    }
}
