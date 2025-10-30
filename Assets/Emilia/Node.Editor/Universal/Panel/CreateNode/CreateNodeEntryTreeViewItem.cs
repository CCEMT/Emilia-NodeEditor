using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    public class CreateNodeEntryTreeViewItem : CreateNodeTreeViewItem
    {
        public ICreateNodeHandle createNodeHandle { get; }
        public bool isCollection { get; private set; }

        public CreateNodeEntryTreeViewItem(ICreateNodeHandle createNodeHandle, bool isCollection = false)
        {
            this.createNodeHandle = createNodeHandle;
            icon = createNodeHandle.icon;
            this.isCollection = isCollection;
        }
    }
}