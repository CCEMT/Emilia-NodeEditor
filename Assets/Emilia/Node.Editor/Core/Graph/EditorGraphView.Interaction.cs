using System.Collections.Generic;
using System.Linq;
using Emilia.Kit;
using Emilia.Kit.Editor;
using Emilia.Reflection.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor
{
    public partial class EditorGraphView
    {
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            OperateMenuContext menuContext = new();
            menuContext.evt = evt;
            menuContext.graphView = this;

            operateMenu.BuildMenu(menuContext);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new();

            IEditorPortView startPortView = startPort as IEditorPortView;
            if (startPortView == null) return compatiblePorts;

            foreach (Port port in this.ports)
            {
                IEditorPortView portView = port as IEditorPortView;
                if (portView == null)
                {
                    Debug.LogError("端口需要继承IEditorPortView");
                    continue;
                }

                if (startPortView.master == portView.master) continue;

                bool canConnect = connectSystem.CanConnect(startPortView, portView);
                if (canConnect) compatiblePorts.Add(port);
            }

            return compatiblePorts;
        }

        private string OnSerializeGraphElements(IEnumerable<GraphElement> elements) => graphCopyPaste.SerializeGraphElementsCallback(elements);

        private bool OnCanPasteSerializedData(string data) => graphCopyPaste.CanPasteSerializedDataCallback(data);

        private void OnUnserializeAndPaste(string operationName, string data)
        {
            GraphElement[] pasteObjects = graphCopyPaste.UnserializeAndPasteCallback(operationName, data).ToArray();

            SetSelection(pasteObjects.OfType<ISelectable>().ToList());
            UpdateSelected();

            clipboard_Internal = graphCopyPaste.SerializeGraphElementsCallback(pasteObjects);
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove == null || graphViewChange.elementsToRemove.Count <= 0) return graphViewChange;

            Undo.IncrementCurrentGroup();

            Delete(graphViewChange.elementsToRemove);
            graphViewChange.elementsToRemove.Clear();
            UpdateSelected();

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            Undo.IncrementCurrentGroup();

            return graphViewChange;
        }

        private void Delete(List<GraphElement> elementsToRemove)
        {
            int amount = elementsToRemove.Count;
            for (int i = 0; i < amount; i++)
            {
                GraphElement element = elementsToRemove[i];
                IDeleteGraphElement iDeleteGraphElement = element as IDeleteGraphElement;
                if (iDeleteGraphElement != null) iDeleteGraphElement.Delete();
            }
        }

        private void OnElementResized(VisualElement element)
        {
            IResizedGraphElement graphElement = element as IResizedGraphElement;
            if (graphElement != null) graphElement.OnElementResized();
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            UpdateSelected();
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            schedule.Execute(UpdateSelected).ExecuteLater(1);
        }

        private void OnUndoRedoPerformed()
        {
            if (isInitialized == false) return;
            graphUndo?.UndoRedoPerformed();
        }

        /// <summary>
        /// 更新选中
        /// </summary>
        public void UpdateSelected()
        {
            graphSelected?.UpdateSelected(selection.OfType<ISelectedHandle>().ToList());
        }

        /// <summary>
        /// 设置选中
        /// </summary>
        public void SetSelection(List<ISelectable> selectables)
        {
            ClearSelection();

            for (int i = 0; i < selectables.Count; i++)
            {
                ISelectable selectable = selectables[i];
                AddToSelection(selectable);
            }
        }

        public void SetSelectionAgent(List<ISelectedHandle> selectedHandles)
        {
            ClearSelection();

            bool shouldRecordUndo = ShouldRecordUndo_Internals();

            if (shouldRecordUndo) RecordSelectionUndoPre_Internals();

            for (int i = 0; i < selectedHandles.Count; i++)
            {
                ISelectedHandle selectedHandle = selectedHandles[i];

                GraphSelectAgent graphSelectAgent = new(selectedHandle);
                selection.Add(graphSelectAgent);

                if (shouldRecordUndo == false) continue;
                string dataKey = $"graph-select-agent-{selectedHandle.GetHashCode()}";
                m_GraphViewUndoRedoSelection_Internals.selectedElements_Internal.Add(dataKey);
                m_PersistedSelection_Internal.selectedElements_Internal.Add(dataKey);
            }

            if (shouldRecordUndo) RecordSelectionUndoPost_Internals();
        }

        /// <summary>
        /// 发送事件
        /// </summary>
        public void SendGraphEvent(IGraphEvent graphEvent)
        {
            graphEvent.graphView = this;
            this.SendEvent_Internal(graphEvent.eventTarget, DispatchMode_Internals.Immediate);
        }

        /// <summary>
        /// 发送事件
        /// </summary>
        public void SendEventImmediate(EventBase eventBase)
        {
            eventBase.target = this;
            this.SendEvent_Internal(eventBase, DispatchMode_Internals.Immediate);
        }

        /// <summary>
        /// 注册Undo
        /// </summary>
        public void RegisterCompleteObjectUndo(string name)
        {
            List<Object> objects = graphAsset.CollectAsset();
            Undo.RegisterCompleteObjectUndo(objects.ToArray(), name);
            graphSave.SetDirty();
        }

        /// <summary>
        /// 注册Undo
        /// </summary>
        public void RecordObjectUndo(string name)
        {
            List<Object> objects = graphAsset.CollectAsset();
            Undo.RecordObjects(objects.ToArray(), name);
            graphSave.SetDirty();
        }

        protected override bool OverrideOnKeyDownShortcut(KeyDownEvent evt) => true;
    }
}
