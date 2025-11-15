namespace Emilia.Node.Universal.Editor
{
    /// <summary>
    /// 节点收藏信息
    /// </summary>
    public class NodeCollectionInfo
    {
        public string nodeName { get; private set; }
        public string nodePath { get; private set; }

        public NodeCollectionInfo(string nodePath)
        {
            this.nodePath = nodePath;
            nodeName = nodePath.Split('/').Length > 0 ? nodePath.Split('/')[nodePath.Split('/').Length - 1] : "";
        }
    }
}