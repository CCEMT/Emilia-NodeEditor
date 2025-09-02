using System.Collections.Generic;
using Emilia.Kit;

namespace Emilia.Node.Editor
{
    public class GraphLocalSettingSystem : BasicGraphViewModule
    {
        private const string GraphLocalSettingSaveKey = "GraphLocalSettingKey";

        private GraphLocalSettingHandle handle;

        private GraphLocalSettingCache _typeSettingCache;
        private GraphLocalSettingCache _assetSettingCache;

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
            if (OdinEditorPrefs.HasValue(typeSaveKey)) this._typeSettingCache = OdinEditorPrefs.GetValue(typeSaveKey, new GraphLocalSettingCache());
            if (OdinEditorPrefs.HasValue(assetSaveKey)) this._assetSettingCache = OdinEditorPrefs.GetValue(assetSaveKey, new GraphLocalSettingCache());

            if (_typeSettingCache == null) _typeSettingCache = new GraphLocalSettingCache();
            if (_assetSettingCache == null) _assetSettingCache = new GraphLocalSettingCache();

            this.handle?.OnReadTypeSetting(this._typeSettingCache);
            this.handle?.OnReadAssetSetting(this._assetSettingCache);
        }

        /// <summary>
        /// 重置类型设置
        /// </summary>
        public void ResetTypeSetting()
        {
            _typeSettingCache.Clear();
        }

        /// <summary>
        /// 重置资源设置
        /// </summary>
        public void ResetAssetSetting()
        {
            _assetSettingCache.Clear();
        }

        /// <summary>
        /// 获取类型设置
        /// </summary>
        public T GetTypeSettingValue<T>(string key, T defaultValue = default)
        {
            string byteString = _typeSettingCache.GetValueOrDefault(key);
            if (string.IsNullOrEmpty(byteString)) return defaultValue;
            return OdinSerializableUtility.FromByteString<T>(byteString);
        }

        /// <summary>
        /// 设置类型设置
        /// </summary>
        public void SetTypeSettingValue<T>(string key, T value)
        {
            _typeSettingCache[key] = OdinSerializableUtility.ToByteString(value);
        }

        /// <summary>
        /// 获取资源设置
        /// </summary>
        public T GetAssetSettingValue<T>(string key, T defaultValue = default)
        {
            string byteString = _assetSettingCache.GetValueOrDefault(key);
            if (string.IsNullOrEmpty(byteString)) return defaultValue;
            return OdinSerializableUtility.FromByteString<T>(byteString);
        }

        /// <summary>
        /// 设置资源设置
        /// </summary>
        public void SetAssetSettingValue<T>(string key, T value)
        {
            _assetSettingCache[key] = OdinSerializableUtility.ToByteString(value);
        }

        /// <summary>
        /// 是否存在类型设置
        /// </summary>
        public bool HasTypeSetting(string key) => _typeSettingCache.ContainsKey(key);

        /// <summary>
        /// 是否存在资源设置
        /// </summary>
        public bool HasAssetSetting(string key) => _assetSettingCache.ContainsKey(key);

        /// <summary>
        /// 保存类型设置
        /// </summary>
        public void SaveTypeSetting()
        {
            OdinEditorPrefs.SetValue(typeSaveKey, this._typeSettingCache);
        }

        /// <summary>
        /// 保存资源设置
        /// </summary>
        public void SaveAssetSetting()
        {
            OdinEditorPrefs.SetValue(assetSaveKey, this._assetSettingCache);
        }

        /// <summary>
        /// 保存所有设置
        /// </summary>
        public void SaveAll()
        {
            OdinEditorPrefs.SetValue(typeSaveKey, this._typeSettingCache);
            OdinEditorPrefs.SetValue(assetSaveKey, this._assetSettingCache);
        }

        public override void Dispose()
        {
            this.handle = null;

            _typeSettingCache = null;
            _assetSettingCache = null;

            base.Dispose();
        }
    }
}