using System;
using UnityEngine;

namespace Emilia.Node.Editor
{
    public class EditorPortInfo
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public Type nodePortViewType { get; set; } = typeof(EditorPortView);
        public Type edgeConnectorType { get; set; } = typeof(EditorEdgeConnector);
        public Type portType { get; set; }
        public EditorPortDirection direction { get; set; }
        public EditorOrientation orientation { get; set; }
        public bool canMultiConnect { get; set; }
        public float order { get; set; }
        public Color color { get; set; }= Color.white;
    }
}