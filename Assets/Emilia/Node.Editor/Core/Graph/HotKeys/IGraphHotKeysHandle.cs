using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    public interface IGraphHotKeysHandle : IEditorHandle
    {
        /// <summary>
        /// 键盘按下事件
        /// </summary>
        void OnKeyDown(KeyDownEvent evt);
    }
}