using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Emilia.Kit.Editor
{
    /// <summary>
    /// 自动补全弹窗
    /// </summary>
    public class AutoCompletePopup
    {
        private List<AutoCompleteSuggestion> _suggestions;
        private int _selectedIndex;
        private Vector2 _scrollPosition;
        private Rect _popupScreenRect;
        private bool _isVisible;
        private Action<AutoCompleteSuggestion> _onSelect;

        private AutoCompleteStyles _styles;

        public bool IsVisible => _isVisible;

        public int SelectedIndex => _selectedIndex;

        public AutoCompletePopup(AutoCompleteStyles styles)
        {
            _styles = styles;
        }

        /// <summary>
        /// 显示弹窗
        /// </summary>
        public void Show(List<string> suggestions, Rect anchorRect, Action<string> onSelect)
        {
            List<AutoCompleteSuggestion> convertedSuggestions = suggestions?
                .Select(value => new AutoCompleteSuggestion(value, value, 0, int.MaxValue))
                .ToList();

            Show(convertedSuggestions, anchorRect, suggestion => onSelect?.Invoke(suggestion.insertText));
        }

        /// <summary>
        /// 显示弹窗
        /// </summary>
        public void Show(List<AutoCompleteSuggestion> suggestions, Rect anchorRect, Action<AutoCompleteSuggestion> onSelect)
        {
            if (suggestions == null || suggestions.Count == 0)
            {
                Hide();
                return;
            }

            _suggestions = suggestions;
            _selectedIndex = 0;
            _scrollPosition = Vector2.zero;
            _onSelect = onSelect;
            _isVisible = true;

            float itemHeight = EditorGUIUtility.singleLineHeight + 2;
            float popupHeight = Mathf.Min(suggestions.Count * itemHeight + 4, 150);
            Vector2 screenPosition = GUIUtility.GUIToScreenPoint(new Vector2(anchorRect.x, anchorRect.yMax + 1));
            _popupScreenRect = new Rect(screenPosition.x, screenPosition.y, anchorRect.width, popupHeight);
        }

        /// <summary>
        /// 隐藏弹窗
        /// </summary>
        public void Hide()
        {
            _isVisible = false;
            _suggestions = null;
        }

        public bool ContainsMousePosition(Vector2 mousePosition)
        {
            return _isVisible && GetPopupRect().Contains(mousePosition);
        }

        /// <summary>
        /// 处理弹窗内的键盘导航
        /// </summary>
        /// <returns>是否已处理事件</returns>
        public bool HandleKeyDown(Event e, out string selectedValue)
        {
            bool handled = HandleKeyDown(e, out AutoCompleteSuggestion selectedSuggestion);
            selectedValue = selectedSuggestion?.insertText;
            return handled;
        }

        /// <summary>
        /// 处理弹窗内的键盘导航
        /// </summary>
        /// <returns>是否已处理事件</returns>
        public bool HandleKeyDown(Event e, out AutoCompleteSuggestion selectedSuggestion)
        {
            selectedSuggestion = null;

            if (!_isVisible || _suggestions == null || _suggestions.Count == 0)
            {
                return false;
            }

            switch (e.keyCode)
            {
                case KeyCode.DownArrow:
                    _selectedIndex = Mathf.Min(_selectedIndex + 1, _suggestions.Count - 1);
                    return true;

                case KeyCode.UpArrow:
                    _selectedIndex = Mathf.Max(_selectedIndex - 1, 0);
                    return true;

                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if (_selectedIndex >= 0 && _selectedIndex < _suggestions.Count)
                    {
                        selectedSuggestion = _suggestions[_selectedIndex];
                    }
                    Hide();
                    return true;

                case KeyCode.Escape:
                    Hide();
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 绘制弹窗
        /// </summary>
        public void Draw()
        {
            if (!_isVisible || _suggestions == null || _suggestions.Count == 0)
            {
                return;
            }

            _styles.Init();

            Event e = Event.current;
            Rect popupRect = GetPopupRect();

            // 点击弹窗外部关闭
            if (e.type == EventType.MouseDown && !popupRect.Contains(e.mousePosition))
            {
                Hide();
                RequestRepaint();
                return;
            }

            // 绘制背景和边框
            EditorGUI.DrawRect(popupRect, AutoCompleteStyles.PopupBackgroundColor);
            DrawBorder(popupRect, AutoCompleteStyles.PopupBorderColor);

            // 绘制列表
            Rect innerRect = new Rect(popupRect.x + 1, popupRect.y + 1, popupRect.width - 2, popupRect.height - 2);
            GUILayout.BeginArea(innerRect);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            for (int i = 0; i < _suggestions.Count; i++)
            {
                var style = i == _selectedIndex ? _styles.SelectedItemStyle : _styles.ItemStyle;
                var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);

                // 鼠标悬停高亮
                Rect worldRect = new Rect(rect.x + innerRect.x, rect.y + innerRect.y - _scrollPosition.y, rect.width, rect.height);
                if (worldRect.Contains(e.mousePosition))
                {
                    if (_selectedIndex != i)
                    {
                        _selectedIndex = i;
                        RequestRepaint();
                    }
                    style = _styles.SelectedItemStyle;
                }

                if (GUI.Button(rect, GUIContent.none, style))
                {
                    _onSelect?.Invoke(_suggestions[i]);
                    Hide();
                    break;
                }

                DrawSuggestion(rect, _suggestions[i], i == _selectedIndex);
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private Rect GetPopupRect()
        {
            Vector2 guiPosition = GUIUtility.ScreenToGUIPoint(_popupScreenRect.position);
            return new Rect(guiPosition.x, guiPosition.y, _popupScreenRect.width, _popupScreenRect.height);
        }

        private void DrawSuggestion(Rect rect, AutoCompleteSuggestion suggestion, bool selected)
        {
            if (suggestion == null) return;

            GUIStyle itemStyle = selected ? _styles.SelectedItemStyle : _styles.ItemStyle;
            if (string.IsNullOrEmpty(suggestion.comment))
            {
                GUI.Label(rect, suggestion.displayText, itemStyle);
                return;
            }

            GUIStyle commentStyle = selected ? _styles.SelectedCommentStyle : _styles.CommentStyle;
            float commentWidth = Mathf.Min(rect.width * 0.45f, commentStyle.CalcSize(new GUIContent(suggestion.comment)).x + 12f);
            Rect nameRect = new Rect(rect.x, rect.y, rect.width - commentWidth, rect.height);
            Rect commentRect = new Rect(rect.xMax - commentWidth, rect.y, commentWidth, rect.height);

            GUI.Label(nameRect, suggestion.displayText, itemStyle);
            GUI.Label(commentRect, suggestion.comment, commentStyle);
        }

        private static void DrawBorder(Rect rect, Color color)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - 1, rect.y, 1, rect.height), color);
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
