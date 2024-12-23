using System;

namespace Emilia.Node.Editor
{
    public struct EditorHandlePriority : IComparable
    {
        public double value;

        public int CompareTo(object obj)
        {
            if (obj is EditorHandlePriority priority) return this.value.CompareTo(priority.value);
            return 0;
        }
    }
}