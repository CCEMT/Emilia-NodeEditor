using Emilia.Node.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    public class UniversalGraphHotKeysHandle : GraphHotKeysHandle<EditorUniversalGraphAsset>
    {
        public override void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.S && evt.actionKey)
            {
                smartValue.graphOperate.Save();
                evt.StopPropagation();
            }

            OnKeyDownShortcut_Hook(evt);
        }
        
        private void OnKeyDownShortcut_Hook(KeyDownEvent evt)
        {
            if (! smartValue.isReframable || smartValue.panel.GetCapturingElement(PointerId.mousePointerId) != null) return;

            EventPropagation eventPropagation = EventPropagation.Continue;
            switch (evt.character)
            {
                case ' ':
                    eventPropagation = smartValue.OnInsertNodeKeyDown_Internals(evt);
                    break;
                case '[':
                    eventPropagation = smartValue.FramePrev();
                    break;
                case ']':
                    eventPropagation = smartValue.FrameNext();
                    break;
                case 'a':
                    eventPropagation = smartValue.FrameAll();
                    break;
                case 'o':
                    eventPropagation = smartValue.FrameOrigin();
                    break;
            }
            if (eventPropagation != EventPropagation.Stop) return;
            evt.StopPropagation();
            if (evt.imguiEvent != null) evt.imguiEvent.Use();
        }
    }
}