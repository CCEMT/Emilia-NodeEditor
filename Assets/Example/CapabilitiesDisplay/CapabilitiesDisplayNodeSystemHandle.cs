using System;
using Emilia.Kit;
using Emilia.Node.Editor;
using UnityEngine;

namespace Example
{
    [EditorHandle(typeof(CapabilitiesDisplayAsset))]
    public class CapabilitiesDisplayNodeSystemHandle : NodeSystemHandle
    {
        public override void OnCreateNode(EditorGraphView graphView, IEditorNodeView editorNodeView)
        {
            base.OnCreateNode(graphView, editorNodeView);
            ChildrenDisplayNodeAsset stateMachineNodeAsset = editorNodeView.asset as ChildrenDisplayNodeAsset;
            if (stateMachineNodeAsset == null) return;

            Type stateMachineAsset = graphView.graphAsset.GetType();
            stateMachineNodeAsset.capabilitiesDisplayAsset = ScriptableObject.CreateInstance(stateMachineAsset) as CapabilitiesDisplayAsset;
            stateMachineNodeAsset.capabilitiesDisplayAsset.name = stateMachineNodeAsset.title;

            graphView.graphAsset.AddChild(stateMachineNodeAsset.capabilitiesDisplayAsset);
        }
    }
}