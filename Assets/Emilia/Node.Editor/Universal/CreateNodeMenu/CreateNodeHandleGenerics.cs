using System;
using UnityEngine;

namespace Emilia.Node.Editor
{
    [GenericHandle]
    public abstract class CreateNodeHandle<T> : EditorHandle, ICreateNodeHandle
    {
        private CreateNodeHandleContext _value;
        public CreateNodeHandleContext value => this._value;

        public virtual Type editorNodeType => this._value.defaultEditorNodeType;

        public virtual object nodeData
        {
            get
            {
                if (parentCreateNodeHandle == null) return null;
                return parentCreateNodeHandle.nodeData;
            }
        }

        public virtual bool validity
        {
            get
            {
                if (parentCreateNodeHandle == null) return true;
                return parentCreateNodeHandle.validity;
            }
        }

        public virtual string path
        {
            get
            {
                if (parentCreateNodeHandle == null) return string.Empty;
                return parentCreateNodeHandle.path;
            }
        }

        public virtual int priority
        {
            get
            {
                if (parentCreateNodeHandle == null) return 0;
                return parentCreateNodeHandle.priority;
            }
        }

        public virtual Texture2D icon
        {
            get
            {
                if (parentCreateNodeHandle == null) return null;
                return parentCreateNodeHandle.icon;
            }
        }

        public ICreateNodeHandle parentCreateNodeHandle { get; private set; }

        public override void Initialize(object weakSmartValue)
        {
            base.Initialize(weakSmartValue);
            this._value = (CreateNodeHandleContext) weakSmartValue;
            parentCreateNodeHandle = parent as ICreateNodeHandle;
            OnInitialize();
        }

        public override void Dispose()
        {
            base.Dispose();
            this._value = default;
        }

        public virtual void OnInitialize()
        {
            parentCreateNodeHandle?.OnInitialize();
        }
    }
}