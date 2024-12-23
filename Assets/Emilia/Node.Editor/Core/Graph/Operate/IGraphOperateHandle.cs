using UnityEngine;

namespace Emilia.Node.Editor
{
    public interface IGraphOperateHandle : IEditorHandle
    {
        void OpenCreateNodeMenu(Vector2 mousePosition, CreateNodeContext createNodeContext = default);
        void Cut();
        void Copy();
        void Paste();
        void Delete();
        void Duplicate();
        void Save();
    }
}