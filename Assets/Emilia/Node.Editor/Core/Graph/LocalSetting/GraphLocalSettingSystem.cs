using System;
using Emilia.Kit;
using Emilia.Kit.Editor;

namespace Emilia.Node.Editor
{
    public class GraphLocalSettingSystem : GraphViewModule
    {
        private const string GraphLocalSettingSaveKey = "GraphLocalSetting";

        private IGraphLocalSettingHandle handle;

        private IGraphLocalSetting _setting;
        public IGraphLocalSetting setting => this._setting;

        private string saveKey => GraphLocalSettingSaveKey + this.graphView.graphAsset.id;

        public override int order => 100;

        public override void Reset(EditorGraphView graphView)
        {
            base.Reset(graphView);

            if (handle != null) EditorHandleUtility.ReleaseHandle(handle);
            this.handle = EditorHandleUtility.BuildHandle<IGraphLocalSettingHandle>(graphView.graphAsset.GetType(), graphView);

            ReadSetting();
        }

        /// <summary>
        /// 读取设置
        /// </summary>
        public void ReadSetting()
        {
            if (OdinEditorPrefs.HasValue(saveKey)) _setting = OdinEditorPrefs.GetValue<IGraphLocalSetting>(saveKey);

            if (this._setting == null)
            {
                Type createSettingType = this.handle?.settingType;
                if (typeof(IGraphLocalSetting).IsAssignableFrom(createSettingType)) this._setting = ReflectUtility.CreateInstance(createSettingType) as IGraphLocalSetting;
            }

            if (_setting != null) this.handle?.OnReadSetting(_setting);
        }

        /// <summary>
        /// 保存设置
        /// </summary>
        public void Save()
        {
            OdinEditorPrefs.SetValue(saveKey, this._setting);
        }

        public override void Dispose()
        {
            if (this.handle != null)
            {
                EditorHandleUtility.ReleaseHandle(handle);
                this.handle = null;
            }

            base.Dispose();
        }
    }
}