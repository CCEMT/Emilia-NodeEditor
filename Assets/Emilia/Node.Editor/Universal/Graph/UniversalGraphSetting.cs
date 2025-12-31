using System;

namespace Emilia.Node.Universal.Editor
{
    /// <summary>
    /// 通用Graph设置
    /// </summary>
    [Serializable]
    public struct UniversalGraphSetting
    {
        /// <summary>
        /// 强制使用内置Inspector
        /// </summary>
        public bool forceUseBuiltInInspector;

        /// <summary>
        /// 禁用传送节点
        /// </summary>
        public bool disabledTransmitNode;

        /// <summary>
        /// 禁用节点插入
        /// </summary>
        public bool disabledNodeInsert;

        /// <summary>
        /// 禁用边绘制优化
        /// </summary>
        public bool disabledEdgeDrawOptimization;
    }
}