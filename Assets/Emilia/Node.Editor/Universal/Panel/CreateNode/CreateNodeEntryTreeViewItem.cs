using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    /// <summary>
    /// 创建节点面板TreeViewItem的创建节点Item实现
    /// </summary>
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