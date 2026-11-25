using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Emilia.Kit.Editor
{
    /// <summary>
    /// 带自动补全功能的文本输入框
    /// </summary>
    public class AutoCompleteTextField
    {
        private readonly AutoCompleteStyles _styles;
        private readonly TextFieldInputHandler _inputHandler;
        private readonly AutoCompletePopup _popup;

        private string _lastText;
        private AutoCompleteSuggestion _pendingSuggestion;
        private int _lastCursorIndex = -1;
        private int _controlId;
        private bool _hadFocus;

        /// <summary>
        /// 弹窗是否显示中
        /// </summary>
        public bool IsPopupVisible => _popup.IsVisible;

        public AutoCompleteTextField()
        {
            _styles = new AutoCompleteStyles();
            _inputHandler = new TextFieldInputHandler();
            _popup = new AutoCompletePopup(_styles);
        }

        /// <summary>
        /// 绘制带自动补全的文本输入框
        /// </summary>
        public string Draw(Rect position, string text, Func<string, IEnumerable<string>> getSuggestions,
            GUIContent label = null, Rect? popupAnchorRect = null)
        {
            return Draw(position, text, context => {
                IEnumerable<string> suggestions = getSuggestions?.Invoke(context.text);
                return suggestions?.Select(value => new AutoCompleteSuggestion(value, value, 0, context.text.Length));
            }, label, popupAnchorRect);
        }

        /// <summary>
        /// 绘制带自动补全的文本输入框
        /// </summary>
        public string Draw(Rect position, string text, Func<AutoCompleteContext, IEnumerable<AutoCompleteSuggestion>> getSuggestions,
            GUIContent label = null, Rect? popupAnchorRect = null)
        {
            _styles.Init();

            int controlId = GUIUtility.GetControlID(FocusType.Keyboard);
            _controlId = controlId;

            // 计算文本框位置
            Rect textFieldRect = position;
            if (label != null)
            {
                textFieldRect = EditorGUI.PrefixLabel(position, controlId, label);
            }
            Rect resolvedPopupAnchorRect = popupAnchorRect ?? textFieldRect;

            // 检查是否有待应用的候选项
            if (_pendingSuggestion != null)
            {
                text = _pendingSuggestion.Apply(text, out int caretIndex);
                _pendingSuggestion = null;
                _lastText = text;
                _lastCursorIndex = caretIndex;
                _popup.Hide();
                _inputHandler.SetCursor(text, caretIndex);
                GUI.changed = true;
            }

            Event e = Event.current;
            string newText = text ?? "";

            // 处理鼠标点击
            if (e.type == EventType.MouseDown && e.button == 0 && textFieldRect.Contains(e.mousePosition))
            {
                GUIUtility.keyboardControl = controlId;
                _inputHandler.HandleMouseDown(newText, textFieldRect, e.mousePosition, e.clickCount);
                e.Use();
                RequestRepaint();
            }

            bool hasFocus = GUIUtility.keyboardControl == controlId;
            bool gainedFocus = hasFocus && !_hadFocus;

            if (hasFocus && e.type == EventType.MouseDrag && e.button == 0 && _inputHandler.IsDragging)
            {
                _inputHandler.HandleMouseDrag(newText, textFieldRect, e.mousePosition);
                e.Use();
                RequestRepaint();
            }

            if (e.type == EventType.MouseUp && _inputHandler.IsDragging)
            {
                _inputHandler.HandleMouseUp();
                if (hasFocus)
                {
                    e.Use();
                    RequestRepaint();
                }
            }

            if (!hasFocus)
            {
                if (_inputHandler.IsDragging)
                    _inputHandler.HandleMouseUp();

                if (_popup.IsVisible && !IsMouseEventInsidePopup(e))
                {
                    _popup.Hide();
                    RequestRepaint();
                }
            }

            // 处理键盘输入
            if (hasFocus && e.type == EventType.KeyDown)
            {
                bool handled = false;

                // 先让弹窗处理导航键
                if (_popup.IsVisible)
                {
                    handled = _popup.HandleKeyDown(e, out AutoCompleteSuggestion selectedSuggestion);
                    if (selectedSuggestion != null)
                    {
                        newText = selectedSuggestion.Apply(newText, out int caretIndex);
                        _inputHandler.SetCursor(newText, caretIndex);
                        _lastText = newText;
                        _lastCursorIndex = caretIndex;
                    }
                }

                // 弹窗未处理则由输入处理器处理
                if (!handled)
                {
                    newText = _inputHandler.HandleKeyDown(newText, e, out handled);
                }

                if (handled)
                {
                    e.Use();
                }
            }

            // 处理字符输入
            if (hasFocus && e.type == EventType.KeyDown && e.character != 0 && !char.IsControl(e.character))
            {
                newText = _inputHandler.HandleCharacterInput(newText, e.character);
                e.Use();
            }

            // 绘制文本框
            if (e.type == EventType.Repaint)
            {
                DrawTextField(textFieldRect, newText, controlId, hasFocus, e);
            }

            // 更新光标闪烁
            if (hasFocus && _inputHandler.UpdateCursorBlink())
            {
                RequestRepaint();
            }

            // 文本变化或光标变化时更新弹窗
            bool textOrCursorChanged = newText != _lastText || _inputHandler.CursorIndex != _lastCursorIndex;
            if (gainedFocus || textOrCursorChanged)
            {
                _lastText = newText;
                _lastCursorIndex = _inputHandler.CursorIndex;
                if (hasFocus)
                    UpdatePopup(newText, resolvedPopupAnchorRect, getSuggestions);
                if (textOrCursorChanged)
                    GUI.changed = true;
            }

            _hadFocus = hasFocus;

            return newText;
        }

        /// <summary>
        /// 在 OnGUI 的最后调用此方法来绘制弹窗
        /// </summary>
        public void DrawPopup()
        {
            if (_popup.IsVisible && _controlId != 0 && GUIUtility.keyboardControl != _controlId && !IsMouseEventInsidePopup(Event.current))
            {
                _popup.Hide();
                return;
            }

            _popup.Draw();
        }

        /// <summary>
        /// 绘制带自动补全的文本输入框（Layout版本）
        /// </summary>
        public string DrawLayout(string text, Func<string, IEnumerable<string>> getSuggestions,
            GUIContent label = null, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, options);
            return Draw(rect, text, getSuggestions, label);
        }

        /// <summary>
        /// 绘制带自动补全的文本输入框（Layout版本）
        /// </summary>
        public string DrawLayout(string text, Func<AutoCompleteContext, IEnumerable<AutoCompleteSuggestion>> getSuggestions,
            GUIContent label = null, params GUILayoutOption[] options)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, options);
            return Draw(rect, text, getSuggestions, label);
        }

        /// <summary>
        /// 绘制带自动补全的文本输入框（Layout版本，字符串标签）
        /// </summary>
        public string DrawLayout(string label, string text, Func<string, IEnumerable<string>> getSuggestions,
            params GUILayoutOption[] options)
        {
            return DrawLayout(text, getSuggestions, new GUIContent(label), options);
        }

        /// <summary>
        /// 绘制带自动补全的文本输入框（Layout版本，字符串标签）
        /// </summary>
        public string DrawLayout(string label, string text, Func<AutoCompleteContext, IEnumerable<AutoCompleteSuggestion>> getSuggestions,
            params GUILayoutOption[] options)
        {
            return DrawLayout(text, getSuggestions, new GUIContent(label), options);
        }

        /// <summary>
        /// 关闭弹窗
        /// </summary>
        public void ClosePopup()
        {
            _popup.Hide();
        }

        private bool IsMouseEventInsidePopup(Event e)
        {
            if (e == null) return false;

            switch (e.type)
            {
                case EventType.MouseDown:
                case EventType.MouseUp:
                case EventType.MouseDrag:
                case EventType.MouseMove:
                    return _popup.ContainsMousePosition(e.mousePosition);
                default:
                    return false;
            }
        }

        private void DrawTextField(Rect textFieldRect, string text, int controlId, bool hasFocus, Event e)
        {
            // 绘制背景
            _styles.TextFieldStyle.Draw(textFieldRect, GUIContent.none, controlId, false, textFieldRect.Contains(e.mousePosition));

            Rect textRect = new Rect(textFieldRect.x + 2, textFieldRect.y, textFieldRect.width - 4, textFieldRect.height);

            // 绘制选择高亮
            if (hasFocus && _inputHandler.HasSelection)
            {
                float startX = TextFieldInputHandler.GetTextWidth(text.Substring(0, _inputHandler.SelectionStart)) + textRect.x;
                float endX = TextFieldInputHandler.GetTextWidth(text.Substring(0, _inputHandler.SelectionEnd)) + textRect.x;
                Rect selectionRect = new Rect(startX, textRect.y + 2, endX - startX, textRect.height - 4);
                EditorGUI.DrawRect(selectionRect, AutoCompleteStyles.SelectionColor);
            }

            // 绘制文本
            GUI.Label(textRect, text, EditorStyles.label);

            // 绘制光标
            if (hasFocus && _inputHandler.CursorVisible)
            {
                float cursorX = TextFieldInputHandler.GetTextWidth(text.Substring(0, _inputHandler.CursorIndex)) + textRect.x;
                Rect cursorRect = new Rect(cursorX, textRect.y + 2, 1, textRect.height - 4);
                EditorGUI.DrawRect(cursorRect, AutoCompleteStyles.CursorColor);
            }
        }

        private void UpdatePopup(string text, Rect popupAnchorRect, Func<AutoCompleteContext, IEnumerable<AutoCompleteSuggestion>> getSuggestions)
        {
            if (string.IsNullOrEmpty(text))
            {
                _popup.Hide();
                return;
            }

            AutoCompleteContext context = new AutoCompleteContext(text, _inputHandler.CursorIndex, _inputHandler.SelectionStart, _inputHandler.SelectionEnd);
            List<AutoCompleteSuggestion> suggestions = getSuggestions?.Invoke(context)?
                .Where(suggestion => suggestion != null && ! string.IsNullOrEmpty(suggestion.displayText))
                .ToList();
            if (suggestions != null && suggestions.Count > 0)
            {
                _popup.Show(suggestions, popupAnchorRect, selected =>
                {
                    _pendingSuggestion = selected;
                    RequestRepaint();
                });
            }
            else
            {
                _popup.Hide();
            }
        }

        private static void RequestRepaint()
        {
            if (EditorWindow.focusedWindow != null)
            {
                EditorWindow.focusedWindow.Repaint();
            }
        }
    }
}
