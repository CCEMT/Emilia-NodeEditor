using System.Collections.Generic;
using Emilia.Node.Editor;
using Sirenix.OdinInspector;

namespace Emilia.Node.Universal.Editor
{
    [HideMonoScript, OnValueChanged(nameof(OnValueChanged), true)]
    public abstract class UniversalItemAsset : EditorItemAsset
    {
        protected virtual void OnValueChanged()
        {
            EditorGraphView graphView = EditorGraphView.GetGraphView(graphAsset);
            if (graphView == null) return;
            IEditorItemView itemView = graphView.graphElementCache.itemViewById.GetValueOrDefault(id);
            if (itemView == null) return;
            itemView.OnValueChanged();
        }
    }
}