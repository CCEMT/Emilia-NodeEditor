using System;

namespace Emilia.Kit.Editor
{
    /// <summary>
    /// 自动补全候选项。
    /// </summary>
    public sealed class AutoCompleteSuggestion
    {
        public readonly string displayText;
        public readonly string insertText;
        public readonly int replacementStart;
        public readonly int replacementLength;
        public readonly int caretIndex;
        public readonly string comment;

        public AutoCompleteSuggestion(string displayText, string insertText, int replacementStart, int replacementLength, int caretIndex = -1, string comment = null)
        {
            this.displayText = displayText ?? string.Empty;
            this.insertText = insertText ?? this.displayText;
            this.replacementStart = replacementStart;
            this.replacementLength = replacementLength;
            this.caretIndex = caretIndex;
            this.comment = comment ?? string.Empty;
        }

        public string Apply(string text, out int newCaretIndex)
        {
            text ??= string.Empty;

            int start = Math.Max(0, Math.Min(replacementStart, text.Length));
            int length = Math.Max(0, Math.Min(replacementLength, text.Length - start));
            string result = text.Remove(start, length).Insert(start, insertText);

            int resolvedCaretIndex = caretIndex >= 0 ? caretIndex : start + insertText.Length;
            newCaretIndex = Math.Max(0, Math.Min(resolvedCaretIndex, result.Length));
            return result;
        }
    }
}
