using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    public interface IGraphHotKeysHandle : IEditorHandle
    {
        void OnKeyDown(KeyDownEvent evt);
    }
}