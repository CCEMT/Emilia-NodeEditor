using System.Collections.Generic;
using System.Linq;
using Emilia.Node.Editor;
using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    public class NodeInsertDragger : MouseManipulator
    {
        private bool isActive;

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
            target.RegisterCallback<MouseUpEvent>(OnMouseUpEvent);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDownEvent);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUpEvent);
        }

        private void OnMouseDownEvent(MouseDownEvent evt)
        {
            IEditorNodeView nodeView = target as IEditorNodeView;
            if (nodeView == null) return;

            int portAmount = nodeView.portViews.Count;
            for (int i = 0; i < portAmount; i++)
            {
                IEditorPortView portView = nodeView.portViews[i];

                List<IEditorEdgeView> edges = portView.GetEdges();
                if (edges.Count <= 0) continue;

                this.isActive = true;
                break;
            }
        }

        private void OnMouseMoveEvent(MouseMoveEvent evt)
        {
            if (isActive == false) return;

            IEditorNodeView nodeView = target as IEditorNodeView;

            List<VisualElement> pickedElements = new List<VisualElement>();
            nodeView.graphView.panel.PickAll(evt.mousePosition, pickedElements);

            List<IEditorEdgeView> edgeViews = pickedElements.OfType<IEditorEdgeView>().ToList();
            if (edgeViews.Count == 0) return;

            IEditorEdgeView edgeView = edgeViews.FirstOrDefault();
            //TODO

            int portAmount = nodeView.portViews.Count;
            for (int i = 0; i < portAmount; i++)
            {
                IEditorPortView portView = nodeView.portViews[i];

                if (portView.portDirection == EditorPortDirection.Input)
                {
                    bool canConnect = nodeView.graphView.connectSystem.CanConnect(portView, edgeView.inputPortView);

                }
                else
                {
                    bool canConnect = nodeView.graphView.connectSystem.CanConnect(edgeView.outputPortView, portView);
                }

            }
        }

        private void OnMouseUpEvent(MouseUpEvent evt)
        {
            if (isActive == false) return;
        }
    }
}