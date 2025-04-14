using System.Collections.Generic;
using System.Linq;
using Emilia.Kit.Editor;
using Emilia.Node.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    public class NodeInsertDragger : MouseManipulator
    {
        private bool isActive;

        private IEditorEdgeView ghostEdgeInput;
        private IEditorEdgeView ghostEdgeOutput;

        private IEditorEdgeView targetEdgeView;
        private IEditorPortView inputPortView;
        private IEditorPortView outputPortView;

        protected override void RegisterCallbacksOnTarget()
        {
            IEditorNodeView nodeView = target as IEditorNodeView;
            if (nodeView == null) return;

            nodeView.element.RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
            nodeView.graphView.RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
            nodeView.graphView.RegisterCallback<MouseUpEvent>(OnMouseUpEvent);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            IEditorNodeView nodeView = target as IEditorNodeView;
            if (nodeView == null) return;

            nodeView.element.UnregisterCallback<MouseDownEvent>(OnMouseDownEvent);
            nodeView.graphView.UnregisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
            nodeView.graphView.UnregisterCallback<MouseUpEvent>(OnMouseUpEvent);
        }

        private void OnMouseDownEvent(MouseDownEvent evt)
        {
            IEditorNodeView nodeView = target as IEditorNodeView;

            this.isActive = true;

            int portAmount = nodeView.portViews.Count;
            for (int i = 0; i < portAmount; i++)
            {
                IEditorPortView portView = nodeView.portViews[i];

                List<IEditorEdgeView> edges = portView.GetEdges();
                if (edges.Count > 0) isActive = false;
            }
        }

        private void OnMouseMoveEvent(MouseMoveEvent evt)
        {
            if (isActive == false) return;

            IEditorNodeView nodeView = target as IEditorNodeView;

            Rect nodeRect = nodeView.asset.position;

            List<IEditorEdgeView> edgeViews = nodeView.graphView.graphElements.OfType<IEditorEdgeView>().Where((edge) => {
                if (edge.edgeElement.isGhostEdge) return false;
                if (edge.inputPortView.master == nodeView || edge.outputPortView.master == nodeView) return false;

                Rect edgeRect = edge.edgeElement.edgeControl.layout;
                return edgeRect.Overlaps(nodeRect);
            }).ToList();

            if (edgeViews.Count != 0)
            {
                IEditorEdgeView edgeView = edgeViews.FirstOrDefault();

                List<IEditorPortView> canConnectInput = new List<IEditorPortView>();
                List<IEditorPortView> canConnectOutput = new List<IEditorPortView>();

                int portAmount = nodeView.portViews.Count;
                for (int i = 0; i < portAmount; i++)
                {
                    IEditorPortView portView = nodeView.portViews[i];

                    if (portView.portDirection == EditorPortDirection.Input)
                    {
                        bool canConnect = nodeView.graphView.connectSystem.CanConnect(portView, edgeView.outputPortView);
                        if (canConnect) canConnectInput.Add(portView);
                    }
                    else
                    {
                        bool canConnect = nodeView.graphView.connectSystem.CanConnect(edgeView.inputPortView, portView);
                        if (canConnect) canConnectOutput.Add(portView);
                    }
                }

                if (canConnectInput.Count > 0 && canConnectOutput.Count > 0)
                {
                    if (ghostEdgeInput == null)
                    {
                        ghostEdgeInput = ReflectUtility.CreateInstance(edgeView.GetType()) as IEditorEdgeView;
                        ghostEdgeInput.edgeElement.isGhostEdge = true;
                        ghostEdgeInput.edgeElement.pickingMode = PickingMode.Ignore;
                        nodeView.graphView.AddElement(this.ghostEdgeInput.edgeElement);
                    }

                    if (ghostEdgeOutput == null)
                    {
                        ghostEdgeOutput = ReflectUtility.CreateInstance(edgeView.GetType()) as IEditorEdgeView;
                        ghostEdgeOutput.edgeElement.isGhostEdge = true;
                        ghostEdgeOutput.edgeElement.pickingMode = PickingMode.Ignore;
                        nodeView.graphView.AddElement(this.ghostEdgeOutput.edgeElement);
                    }

                    targetEdgeView = edgeView;
                    inputPortView = canConnectInput.FirstOrDefault();
                    outputPortView = canConnectOutput.FirstOrDefault();

                    ghostEdgeInput.inputPortView = this.inputPortView;
                    ghostEdgeInput.outputPortView = targetEdgeView.outputPortView;

                    ghostEdgeOutput.inputPortView = this.targetEdgeView.inputPortView;
                    ghostEdgeOutput.outputPortView = this.outputPortView;

                    this.inputPortView.portElement.portCapLit = true;
                    outputPortView.portElement.portCapLit = true;

                    targetEdgeView.outputPortView.portElement.portCapLit = true;
                    this.targetEdgeView.inputPortView.portElement.portCapLit = true;

                    return;
                }
            }

            RemoveGhostEdge();

            targetEdgeView = null;
            inputPortView = null;
            outputPortView = null;
        }

        private void OnMouseUpEvent(MouseUpEvent evt)
        {
            if (isActive == false) return;

            if (this.targetEdgeView != null)
            {
                IEditorNodeView nodeView = target as IEditorNodeView;
                nodeView.graphView.connectSystem.Connect(inputPortView, targetEdgeView.outputPortView);
                nodeView.graphView.connectSystem.Connect(targetEdgeView.inputPortView, outputPortView);

                targetEdgeView.Delete();
            }

            RemoveGhostEdge();

            targetEdgeView = null;
            inputPortView = null;
            outputPortView = null;

            isActive = false;
        }

        private void RemoveGhostEdge()
        {
            if (this.ghostEdgeInput != null)
            {
                if (ghostEdgeInput.inputPortView != null) ghostEdgeInput.inputPortView.portElement.portCapLit = false;
                if (ghostEdgeInput.outputPortView != null) ghostEdgeInput.outputPortView.portElement.portCapLit = false;

                this.ghostEdgeInput.edgeElement.RemoveFromHierarchy();
                ghostEdgeInput = null;
            }

            if (this.ghostEdgeOutput != null)
            {
                if (ghostEdgeOutput.inputPortView != null) ghostEdgeOutput.inputPortView.portElement.portCapLit = false;
                if (ghostEdgeOutput.outputPortView != null) ghostEdgeOutput.outputPortView.portElement.portCapLit = false;

                ghostEdgeOutput.edgeElement.RemoveFromHierarchy();
                ghostEdgeOutput = null;
            }
        }
    }
}