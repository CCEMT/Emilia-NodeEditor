using Emilia.Kit;
using Emilia.Node.Attributes;
using Emilia.Node.Editor;
using Emilia.Variables.Editor;

namespace Emilia.Node.Universal.Editor
{
    [ObjectDescription(typeof(VariableKeySelectorAttribute))]
    public class VariableKeySelectorAttributeDescription : IObjectDescriptionGetter
    {
        public string GetDescription(object obj, object owner, object userData)
        {
            if (obj is not string key) return string.Empty;

            EditorGraphView editorGraphView = EditorGraphView.focusedGraphView;
            if (editorGraphView == null) return string.Empty;

            EditorUniversalGraphAsset universalGraphAsset = editorGraphView.graphAsset as EditorUniversalGraphAsset;
            if (universalGraphAsset == null) return string.Empty;
            if (universalGraphAsset.editorParametersManage == null) return string.Empty;

            EditorParameter editorParameter = universalGraphAsset.editorParametersManage.GetParameter(key);
            if (editorParameter == null) return string.Empty;
            return editorParameter.description;
        }
    }
}