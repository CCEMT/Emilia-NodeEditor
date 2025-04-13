using System.Collections.Generic;
using System.Linq;
using Emilia.Node.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    public class NodeDuplicateDragger : MouseManipulator
    {
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDownEvent);
        }

        protected virtual void OnMouseDownEvent(MouseDownEvent evt)
        {
            bool isDown = evt.button == 0 && evt.shiftKey;
            if (isDown == false) return;
            
             IEditorNodeView editorNodeView = target as IEditorNodeView;

            List<GraphElement> collectedElementSet = new List<GraphElement>();

            collectedElementSet.Add(editorNodeView.element);

            int amount = editorNodeView.portViews.Count;
            for (int i = 0; i < amount; i++)
            {
                IEditorPortView portView = editorNodeView.portViews[i];
                List<IEditorEdgeView> edges = portView.GetEdges();
                int edgeAmount = edges.Count;
                for (int j = 0; j < edgeAmount; j++)
                {
                    IEditorEdgeView edge = edges[j];
                    collectedElementSet.Add(edge.edgeElement);
                }
            }

            editorNodeView.graphView.graphCopyPaste.SerializeGraphElementsCallback(collectedElementSet);

            var pasteContent = editorNodeView.graphView.graphCopyPaste.UnserializeAndPasteCallback("Paste", editorNodeView.graphView.GetSerializedData_Internal());
            var nodeViews = pasteContent.OfType<IEditorNodeView>();
            var views = nodeViews.Where((nodeView) => nodeView.GetType() == GetType() &&
                                                      nodeView.asset.GetType() == editorNodeView.asset.GetType() &&
                                                      nodeView.asset.userData?.GetType() == editorNodeView.asset.userData?.GetType());

            IEditorNodeView pasteNode = views.FirstOrDefault();
            pasteNode.asset.position = editorNodeView.asset.position;
            pasteNode.SetPositionNoUndo(editorNodeView.asset.position);

            MouseUpEvent mouseUpEvent = MouseUpEvent.GetPooled(
                evt.mousePosition,
                evt.button,
                evt.clickCount,
                evt.mouseDelta,
                evt.modifiers);

            editorNodeView.graphView.SendGraphEvent(mouseUpEvent);
            mouseUpEvent.Dispose();

            MouseDownEvent mouseDownEventCopy = MouseDownEvent.GetPooled(
                evt.mousePosition,
                evt.button,
                evt.clickCount,
                evt.mouseDelta,
                evt.modifiers);

            editorNodeView.graphView.SendGraphEvent(mouseDownEventCopy);
            mouseUpEvent.Dispose();
        }
    }
}