namespace Emilia.Node.Editor
{
    public struct OperateMenuActionInfo
    {
        public IOperateMenuAction action;
        public string name;
        public string category;
        public int priority;
        public string[] tags;
    }
}