using System;
using Emilia.Kit;
using Emilia.Kit.Editor;

namespace Emilia.Node.Editor
{
    public class GraphLocalSettingSystem : BasicGraphViewModule
    {
        private const string GraphLocalSettingSaveKey = "GraphLocalSetting";

        private GraphLocalSettingHandle handle;

        private IGraphTypeLocalSetting _typeSetting;
        private IGraphAssetLocalSetting _assetSetting;
        public IGraphTypeLocalSetting typeSetting => this._typeSetting;
        public IGraphAssetLocalSetting assetSetting => this._assetSetting;

        private string typeSaveKey => GraphLocalSettingSaveKey + this.graphView.graphAsset.GetType().FullName;
        private string assetSaveKey => GraphLocalSettingSaveKey + this.graphView.graphAsset.id;

        public override int order => 100;

        public override void Initialize(EditorGraphView graphView)
        {
            base.Initialize(graphView);
            this.handle = EditorHandleUtility.CreateHandle<GraphLocalSettingHandle>(graphView.graphAsset.GetType());
        }

        public override void AllModuleInitializeSuccess()
        {
            base.AllModuleInitializeSuccess();
            ReadSetting();
        }

        /// <summary>
        /// 读取设置
        /// </summary>
        public void ReadSetting()
        {
            if (OdinEditorPrefs.HasValue(typeSaveKey)) this._typeSetting = OdinEditorPrefs.GetValue<IGraphTypeLocalSetting>(typeSaveKey);
            if (OdinEditorPrefs.HasValue(assetSaveKey)) this._assetSetting = OdinEditorPrefs.GetValue<IGraphAssetLocalSetting>(assetSaveKey);

            if (this._typeSetting == null) ResetTypeSetting();
            if (this._assetSetting == null) ResetAssetSetting();

            if (this._typeSetting != null) this.handle?.OnReadTypeSetting(this._typeSetting);
            if (this._assetSetting != null) this.handle?.OnReadAssetSetting(this._assetSetting);
        }

        /// <summary>
        /// 重置类型设置
        /// </summary>
        public void ResetTypeSetting()
        {
            Type createSettingType = this.handle?.GetTypeSettingType(this.graphView);
            if (typeof(IGraphTypeLocalSetting).IsAssignableFrom(createSettingType)) this._typeSetting = ReflectUtility.CreateInstance(createSettingType) as IGraphTypeLocalSetting;
        }

        /// <summary>
        /// 重置资源设置
        /// </summary>
        public void ResetAssetSetting()
        {
            Type createSettingType = this.handle?.GetAssetSettingType(this.graphView);
            if (typeof(IGraphAssetLocalSetting).IsAssignableFrom(createSettingType)) this._assetSetting = ReflectUtility.CreateInstance(createSettingType) as IGraphAssetLocalSetting;
        }

        /// <summary>
        /// 保存类型设置
        /// </summary>
        public void SaveTypeSetting()
        {
            OdinEditorPrefs.SetValue(typeSaveKey, this._typeSetting);
        }

        /// <summary>
        /// 保存资源设置
        /// </summary>
        public void SaveAssetSetting()
        {
            OdinEditorPrefs.SetValue(assetSaveKey, this._assetSetting);
        }

        /// <summary>
        /// 保存所有设置
        /// </summary>
        public void SaveAll()
        {
            OdinEditorPrefs.SetValue(typeSaveKey, this._typeSetting);
            OdinEditorPrefs.SetValue(assetSaveKey, this._assetSetting);
        }

        public override void Dispose()
        {
            this.handle = null;

            _typeSetting = null;
            _assetSetting = null;

            base.Dispose();
        }
    }
}