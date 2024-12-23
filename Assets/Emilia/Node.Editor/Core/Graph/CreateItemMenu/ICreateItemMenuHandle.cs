using System.Collections.Generic;

namespace Emilia.Node.Editor
{
    public interface ICreateItemMenuHandle : IEditorHandle
    {
        void CollectItemMenus(List<CreateItemMenuInfo> itemTypes);
    }
}