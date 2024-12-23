using System;

namespace Emilia.Node.Editor
{
    public struct OperateMenuItem
    {
        public string menuName;
        public string category;
        public int priority;
        public bool isOn;
        public OperateMenuActionValidity state;
        public Action onAction;
    }
}