using System;

namespace Emilia.Node.Editor
{
    public interface IGraphLocalSettingHandle : IEditorHandle
    {
        Type settingType { get; }

        void OnReadSetting(IGraphLocalSetting setting);
    }
}