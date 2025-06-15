using Emilia.Kit;
using Emilia.Node.Universal.Editor;

namespace Example.Expand
{
    //拖拽拓展
    [EditorHandle(typeof(ExpandAsset))]
    public class ExpandDragAndDropHandle : UniversalDragAndDropHandle { }
}