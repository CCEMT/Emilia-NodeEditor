using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Emilia.Node.Editor
{
    public interface IEditorNodeView : IDeleteGraphElement, IRemoveViewElement, IGraphCopyPasteElement, ISelectableGraphElement
    {
        EditorGraphView graphView { get; }
        EditorNodeAsset asset { get; }
        GraphElement element { get; }

        IReadOnlyList<IEditorPortView> portViews { get; }

        void Initialize(EditorGraphView graphView, EditorNodeAsset asset);

        List<EditorPortInfo> CollectStaticPortAssets();

        IEditorPortView GetPortView(string portId);

        IEditorPortView AddPortView(EditorPortInfo asset);

        void SetPosition(Rect position);

        void SetPositionNoUndo(Rect position);

        void OnValueChanged();

        void Dispose();
    }
}