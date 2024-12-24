using System;
using UnityEngine;

namespace Emilia.Node.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GraphSettingAttribute : Attribute
    {
        public float maxLoadTimeMs = 0.0416f;
        public bool fastUndo = true;
        public Vector2 zoomSize = new Vector2(0.15f, 3f);
    }
}