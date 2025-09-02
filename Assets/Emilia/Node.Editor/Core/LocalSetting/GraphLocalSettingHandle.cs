using Emilia.Kit;

namespace Emilia.Node.Editor
{
    [EditorHandleGenerate]
    public abstract class GraphLocalSettingHandle
    {
        /// <summary>
        /// 读取“类型”设置
        /// </summary>
        public virtual void OnReadTypeSetting(GraphLocalSettingCache setting) { }

        /// <summary>
        /// 读取“资源”设置
        /// </summary>
        public virtual void OnReadAssetSetting(GraphLocalSettingCache setting) { }
    }
}