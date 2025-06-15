using Sirenix.OdinInspector;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    public class StickyNoteProAsset : UniversalItemAsset
    {
        [HideLabel, TextArea(50, 50)]
        public string context = "内容";

        public override string title => "便利贴";
    }
}