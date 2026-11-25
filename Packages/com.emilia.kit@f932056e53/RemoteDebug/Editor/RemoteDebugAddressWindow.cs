using System;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Emilia.RemoteDebug.Editor
{
    public sealed class RemoteDebugAddressWindow : EditorWindow
    {
        private const string AddressControl = "RemoteDebugAddress";
        private RemoteDebugTargetMenu _menu;
        private EditorWindow _owner;
        private CancellationTokenSource _lifetime;
        private string _address, _error;
        private bool _connecting, _focused;

        public static void Open(Rect screenAnchor, RemoteDebugTargetMenu menu, EditorWindow owner)
        {
            var window = CreateInstance<RemoteDebugAddressWindow>();
            window.titleContent = new GUIContent("通过 IP 连接");
            window.hideFlags = HideFlags.HideAndDontSave;
            window._menu = menu;
            window._owner = owner;
            window._address = RemoteDebugEditorHost.Connections.LastAddress;
            window._lifetime = new CancellationTokenSource();
            window.ShowAsDropDown(screenAnchor, new Vector2(360, 142));
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("IP 地址（可带端口）");
            using (new EditorGUI.DisabledScope(_connecting))
            {
                GUI.SetNextControlName(AddressControl);
                _address = EditorGUILayout.TextField(_address ?? string.Empty);
            }
            if (! _focused)
            {
                _focused = true;
                EditorGUI.FocusTextInControl(AddressControl);
            }
            if (! string.IsNullOrEmpty(_error)) EditorGUILayout.HelpBox(_error, MessageType.Error);
            else EditorGUILayout.LabelField(_connecting ? "正在连接，请稍候…" : "例如 192.168.1.20 或 127.0.0.1:54999", EditorStyles.wordWrappedMiniLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            bool enter = Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);
            using (new EditorGUI.DisabledScope(_connecting || string.IsNullOrWhiteSpace(_address)))
                if (GUILayout.Button("连接") || (enter && ! _connecting))
                {
                    if (enter) Event.current.Use();
                    Connect();
                }
            if (GUILayout.Button("取消")) Close();
            EditorGUILayout.EndHorizontal();
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                Event.current.Use();
                Close();
            }
        }

        private async void Connect()
        {
            if (_connecting) return;
            if (! RemoteDebugConnectionAddress.TryParse(_address, out var address, out _error)) return;
            _address = address.Value;
            _connecting = true;
            try
            {
                bool connected = await _menu.ConnectAddressAsync(_address, _owner, _lifetime.Token);
                if (this == null) return;
                _connecting = false;
                if (connected) Close();
                else
                {
                    _error = _menu.LastError;
                    Repaint();
                }
            }
            catch (Exception error)
            {
                if (this == null) return;
                _connecting = false;
                _error = error.Message;
                Repaint();
            }
        }

        private void OnDisable()
        {
            _lifetime?.Cancel();
            _lifetime?.Dispose();
            _lifetime = null;
        }
    }
}