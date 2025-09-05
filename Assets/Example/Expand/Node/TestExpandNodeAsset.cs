using System.Collections.Generic;
using Emilia.Node.Attributes;
using Emilia.Node.Editor;
using Emilia.Node.Universal.Editor;

namespace Example.Expand.Node
{
    [NodeMenu("Test")]
    public class TestExpandNodeAsset : EditorExpandNodeAsset { }

    [EditorNode(typeof(TestExpandNodeAsset))]
    public class TestExpandNodeView : EditorExpandNodeView
    {
        public override List<EditorPortInfo> CollectStaticPortAssets()
        {
            List<EditorPortInfo> portInfos = new List<EditorPortInfo>();

            EditorPortInfo input = new EditorPortInfo();
            input.id = "input";
            input.displayName = "输入";
            input.direction = EditorPortDirection.Input;
            input.orientation = EditorOrientation.Horizontal;
            input.canMultiConnect = true;

            portInfos.Add(input);

            EditorPortInfo output = new EditorPortInfo();
            output.id = "output";
            output.displayName = "输出";
            output.direction = EditorPortDirection.Output;
            output.orientation = EditorOrientation.Horizontal;
            output.canMultiConnect = true;

            portInfos.Add(output);

            return portInfos;
        }
    }
}