using System;
using UnityEngine;

namespace Emilia.Node.Editor
{
    public class CreateNodeHandle : ICreateNodeHandle
    {
        public object userData { get; set; }
        public Type editorNodeType { get; set; }
        public bool validity { get; set; } = true;
        public string path { get; set; }
        public int priority { get; set; }
        public Texture2D icon { get; set; }
        public void OnInitialize() { }
    }
}