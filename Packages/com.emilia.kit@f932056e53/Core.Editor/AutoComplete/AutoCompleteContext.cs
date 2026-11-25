namespace Emilia.Kit.Editor
{
    /// <summary>
    /// 自动补全文本框当前输入状态。
    /// </summary>
    public sealed class AutoCompleteContext
    {
        public readonly string text;
        public readonly int cursorIndex;
        public readonly int selectionStart;
        public readonly int selectionEnd;

        public AutoCompleteContext(string text, int cursorIndex, int selectionStart, int selectionEnd)
        {
            this.text = text ?? string.Empty;
            this.cursorIndex = cursorIndex;
            this.selectionStart = selectionStart;
            this.selectionEnd = selectionEnd;
        }
    }
}
