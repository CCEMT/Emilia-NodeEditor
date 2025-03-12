using System;

namespace Emilia.Node.Editor
{
    [GenericHandle]
    public abstract class ConnectSystemHandle<T> : EditorHandle, IConnectSystemHandle where T : EditorGraphAsset
    {
        public EditorGraphView smartValue { get; private set; }
        public IConnectSystemHandle parentHandle { get; private set; }

        public virtual Type connectorListenerType => typeof(EditorEdgeConnectorListener);

        public override void Initialize(object weakSmartValue)
        {
            base.Initialize(weakSmartValue);
            smartValue = weakSmartValue as EditorGraphView;
            parentHandle = parent as IConnectSystemHandle;
            OnInitialize();
        }

        protected virtual void OnInitialize() { }

        public virtual Type GetEdgeTypeByPort(IEditorPortView portView)
        {
            return parentHandle?.GetEdgeTypeByPort(portView);
        }

        public virtual bool CanConnect(IEditorPortView inputPort, IEditorPortView outputPort)
        {
            return parentHandle?.CanConnect(inputPort, outputPort) ?? false;
        }

        public virtual bool BeforeConnect(IEditorPortView input, IEditorPortView output)
        {
            return parentHandle?.BeforeConnect(input, output) ?? false;
        }

        public virtual void AfterConnect(IEditorEdgeView edgeView)
        {
            parentHandle?.AfterConnect(edgeView);
        }

        public override void Dispose()
        {
            base.Dispose();
            smartValue = null;
            parentHandle = null;
        }
    }
}