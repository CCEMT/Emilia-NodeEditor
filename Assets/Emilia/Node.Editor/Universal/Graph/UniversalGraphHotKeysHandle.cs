using Emilia.Kit;
using Emilia.Node.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    [EditorHandle(typeof(EditorUniversalGraphAsset))]
    public class UniversalGraphHotKeysHandle : GraphHotKeysHandle
    {
        public override void OnGraphKeyDown(EditorGraphView graphView, KeyDownEvent evt)
        {
            base.OnGraphKeyDown(graphView, evt);
            if (evt.keyCode == KeyCode.S && evt.actionKey)
            {
                graphView.graphOperate.Save();
                evt.StopPropagation();
            }

            OnKeyDownShortcut_Hook(graphView, evt);
        }

        public override void OnTreeKeyDown(EditorGraphView graphView, KeyDownEvent evt)
        {
            base.OnTreeKeyDown(graphView, evt);

            if (evt.keyCode == KeyCode.Z && evt.ctrlKey)
            {
                bool isReload = evt.shiftKey;

                Undo.undoRedoPerformed += OnUndoRedoPerformed;
                Undo.PerformUndo();

                void OnUndoRedoPerformed()
                {
                    Undo.undoRedoPerformed -= OnUndoRedoPerformed;
                    if (isReload) graphView.Reload(graphView.graphAsset);
                    else
                    {
                        graphView.graphUndo.OnUndoRedoPerformed();
                        if (EditorGraphView.focusedGraphView == graphView) graphView.graphSelected.UpdateSelected();
                    }
                }

                evt.StopPropagation();
            }
        }

        private void OnKeyDownShortcut_Hook(EditorGraphView graphView, KeyDownEvent evt)
        {
            if (! graphView.isReframable || graphView.panel.GetCapturingElement(PointerId.mousePointerId) != null) return;

            EventPropagation eventPropagation = EventPropagation.Continue;
            switch (evt.character)
            {
                case ' ':
                    eventPropagation = graphView.OnInsertNodeKeyDown_Internals(evt);
                    break;
                case '[':
                    eventPropagation = graphView.FramePrev();
                    break;
                case ']':
                    eventPropagation = graphView.FrameNext();
                    break;
                case 'a':
                    eventPropagation = graphView.FrameAll();
                    break;
                case 'o':
                    eventPropagation = graphView.FrameOrigin();
                    break;
            }
            if (eventPropagation != EventPropagation.Stop) return;
            evt.StopPropagation();
            if (evt.imguiEvent != null) evt.imguiEvent.Use();
        }
    }
}