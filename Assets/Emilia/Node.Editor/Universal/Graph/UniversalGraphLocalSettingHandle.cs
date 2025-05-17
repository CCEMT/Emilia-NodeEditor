using System;
using Emilia.Kit;
using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    [EditorHandle(typeof(EditorUniversalGraphAsset))]
    public class UniversalGraphLocalSettingHandle : GraphLocalSettingHandle
    {
        public override Type GetAssetSettingType(EditorGraphView graphView) => typeof(UniversalGraphAssetLocalSetting);
    }
}