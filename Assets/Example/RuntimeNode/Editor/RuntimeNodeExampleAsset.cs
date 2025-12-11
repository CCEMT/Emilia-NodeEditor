using Emilia.Node.Universal.Editor;
using Example.RuntimeNode.Runtime;
using UnityEngine;

namespace Example.RuntimeNode.Editor
{
    //             指定运行时节点基类            指定编辑器节点基类
    [NodeToRuntime(typeof(RuntimeNodeAsset), typeof(EditorRuntimeNodeAsset)),
     CreateAssetMenu(menuName = "Emilia/Example/RuntimeNodeExampleAsset", fileName = "RuntimeNodeExampleAsset")]
    public class RuntimeNodeExampleAsset : ExampleAsset { }
}