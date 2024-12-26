namespace Emilia.Node.Editor
{
    /// <summary>
    /// 端口展开编辑InputPort
    /// </summary>
    public struct EditorNodeInputPortEditInfo
    {
        public string portName;
        public string fieldPath;
        public bool forceImGUIDraw;

        public EditorNodeInputPortEditInfo(string portName, string fieldPath, bool forceImGUIDraw = false)
        {
            this.portName = portName;
            this.fieldPath = fieldPath;
            this.forceImGUIDraw = forceImGUIDraw;
        }
    }
}