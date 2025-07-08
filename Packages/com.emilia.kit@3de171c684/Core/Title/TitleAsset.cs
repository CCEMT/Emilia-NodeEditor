#if UNITY_EDITOR
using System;
using Sirenix.OdinInspector;

namespace Emilia.Kit
{
    [Serializable]
    public abstract class TitleAsset : SerializedScriptableObject
    {
        public abstract string title { get; }
    }
}
#endif