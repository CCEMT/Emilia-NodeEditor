using Emilia.Node.Universal.Editor;
using UnityEngine;

namespace Example
{
    [NodeToEditor(typeof(EditorCapabilitiesDisplayNodeAsset)),
     CreateAssetMenu(menuName = "Emilia/Example/CapabilitiesDisplayAsset", fileName = "CapabilitiesDisplayAsset")]
    public class CapabilitiesDisplayAsset : ExampleAsset { }
}