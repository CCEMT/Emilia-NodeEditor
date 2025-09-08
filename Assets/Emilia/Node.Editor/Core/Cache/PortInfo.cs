using System;

namespace Emilia.Node.Editor
{
    public struct PortInfo
    {
        /// <summary>
        /// 节点编辑器资产类型
        /// </summary>
        public Type nodeAssetType;

        /// <summary>
        /// 节点数据
        /// </summary>
        public object nodeData;

        /// <summary>
        /// 端口Id
        /// </summary>
        public string portId;
        
        /// <summary>
        /// 端口名称
        /// </summary>
        public string displayName;
    }
}