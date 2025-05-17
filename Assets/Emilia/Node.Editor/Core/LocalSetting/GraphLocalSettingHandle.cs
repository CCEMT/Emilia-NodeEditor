using System;
using Emilia.Kit;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public abstract class GraphLocalSettingHandle
    {
        public virtual Type GetTypeSettingType(EditorGraphView graphView) => null;
        public virtual Type GetAssetSettingType(EditorGraphView graphView) => null;
        public virtual void OnReadTypeSetting(IGraphTypeLocalSetting setting) { }
        public virtual void OnReadAssetSetting(IGraphAssetLocalSetting setting) { }
    }
}