using System;

namespace Emilia.Node.Editor
{
    public abstract class EditorHandle : IEditorHandle, IDisposable
    {
        private object _weakSmartValue;

        public IEditorHandle parent { get; set; }

        public object weakSmartValue => this._weakSmartValue;

        public virtual void Initialize(object weakSmartValue)
        {
            this._weakSmartValue = weakSmartValue;
        }

        public virtual void Dispose()
        {
            parent = null;
        }
    }
}