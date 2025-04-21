using Emilia.Node.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    public class UniversalDragAndDropHandle : GraphDragAndDropHandle<EditorUniversalGraphAsset>
    {
        public const string CreateNodeDragAndDropType = "CreateNode";

        public override void DragUpdatedCallback(DragUpdatedEvent evt)
        {
            base.DragUpdatedCallback(evt);

            object genericData = DragAndDrop.GetGenericData(CreateNodeDragAndDropType);
            if (genericData is ICreateNodeHandle createNodeHandle)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                DragAndDrop.AcceptDrag();
            }
        }

        public override void DragPerformedCallback(DragPerformEvent evt)
        {
            base.DragPerformedCallback(evt);

            object genericData = DragAndDrop.GetGenericData(CreateNodeDragAndDropType);
            if (genericData is ICreateNodeHandle createNodeHandle)
            {
                Vector2 mousePosition = evt.mousePosition;
                Vector2 graphMousePosition = smartValue.contentViewContainer.WorldToLocal(mousePosition);
                
                smartValue.nodeSystem.CreateNode(createNodeHandle.editorNodeType, graphMousePosition, createNodeHandle.nodeData);
            }
        }
    }
}