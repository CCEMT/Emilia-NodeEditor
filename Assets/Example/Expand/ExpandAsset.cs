using Emilia.Node.Universal.Editor;
using Example.Expand.Node;
using UnityEngine;

namespace Example.Expand
{
    [NodeToEditor(typeof(EditorExpandNodeAsset)),
     CreateAssetMenu(menuName = "Emilia/Example/ExpandAsset", fileName = "ExpandAsset")]
    public class ExpandAsset : ExampleAsset { }
}