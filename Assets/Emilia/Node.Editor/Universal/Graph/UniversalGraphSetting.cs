using System;

namespace Emilia.Node.Universal.Editor
{
    /// <summary>
    /// 通用Graph设置
    /// </summary>
    [Serializable]
    public struct UniversalGraphSetting
    {
        public bool forceUseBuiltInInspector;
        public bool disabledTransmitNode;
        public bool disabledNodeInsert;
        public bool disabledEdgeDrawOptimization;
    }
}