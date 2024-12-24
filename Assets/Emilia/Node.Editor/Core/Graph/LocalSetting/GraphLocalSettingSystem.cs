using System;
using Emilia.Kit;
using Emilia.Kit.Editor;

namespace Emilia.Node.Editor
{
    public class GraphLocalSettingSystem
    {
        private const string GraphLocalSettingSaveKey = "GraphLocalSetting";

        private EditorGraphView graphView;
        private IGraphLocalSettingHandle handle;

        private IGraphLocalSetting _setting;
        public IGraphLocalSetting setting => this._setting;

        public void Reset(EditorGraphView graphView)
        {
            this.graphView = graphView;

            if (handle != null) EditorHandleUtility.ReleaseHandle(handle);
            this.handle = EditorHandleUtility.BuildHandle<IGraphLocalSettingHandle>(graphView.graphAsset.GetType(), graphView);

            ReadSetting();
        }

        public void ReadSetting()
        {
            if (OdinEditorPrefs.HasValue(GraphLocalSettingSaveKey)) _setting = OdinEditorPrefs.GetValue<IGraphLocalSetting>(GraphLocalSettingSaveKey);

            if (this._setting == null)
            {
                Type createSettingType = this.handle?.settingType;
                if (typeof(IGraphLocalSetting).IsAssignableFrom(createSettingType)) this._setting = ReflectUtility.CreateInstance(createSettingType) as IGraphLocalSetting;
            }

            if (_setting != null) this.handle?.OnReadSetting(_setting);
        }

        public void Save()
        {
            OdinEditorPrefs.SetValue(GraphLocalSettingSaveKey, this._setting);
        }

        public void Dispose()
        {
            if (this.handle != null)
            {
                EditorHandleUtility.ReleaseHandle(handle);
                this.handle = null;
            }

            this.graphView = null;
        }
    }
}