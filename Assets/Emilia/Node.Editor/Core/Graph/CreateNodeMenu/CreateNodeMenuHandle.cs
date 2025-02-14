using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Emilia.Node.Editor
{
    [GenericHandle]
    public abstract class CreateNodeMenuHandle<T> : EditorHandle, ICreateNodeMenuHandle where T : EditorGraphAsset
    {
        public EditorGraphView smartValue { get; private set; }

        public ICreateNodeMenuHandle parentHandle { get; private set; }
        public virtual string title => "Create Node";

        public override void Initialize(object weakSmartValue)
        {
            base.Initialize(weakSmartValue);
            smartValue = weakSmartValue as EditorGraphView;
            parentHandle = parent as ICreateNodeMenuHandle;
        }

        public virtual void InitializeCache()
        {
            parentHandle?.InitializeCache();
        }

        public virtual void MenuCreateInitialize(CreateNodeContext createNodeContext)
        {
            parentHandle?.MenuCreateInitialize(createNodeContext);
        }

        public virtual void ShowCreateNodeMenu(NodeCreationContext c)
        {
            parentHandle?.ShowCreateNodeMenu(c);
        }

        public virtual void CollectAllCreateNodeInfos(List<CreateNodeInfo> createNodeInfos, CreateNodeContext createNodeContext)
        {
            parentHandle?.CollectAllCreateNodeInfos(createNodeInfos, createNodeContext);
        }

        public override void Dispose()
        {
            base.Dispose();
            smartValue = null;
            parentHandle = null;
        }
    }
}