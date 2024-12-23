using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Emilia.Node.Editor
{
    public interface IEditorPortView
    {
        EditorPortInfo info { get; }

        IEditorNodeView master { get; }

        EditorPortDirection portDirection { get; }

        EditorOrientation editorOrientation { get; }

        Port portElement { get; }

        IReadOnlyList<IEditorEdgeView> edges { get; }
        event Action<IEditorPortView, IEditorEdgeView> onConnected;
        event Action<IEditorPortView, IEditorEdgeView> OnDisconnected;

        void Initialize(IEditorNodeView master, EditorPortInfo info);

        void Dispose();
    }
}