using System;

namespace Emilia.Node.Editor
{
    public interface IGraphLocalSettingHandle : IEditorHandle
    {
        /// <summary>
        /// 指定的设置数据类型
        /// </summary>
        Type settingType { get; }

        /// <summary>
        /// 读取设置
        /// </summary>
        void OnReadSetting(IGraphLocalSetting setting);
    }
}