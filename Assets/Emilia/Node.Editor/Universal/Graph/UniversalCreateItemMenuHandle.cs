using System.Collections.Generic;
using Emilia.Kit;
using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    [EditorHandle(typeof(EditorUniversalGraphAsset))]
    public class UniversalCreateItemMenuHandle : CreateItemMenuHandle
    {
        public override void CollectItemMenus(EditorGraphView graphView, List<CreateItemMenuInfo> itemTypes)
        {
            base.CollectItemMenus(graphView, itemTypes);
            CreateItemMenuInfo group = new CreateItemMenuInfo();
            group.itemAssetType = typeof(EditorGroupAsset);
            group.path = "Group";

            itemTypes.Add(group);

            CreateItemMenuInfo sticky = new CreateItemMenuInfo();
            sticky.itemAssetType = typeof(StickyNoteAsset);
            sticky.path = "Sticky Note";

            itemTypes.Add(sticky);
        }
    }
}