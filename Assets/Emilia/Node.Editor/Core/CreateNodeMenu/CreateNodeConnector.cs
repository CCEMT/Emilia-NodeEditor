namespace Emilia.Node.Editor
{
    public struct CreateNodeConnector
    {
        public string originalNodeId { get; set; }
        public string originalPortId { get; set; }
        public string edgeId { get; set; }
        public string targetPortId { get; set; }
    }
}