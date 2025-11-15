using System;

namespace Emilia.Node.Universal.Editor
{
    /// <summary>
    /// 工具栏自定义控件
    /// </summary>
    public class CustomToolbarViewControl : ToolbarViewControl
    {
        public Action onCustom;

        public CustomToolbarViewControl(Action onCustom)
        {
            this.onCustom = onCustom;
        }

        public override void OnDraw()
        {
            onCustom?.Invoke();
        }
    }
}