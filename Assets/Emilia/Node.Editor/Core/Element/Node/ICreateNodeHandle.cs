using System;
using UnityEngine;

namespace Emilia.Node.Editor
{
    public interface ICreateNodeHandle
    {
        object userData { get; }
        Type editorNodeType { get; }

        bool validity { get; }
        string path { get; }
        int priority { get; }
        Texture2D icon { get; }

        void OnInitialize();
    }
}