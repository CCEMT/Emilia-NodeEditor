using System.Linq;
using Emilia.Kit;
using Emilia.Node.Editor;
using Emilia.Node.Universal.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Example.Expand
{
    //快捷键拓展
    [EditorHandle(typeof(ExpandAsset))]
    public class ExpandHotkeysHandle : UniversalGraphHotKeysHandle
    {
        public override void OnKeyDown(EditorGraphView graphView, KeyDownEvent evt)
        {
            base.OnKeyDown(graphView, evt);

            //布局快捷键
            if (evt.ctrlKey && evt.keyCode == KeyCode.Q)
            {
                const float Interval = 50;

                GraphLayoutUtility.AlignmentType alignmentType = GraphLayoutUtility.AlignmentType.Horizontal | GraphLayoutUtility.AlignmentType.TopOrLeft;
                GraphLayoutUtility.Start(Interval, alignmentType, graphView.graphSelected.selected.OfType<IEditorNodeView>().ToList());
            }
        }
    }
}