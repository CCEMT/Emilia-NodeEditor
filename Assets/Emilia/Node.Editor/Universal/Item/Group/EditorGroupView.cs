using System.Collections.Generic;
using System.Linq;
using Emilia.Kit;
using Emilia.Node.Attributes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    [EditorItem(typeof(EditorGroupAsset))]
    public class EditorGroupView : Group, IEditorItemView, IResizedGraphElement
    {
        private EditorGraphView graphView;
        private EditorGroupAsset groupAsset;

        private Dictionary<string, ContextualMenuManipulator> menuManipulators = new Dictionary<string, ContextualMenuManipulator>();

        private bool isUndo;

        public EditorItemAsset asset => groupAsset;
        public GraphElement element => this;

        public virtual Color groupColor => new Color(0, 0, 0, 0.3f);

        public virtual void Initialize(EditorGraphView graphView, EditorItemAsset asset)
        {
            this.graphView = graphView;
            this.groupAsset = asset as EditorGroupAsset;

            title = this.groupAsset.groupTitle;

            UpdateGroupColor(groupColor);

            headerContainer.Q<TextField>().RegisterCallback<ChangeEvent<string>>((text) => this.groupAsset.groupTitle = text.newValue);

            isUndo = false;
            InitializeInnerNodes();
            isUndo = true;

            SetPositionNoUndo(asset.position);
        }

        private void InitializeInnerNodes()
        {
            int amount = this.groupAsset.innerNodes.Count;
            for (int i = 0; i < amount; i++)
            {
                string nodeId = this.groupAsset.innerNodes[i];

                EditorNodeView nodeView = this.graphView.graphElementCache.nodeViewById.GetValueOrDefault(nodeId) as EditorNodeView;
                if (nodeView == default)
                {
                    groupAsset.innerNodes.RemoveAt(i);
                    i--;
                    continue;
                }

                AddElement(nodeView);
            }
        }

        private void GroupMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("移除节点", RemoveSelectedNode);
        }

        private void RemoveSelectedNode(DropdownMenuAction dropdownMenuAction)
        {
            Undo.IncrementCurrentGroup();

            int amount = this.graphView.selection.Count;
            for (var i = 0; i < amount; i++)
            {
                ISelectable selectable = this.graphView.selection[i];
                EditorNodeView editorNodeView = selectable as EditorNodeView;
                if (editorNodeView == null) continue;
                string id = editorNodeView.asset.id;
                if (this.groupAsset.innerNodes.Contains(id) == false) continue;

                RemoveElement(editorNodeView);

                ContextualMenuManipulator contextualMenuManipulator = menuManipulators.GetValueOrDefault(id);
                if (contextualMenuManipulator != null) editorNodeView.RemoveManipulator(contextualMenuManipulator);
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            Undo.IncrementCurrentGroup();
        }

        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            base.OnElementsAdded(elements);

            foreach (GraphElement graphElement in elements)
            {
                EditorNodeView nodeView = graphElement as EditorNodeView;
                if (nodeView == default) continue;

                ContextualMenuManipulator manipulator = new ContextualMenuManipulator(GroupMenu);
                nodeView.AddManipulator(manipulator);
                menuManipulators[nodeView.asset.id] = manipulator;

                if (this.groupAsset.innerNodes.Contains(nodeView.asset.id)) continue;

                if (isUndo) this.graphView.RegisterCompleteObjectUndo("Graph GroupAddNode");
                this.groupAsset.innerNodes.Add(nodeView.asset.id);
            }
        }

        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            base.OnElementsRemoved(elements);

            if (parent == null) return;

            foreach (GraphElement graphElement in elements)
            {
                EditorNodeView nodeView = graphElement as EditorNodeView;
                if (nodeView == default) continue;

                string id = nodeView.asset.id;

                if (isUndo) this.graphView.RegisterCompleteObjectUndo("Graph GroupRemoveNode");
                this.groupAsset.innerNodes.Remove(id);

                ContextualMenuManipulator contextualMenuManipulator = this.menuManipulators.GetValueOrDefault(id);
                if (contextualMenuManipulator != null) nodeView.RemoveManipulator(contextualMenuManipulator);
            }
        }

        public virtual void UpdateGroupColor(Color newColor)
        {
            style.backgroundColor = newColor;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Undo.RegisterCompleteObjectUndo(asset, "Graph MoveGroup");
            this.groupAsset.position = newPos;
        }

        public void SetPositionNoUndo(Rect position)
        {
            base.SetPosition(position);
            asset.position = position;
        }

        public virtual void OnValueChanged()
        {
            title = groupAsset.groupTitle;
            this.isUndo = false;

            SetPositionNoUndo(asset.position);
            RemoveNode();
            AddNode();

            this.isUndo = true;
        }

        private void RemoveNode()
        {
            foreach (GraphElement graphElement in containedElements.ToList())
            {
                EditorNodeView nodeView = graphElement as EditorNodeView;
                if (nodeView == default) continue;

                bool contains = this.groupAsset.innerNodes.Contains(nodeView.asset.id);
                if (contains) continue;
                RemoveElement(nodeView);
            }
        }

        private void AddNode()
        {
            foreach (string nodeId in this.groupAsset.innerNodes.ToList())
            {
                EditorNodeView nodeView = this.graphView.graphElementCache.nodeViewById.GetValueOrDefault(nodeId) as EditorNodeView;
                if (nodeView == default) continue;

                bool contains = this.containedElements.Contains(nodeView);
                if (contains) continue;
                AddElement(nodeView);
            }
        }

        public virtual ICopyPastePack GetPack()
        {
            return new GroupCopyPastePack(asset);
        }

        public virtual void OnElementResized()
        {
            groupAsset.size = GetPosition().size;
        }

        public virtual IEnumerable<Object> CollectSelectedObjects()
        {
            yield return asset;
        }

        public virtual void UpdateSelected() { }

        public virtual void Delete()
        {
            this.graphView.itemSystem.DeleteItem(this);
        }

        public virtual void RemoveView()
        {
            graphView.RemoveItemView(this);
        }

        public virtual void Dispose()
        {
            graphView = null;
        }
    }
}