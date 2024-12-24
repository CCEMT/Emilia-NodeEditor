using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Emilia.Node.Editor
{
    public interface IEditorItemView : IDeleteGraphElement, IRemoveViewElement, IGraphCopyPasteElement, ISelectableGraphElement
    {
        EditorItemAsset asset { get; }

        GraphElement element { get; }

        void Initialize(EditorGraphView graphView, EditorItemAsset asset);

        void SetPosition(Rect position);

        void SetPositionNoUndo(Rect position);
        
        void OnValueChanged();

        void Dispose();
    }
}