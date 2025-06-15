using Emilia.Kit;
using Emilia.Node.Attributes;
using Emilia.Node.Universal.Editor;

namespace Example.Expand
{
    public class ExpandToolbarView : ToolbarView
    {
        protected override void InitControls()
        {
            base.InitControls();

            AddControl(new ButtonToolbarViewControl("保存", OnSave), ToolbarViewControlPosition.RightOrBottom);
        }

        private void OnSave()
        {
            graphView.graphAsset.Save();
        }
    }
}