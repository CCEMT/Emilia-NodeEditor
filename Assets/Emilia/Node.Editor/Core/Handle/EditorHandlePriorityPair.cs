using System;

namespace Emilia.Node.Editor
{
    [Serializable]
    public struct EditorHandlePriorityPair
    {
        public Type type;
        public EditorHandlePriority priority;
    }
}