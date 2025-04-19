using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    public class CreateNodeEntryTreeViewItem : CreateNodeTreeViewItem
    {
        private ICreateNodeHandle _createNodeHandle;

        public ICreateNodeHandle createNodeHandle => _createNodeHandle;

        public CreateNodeEntryTreeViewItem(ICreateNodeHandle createNodeHandle)
        {
            _createNodeHandle = createNodeHandle;
            icon = createNodeHandle.icon;
        }
    }
}