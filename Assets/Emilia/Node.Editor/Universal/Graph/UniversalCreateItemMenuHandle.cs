using System.Collections.Generic;
using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    public class UniversalCreateItemMenuHandle : CreateItemMenuHandle<EditorUniversalGraphAsset>
    {
        public override void CollectItemMenus(List<CreateItemMenuInfo> itemTypes)
        {
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