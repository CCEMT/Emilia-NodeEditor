namespace Emilia.Node.Editor
{
    public struct EditorLogicalConnection
    {
        public EditorNodeAsset outputNode;
        public string outputPortId;
        public EditorNodeAsset inputNode;
        public string inputPortId;

        public EditorLogicalConnection(
            EditorNodeAsset outputNode,
            string outputPortId,
            EditorNodeAsset inputNode,
            string inputPortId)
        {
            this.outputNode = outputNode;
            this.outputPortId = outputPortId;
            this.inputNode = inputNode;
            this.inputPortId = inputPortId;
        }
    }
}