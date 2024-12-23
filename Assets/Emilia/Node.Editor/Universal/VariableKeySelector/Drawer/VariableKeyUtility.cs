using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    public static class VariableKeyUtility
    {
        public static string GetDescription(string key)
        {
            EditorGraphView editorGraphView = EditorGraphView.focusedGraphView;
            if (editorGraphView == null) return string.Empty;
            
            EditorUniversalGraphAsset universalGraphAsset = editorGraphView.graphAsset as EditorUniversalGraphAsset;
            if (universalGraphAsset == null) return string.Empty;
            if (universalGraphAsset.editorParametersManage == null) return string.Empty;

            EditorParameter editorParameter = universalGraphAsset.editorParametersManage.parameters.Find((p) => p.key == key);
            if (editorParameter == null) return string.Empty;
            
            return editorParameter.description;
        }
    }
}