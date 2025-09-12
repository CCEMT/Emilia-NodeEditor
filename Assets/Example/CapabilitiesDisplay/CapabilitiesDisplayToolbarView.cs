using System.Collections.Generic;
using Emilia.Kit;
using Emilia.Node.Attributes;
using Emilia.Node.Universal.Editor;
using Emilia.Variables.Editor;
using UnityEngine;

namespace Example
{
    public class CapabilitiesDisplayToolbarView : ToolbarView
    {
        protected override void InitControls()
        {
            AddControl(new ButtonToolbarViewControl("参数", OnEditorParameter));

            AddControl(new ButtonToolbarViewControl("保存", OnSave), ToolbarViewControlPosition.RightOrBottom);
        }

        protected virtual void OnEditorParameter()
        {
            CapabilitiesDisplayAsset displayAsset = graphView.graphAsset as CapabilitiesDisplayAsset;
            EditorParametersManager editorParametersManage = displayAsset.editorParametersManage;
            if (editorParametersManage == null)
            {
                editorParametersManage = displayAsset.editorParametersManage = ScriptableObject.CreateInstance<EditorParametersManager>();
                EditorAssetKit.SaveAssetIntoObject(editorParametersManage, displayAsset);
            }

            graphView.graphSelected.UpdateSelected(new List<ISelectedHandle> {editorParametersManage});
        }

        protected virtual void OnSave()
        {
            CapabilitiesDisplayAsset displayAsset = graphView.graphAsset as CapabilitiesDisplayAsset;
            displayAsset.Save();
        }
    }
}