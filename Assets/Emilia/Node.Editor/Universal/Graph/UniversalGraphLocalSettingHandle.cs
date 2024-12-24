using System;
using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    public class UniversalGraphLocalSettingHandle : GraphLocalSettingHandle<EditorUniversalGraphAsset>
    {
        public override Type settingType => typeof(UniversalGraphLocalSetting);

        public override void OnReadSetting(IGraphLocalSetting setting)
        {
            UniversalGraphLocalSetting universalSetting = setting as UniversalGraphLocalSetting;
            smartValue.UpdateViewTransform(universalSetting.position, universalSetting.scale);
        }
    }
}