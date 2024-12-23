using System;

namespace Emilia.Node.Universal.Editor
{
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