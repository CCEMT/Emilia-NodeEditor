using System;
using Emilia.Kit;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public abstract class GraphLocalSettingHandle
    {
        /// <summary>
        /// 获取“类型”设置的资源Type
        /// </summary>
        public virtual Type GetTypeSettingType(EditorGraphView graphView) => null;

        /// <summary>
        /// 获取“资源”设置的资源Type
        /// </summary>
        public virtual Type GetAssetSettingType(EditorGraphView graphView) => null;

        /// <summary>
        /// 读取“类型”设置
        /// </summary>
        public virtual void OnReadTypeSetting(IGraphTypeLocalSetting setting) { }

        /// <summary>
        /// 读取“资源”设置
        /// </summary>
        public virtual void OnReadAssetSetting(IGraphAssetLocalSetting setting) { }
    }
}