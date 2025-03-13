using System;
using UnityEngine;

namespace Emilia.Node.Editor
{
    public struct CreateNodeInfo
    {
        public Type editorNodeAssetType;
        public object nodeData;
        public string portId;

        public string path;
        public int priority;
        public Texture2D icon;
    }
}