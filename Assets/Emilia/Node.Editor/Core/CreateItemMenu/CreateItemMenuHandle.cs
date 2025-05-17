using System.Collections.Generic;
using Emilia.Kit;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public abstract class CreateItemMenuHandle
    {
        public virtual void CollectItemMenus(EditorGraphView graphView, List<CreateItemMenuInfo> itemTypes) { }
    }
}