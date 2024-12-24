using System;
using UnityEngine;

namespace Emilia.Node.Editor
{
    public struct CreateNodeInfo
    {
        public Type nodeAssetType;
        public object userData;
        public string portId;

        public string path;
        public int priority;
        public Texture2D icon;
    }
}