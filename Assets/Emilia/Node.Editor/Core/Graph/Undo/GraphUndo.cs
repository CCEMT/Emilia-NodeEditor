namespace Emilia.Node.Editor
{
    public class GraphUndo
    {
        private EditorGraphView graphView;

        public void Reset(EditorGraphView graphView)
        {
            this.graphView = graphView;
        }

        public void OnUndoRedoPerformed()
        {
            UndoNode();
            UndoEdge();
            UndoItem();
        }

        private void UndoNode()
        {
            DeleteNode();
            CreateNode();

            int amount = this.graphView.nodeViews.Count;
            for (int i = 0; i < amount; i++)
            {
                IEditorNodeView nodeView = this.graphView.nodeViews[i];
                nodeView.OnValueChanged();
            }
        }

        private void DeleteNode()
        {
            int amount = this.graphView.nodeViews.Count;
            for (int i = 0; i < amount; i++)
            {
                IEditorNodeView nodeView = this.graphView.nodeViews[i];
                bool contains = this.graphView.graphAsset.nodeMap.ContainsKey(nodeView.asset.id);
                if (contains) continue;
                this.graphView.nodeSystem.DeleteNodeNoUndo(nodeView);
            }
        }

        private void CreateNode()
        {
            int amount = this.graphView.graphAsset.nodes.Count;
            for (int i = 0; i < amount; i++)
            {
                EditorNodeAsset node = this.graphView.graphAsset.nodes[i];
                bool contains = this.graphView.graphElementCache.nodeViewById.ContainsKey(node.id);
                if (contains) continue;
                this.graphView.AddNodeView(node);
            }
        }

        private void UndoEdge()
        {
            RemoveEdgeView();
            DeleteEdge();
            CreateEdge();

            int amount = this.graphView.edgeViews.Count;
            for (int i = 0; i < amount; i++)
            {
                IEditorEdgeView edgeView = this.graphView.edgeViews[i];
                edgeView.OnValueChanged();
            }
        }

        private void DeleteEdge()
        {
            int amount = this.graphView.edgeViews.Count;
            for (int i = amount - 1; i >= 0; i--)
            {
                IEditorEdgeView edgeView = this.graphView.edgeViews[i];
                bool contains = this.graphView.graphAsset.edgeMap.ContainsKey(edgeView.asset.id);
                if (contains) continue;
                this.graphView.connectSystem.DisconnectNoUndo(edgeView);
            }
        }

        private void RemoveEdgeView()
        {
            int amount = this.graphView.edgeViews.Count;
            for (int i = amount - 1; i >= 0; i--)
            {
                IEditorEdgeView edgeView = this.graphView.edgeViews[i];

                if (edgeView.inputPortView == null || edgeView.outputPortView == null)
                {
                    edgeView.RemoveView();
                    continue;
                }

                bool nodeConsistency = edgeView.asset.inputNodeId == edgeView.inputPortView.master.asset.id && edgeView.asset.outputNodeId == edgeView.outputPortView.master.asset.id;
                bool portConsistency = edgeView.asset.inputPortId == edgeView.inputPortView.info.id && edgeView.asset.outputPortId == edgeView.outputPortView.info.id;
                if (nodeConsistency == false || portConsistency == false) edgeView.RemoveView();
            }
        }

        private void CreateEdge()
        {
            int amount = this.graphView.graphAsset.edges.Count;
            for (int i = 0; i < amount; i++)
            {
                EditorEdgeAsset edge = this.graphView.graphAsset.edges[i];
                bool contains = this.graphView.graphElementCache.edgeViewById.ContainsKey(edge.id);
                if (contains) continue;
                this.graphView.AddEdgeView(edge);
            }
        }

        private void UndoItem()
        {
            DeleteItem();
            CreateItem();

            int amount = this.graphView.itemViews.Count;
            for (int i = 0; i < amount; i++)
            {
                IEditorItemView itemView = this.graphView.itemViews[i];
                itemView.OnValueChanged();
            }
        }

        private void DeleteItem()
        {
            int amount = this.graphView.itemViews.Count;
            for (int i = 0; i < amount; i++)
            {
                IEditorItemView itemView = this.graphView.itemViews[i];
                bool contains = this.graphView.graphAsset.itemMap.ContainsKey(itemView.asset.id);
                if (contains) continue;
                this.graphView.itemSystem.DeleteItemNoUndo(itemView);
            }
        }

        private void CreateItem()
        {
            int amount = this.graphView.graphAsset.items.Count;
            for (int i = 0; i < amount; i++)
            {
                EditorItemAsset item = this.graphView.graphAsset.items[i];
                bool contains = this.graphView.graphElementCache.itemViewById.ContainsKey(item.id);
                if (contains) continue;
                this.graphView.AddItem(item);
            }
        }

        public void Dispose()
        {
            graphView = default;
        }
    }
}