using System;

namespace Emilia.Node.Editor
{
    public interface IGraphLocalSettingHandle : IEditorHandle
    {
        /// <summary>
        /// 指定的类型设置数据类型
        /// </summary>
        Type typeSettingType { get; }

        /// <summary>
        /// 指定的资产设置数据类型
        /// </summary>
        Type assetSettingType { get; }

        /// <summary>
        /// 读取类型设置
        /// </summary>
        void OnReadTypeSetting(IGraphTypeLocalSetting setting);
        
        /// <summary>
        /// 读取资产设置
        /// </summary>
        void OnReadAssetSetting(IGraphAssetLocalSetting setting);
    }
}