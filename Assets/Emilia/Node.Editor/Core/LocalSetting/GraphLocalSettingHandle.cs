using System;

namespace Emilia.Node.Editor
{
    [GenericHandle]
    public abstract class GraphLocalSettingHandle<T> : EditorHandle, IGraphLocalSettingHandle where T : EditorGraphAsset
    {
        public EditorGraphView smartValue { get; private set; }

        public IGraphLocalSettingHandle parentHandle { get; private set; }

        public override void Initialize(object weakSmartValue)
        {
            base.Initialize(weakSmartValue);
            smartValue = weakSmartValue as EditorGraphView;
            parentHandle = parent as IGraphLocalSettingHandle;
        }

        public Type typeSettingType
        {
            get
            {
                if (parentHandle == null) return null;
                return parentHandle.typeSettingType;
            }
        }

        public virtual Type assetSettingType
        {
            get
            {
                if (parentHandle == null) return null;
                return parentHandle.assetSettingType;
            }
        }

        public void OnReadTypeSetting(IGraphTypeLocalSetting setting)
        {
            parentHandle?.OnReadTypeSetting(setting);
        }

        public virtual void OnReadAssetSetting(IGraphAssetLocalSetting setting)
        {
            parentHandle?.OnReadAssetSetting(setting);
        }

        public override void Dispose()
        {
            base.Dispose();
            smartValue = null;
            parentHandle = null;
        }
    }
}