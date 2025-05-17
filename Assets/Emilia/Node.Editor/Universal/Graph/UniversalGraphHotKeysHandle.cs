using Emilia.Kit;
using Emilia.Node.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    [EditorHandle(typeof(EditorUniversalGraphAsset))]
    public class UniversalGraphHotKeysHandle : GraphHotKeysHandle
    {
        public override void OnKeyDown(EditorGraphView graphView, KeyDownEvent evt)
        {
            base.OnKeyDown(graphView, evt);
            if (evt.keyCode == KeyCode.S && evt.actionKey)
            {
                graphView.graphOperate.Save();
                evt.StopPropagation();
            }

            OnKeyDownShortcut_Hook(graphView, evt);
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