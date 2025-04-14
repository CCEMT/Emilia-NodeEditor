using System;
using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    public class UniversalGraphLocalSettingHandle : GraphLocalSettingHandle<EditorUniversalGraphAsset>
    {
        public override Type assetSettingType => typeof(UniversalGraphAssetLocalSetting);
    }
}