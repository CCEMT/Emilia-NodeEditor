using System.Collections.Generic;
using Emilia.Node.Attributes;
using Emilia.Node.Editor;
using Emilia.Node.Universal.Editor;
using UnityEngine;

namespace Example
{
    [NodeMenu("横向演示节点")]
    public class HorizontalDisplayNodeAsset : EditorCapabilitiesDisplayNodeAsset
    {
        protected override string defaultDisplayName => "横向演示节点";
    }

    [EditorNode(typeof(HorizontalDisplayNodeAsset))]
    public class HorizontalDisplayNodeView : EditorCapabilitiesDisplayNodeView
    {
        public override void Initialize(EditorGraphView graphView, EditorNodeAsset asset)
        {
            base.Initialize(graphView, asset);
            SetColor(Color.cyan);
        }

        public override List<EditorPortInfo> CollectStaticPortAssets()
        {
            List<EditorPortInfo> portInfos = new List<EditorPortInfo>();

            UniversalEditorPortInfo leftIntPortInfo = new UniversalEditorPortInfo();
            leftIntPortInfo.id = "leftInt1PortInfo";
            leftIntPortInfo.displayName = "左端口1(int)";
            leftIntPortInfo.portType = typeof(int);
            leftIntPortInfo.direction = EditorPortDirection.Input;
            leftIntPortInfo.orientation = EditorOrientation.Horizontal;
            leftIntPortInfo.color = Color.black;
            leftIntPortInfo.canMultiConnect = true;

            portInfos.Add(leftIntPortInfo);

            UniversalEditorPortInfo leftStringPortInfo = new UniversalEditorPortInfo();
            leftStringPortInfo.id = "leftInt2PortInfo";
            leftStringPortInfo.displayName = "左端口2(int)";
            leftStringPortInfo.portType = typeof(int);
            leftStringPortInfo.direction = EditorPortDirection.Input;
            leftStringPortInfo.orientation = EditorOrientation.Horizontal;
            leftStringPortInfo.color = Color.blue;
            leftStringPortInfo.canMultiConnect = true;
            leftStringPortInfo.insertOrder = 100;

            portInfos.Add(leftStringPortInfo);

            UniversalEditorPortInfo rightIntPortInfo = new UniversalEditorPortInfo();
            rightIntPortInfo.id = "rightInt1PortInfo";
            rightIntPortInfo.displayName = "右端口1(int)";
            rightIntPortInfo.portType = typeof(int);
            rightIntPortInfo.direction = EditorPortDirection.Output;
            rightIntPortInfo.orientation = EditorOrientation.Horizontal;
            rightIntPortInfo.color = Color.cyan;
            rightIntPortInfo.canMultiConnect = true;

            portInfos.Add(rightIntPortInfo);

            UniversalEditorPortInfo rightStringPortInfo = new UniversalEditorPortInfo();
            rightStringPortInfo.id = "rightInt2PortInfo";
            rightStringPortInfo.displayName = "右端口2(int)";
            rightStringPortInfo.portType = typeof(int);
            rightStringPortInfo.direction = EditorPortDirection.Output;
            rightStringPortInfo.orientation = EditorOrientation.Horizontal;
            rightStringPortInfo.color = Color.gray;
            rightStringPortInfo.canMultiConnect = true;
            rightStringPortInfo.insertOrder = 100;

            portInfos.Add(rightStringPortInfo);

            return portInfos;
        }
    }
}