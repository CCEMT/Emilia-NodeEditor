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

        public virtual Type settingType
        {
            get
            {
                if (parentHandle == null) return null;
                return parentHandle.settingType;
            }
        }

        public virtual void OnReadSetting(IGraphLocalSetting setting)
        {
            parentHandle?.OnReadSetting(setting);
        }

        public override void Dispose()
        {
            base.Dispose();
            smartValue = null;
            parentHandle = null;
        }
    }
}