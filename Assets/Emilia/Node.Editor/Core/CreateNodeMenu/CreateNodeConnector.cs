namespace Emilia.Node.Editor
{
    public class CreateNodeConnector
    {
        /// <summary>
        /// 原节点Id
        /// </summary>
        public string originalNodeId { get; set; }

        /// <summary>
        /// 原端口Id
        /// </summary>
        public string originalPortId { get; set; }

        /// <summary>
        /// Edge Id
        /// </summary>
        public string edgeId { get; set; }

        /// <summary>
        /// 目标端口Id
        /// </summary>
        public string targetPortId { get; set; }
    }
}