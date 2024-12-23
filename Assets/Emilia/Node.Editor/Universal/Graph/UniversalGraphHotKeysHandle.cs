using Emilia.Node.Editor;
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
                this.smartValue.graphOperate.Save();
                evt.StopPropagation();
            }
        }
    }
}