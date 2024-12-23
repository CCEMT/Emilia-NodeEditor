using UnityEditor.Experimental.GraphView;

namespace Emilia.Node.Editor
{
    public interface IEditorEdgeView : IDeleteGraphElement, IRemoveViewElement, IGraphCopyPasteElement, ISelectableGraphElement
    {
        EditorEdgeAsset asset { get; }
        EditorGraphView graphView { get; }

        IEditorPortView inputPortView { get; set; }
        IEditorPortView outputPortView { get; set; }

        bool isDrag { get; set; }
        Edge edgeElement { get; }

        void Initialize(EditorGraphView graphView, EditorEdgeAsset asset);

        void OnValueChanged();

        void Dispose();
    }
}