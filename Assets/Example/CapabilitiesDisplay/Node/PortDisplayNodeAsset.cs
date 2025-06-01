using System.Collections.Generic;
using Emilia.Node.Attributes;
using Emilia.Node.Editor;
using Emilia.Node.Universal.Editor;
using UnityEngine;

namespace Example
{
    [NodeMenu("端口演示节点")]
    public class PortDisplayNodeAsset : EditorCapabilitiesDisplayNodeAsset
    {
        public int leftIntPortValue;
        public string leftStringPortValue;

        protected override string defaultDisplayName => "端口演示节点";
    }

    [EditorNode(typeof(PortDisplayNodeAsset))]
    public class PortDisplayNodeView : EditorCapabilitiesDisplayNodeView
    {
        protected override bool canRename => true;

        public override void Initialize(EditorGraphView graphView, EditorNodeAsset asset)
        {
            base.Initialize(graphView, asset);
            SetColor(new Color(0, 1f, 0, 1));
        }

        public override List<EditorPortInfo> CollectStaticPortAssets()
        {
            List<EditorPortInfo> portInfos = new List<EditorPortInfo>();

            UniversalEditorPortInfo leftIntPortInfo = new UniversalEditorPortInfo();
            leftIntPortInfo.id = "leftIntPortInfo";
            leftIntPortInfo.displayName = "左端口(int)";
            leftIntPortInfo.portType = typeof(int);
            leftIntPortInfo.direction = EditorPortDirection.Input;
            leftIntPortInfo.orientation = EditorOrientation.Horizontal;
            leftIntPortInfo.color = Color.black;
            leftIntPortInfo.canMultiConnect = true;

            portInfos.Add(leftIntPortInfo);

            this.inputEditInfos.Add("leftIntPortInfo", new EditorNodeInputPortEditInfo("leftIntPortInfo", "leftIntPortValue"));

            UniversalEditorPortInfo leftStringPortInfo = new UniversalEditorPortInfo();
            leftStringPortInfo.id = "leftStringPortInfo";
            leftStringPortInfo.displayName = "左端口(string)";
            leftStringPortInfo.portType = typeof(string);
            leftStringPortInfo.direction = EditorPortDirection.Input;
            leftStringPortInfo.orientation = EditorOrientation.Horizontal;
            leftStringPortInfo.color = Color.blue;
            leftStringPortInfo.canMultiConnect = true;

            portInfos.Add(leftStringPortInfo);

            this.inputEditInfos.Add("leftStringPortInfo", new EditorNodeInputPortEditInfo("leftStringPortInfo", "leftStringPortValue"));

            UniversalEditorPortInfo rightIntPortInfo = new UniversalEditorPortInfo();
            rightIntPortInfo.id = "rightIntPortInfo";
            rightIntPortInfo.displayName = "右端口(int)";
            rightIntPortInfo.portType = typeof(int);
            rightIntPortInfo.direction = EditorPortDirection.Output;
            rightIntPortInfo.orientation = EditorOrientation.Horizontal;
            rightIntPortInfo.color = Color.cyan;
            rightIntPortInfo.canMultiConnect = true;

            portInfos.Add(rightIntPortInfo);

            UniversalEditorPortInfo rightStringPortInfo = new UniversalEditorPortInfo();
            rightStringPortInfo.id = "rightStringPortInfo";
            rightStringPortInfo.displayName = "右端口(string)";
            rightStringPortInfo.portType = typeof(string);
            rightStringPortInfo.direction = EditorPortDirection.Output;
            rightStringPortInfo.orientation = EditorOrientation.Horizontal;
            rightStringPortInfo.color = Color.gray;
            rightStringPortInfo.canMultiConnect = true;

            portInfos.Add(rightStringPortInfo);

            UniversalEditorPortInfo topIntPortInfo = new UniversalEditorPortInfo();
            topIntPortInfo.id = "topIntPortInfo";
            topIntPortInfo.displayName = "上端口(int)";
            topIntPortInfo.portType = typeof(int);
            topIntPortInfo.direction = EditorPortDirection.Input;
            topIntPortInfo.orientation = EditorOrientation.Vertical;
            topIntPortInfo.color = Color.green;
            topIntPortInfo.canMultiConnect = true;

            portInfos.Add(topIntPortInfo);

            UniversalEditorPortInfo topStringPortInfo = new UniversalEditorPortInfo();
            topStringPortInfo.id = "topStringPortInfo";
            topStringPortInfo.displayName = "上端口(string)";
            topStringPortInfo.portType = typeof(string);
            topStringPortInfo.direction = EditorPortDirection.Input;
            topStringPortInfo.orientation = EditorOrientation.Vertical;
            topStringPortInfo.color = Color.magenta;
            topStringPortInfo.canMultiConnect = true;

            portInfos.Add(topStringPortInfo);

            UniversalEditorPortInfo bottomIntPortInfo = new UniversalEditorPortInfo();
            bottomIntPortInfo.id = "bottomIntPortInfo";
            bottomIntPortInfo.displayName = "下端口(int)";
            bottomIntPortInfo.portType = typeof(int);
            bottomIntPortInfo.direction = EditorPortDirection.Output;
            bottomIntPortInfo.orientation = EditorOrientation.Vertical;
            bottomIntPortInfo.color = Color.red;
            bottomIntPortInfo.canMultiConnect = true;

            portInfos.Add(bottomIntPortInfo);

            UniversalEditorPortInfo bottomStringPortInfo = new UniversalEditorPortInfo();
            bottomStringPortInfo.id = "bottomStringPortInfo";
            bottomStringPortInfo.displayName = "下端口(string)";
            bottomStringPortInfo.portType = typeof(string);
            bottomStringPortInfo.direction = EditorPortDirection.Output;
            bottomStringPortInfo.orientation = EditorOrientation.Vertical;
            bottomStringPortInfo.color = Color.yellow;
            bottomStringPortInfo.canMultiConnect = true;

            portInfos.Add(bottomStringPortInfo);

            return portInfos;
        }
    }
}