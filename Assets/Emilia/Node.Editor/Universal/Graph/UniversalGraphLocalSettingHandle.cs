using System;
using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    public class UniversalGraphLocalSettingHandle : GraphLocalSettingHandle<EditorUniversalGraphAsset>
    {
        public override Type assetSettingType => typeof(UniversalGraphAssetLocalSetting);

        public override void OnReadAssetSetting(IGraphAssetLocalSetting setting)
        {
            UniversalGraphAssetLocalSetting universalSetting = setting as UniversalGraphAssetLocalSetting;
            smartValue.UpdateViewTransform(universalSetting.position, universalSetting.scale);
        }
    }
}