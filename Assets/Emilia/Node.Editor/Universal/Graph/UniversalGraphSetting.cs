using System;

namespace Emilia.Node.Universal.Editor
{
    [Serializable]
    public struct UniversalGraphSetting
    {
        public bool forceUseBuiltInInspector;
        public bool disabledTransmitNode;
        public bool disabledNodeInsert;
        public bool disabledEdgeDrawOptimization;
    }
}