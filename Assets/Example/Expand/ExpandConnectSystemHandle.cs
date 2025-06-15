using System;
using Emilia.Kit;
using Emilia.Node.Editor;
using Emilia.Node.Universal.Editor;

namespace Example.Expand
{
    //连接拓展
    [EditorHandle(typeof(ExpandAsset))]
    public class ExpandConnectSystemHandle : UniversalConnectSystemHandle
    {
        //指定Edge类型
        public override Type GetEdgeAssetTypeByPort(EditorGraphView graphView, IEditorPortView portView) => typeof(EditorExpandEdgeAsset);

        //连接后拓展
        public override void AfterConnect(EditorGraphView graphView, IEditorEdgeView edgeView)
        {
            base.AfterConnect(graphView, edgeView);
            EditorExpandEdgeView editorExpandEdgeView = edgeView as EditorExpandEdgeView;
            editorExpandEdgeView?.OnCreate();
        }
    }
}