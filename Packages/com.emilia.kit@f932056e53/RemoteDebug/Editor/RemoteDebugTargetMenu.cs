using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Emilia.RemoteDebug.Editor
{
    public sealed class RemoteDebugTargetMenu : IDisposable
    {
        private readonly string _service;
        private readonly int _version;
        private readonly Func<RemoteDebugTarget, CancellationToken, Task> _connect;
        private readonly Action<string> _report;
        private CancellationTokenSource _request;
        private EditorWindow _owner;
        private bool _hasOwner, _disposed;
        private int _generation;
        public bool IsConnecting => _request != null;
        public string LastError { get; private set; }

        public RemoteDebugTargetMenu(string serviceId, int version, Func<RemoteDebugTarget, CancellationToken, Task> connect,
            Action<string> report)
        {
            _service = serviceId;
            _version = version;
            _connect = connect ?? throw new ArgumentNullException(nameof(connect));
            _report = report ?? throw new ArgumentNullException(nameof(report));
            EditorApplication.update += CheckOwner;
        }

        public void AddItems(GenericMenu menu, Rect anchor, EditorWindow owner, int? activePeer)
        {
            if (_disposed) return;
            var connections = RemoteDebugEditorHost.Connections;
            connections.Refresh();
            foreach (var target in connections.Targets)
            {
                var captured = target;
                string suffix = target.PeerId.HasValue ? "（已连接）" : "（未连接）";
                bool enabled = target.CanConnect;
                if (! enabled) suffix = "（" + target.UnavailableReason + "）";
                else if (connections.ConnectingTargetKey == target.Key)
                {
                    suffix = "（连接中…）";
                    enabled = false;
                }
                else if (target.PeerId.HasValue)
                {
                    var known = RemoteDebugEditorHost.Client.Targets.FirstOrDefault(value => value.PeerId == target.PeerId.Value);
                    var service = known?.FindService(_service);
                    if (service != null && service.Version != _version)
                    {
                        suffix = "（调试版本不匹配）";
                        enabled = false;
                    }
                    else if (known != null && known.Services.Count > 0 && service == null)
                    {
                        suffix = "（未启用当前调试服务）";
                        enabled = false;
                    }
                }
                string label = target.Name.Replace('/', '／') + (target.Kind == RemoteDebugConnectionKind.Device ? " [USB]" : string.Empty) + suffix;
                if (enabled)
                    menu.AddItem(new GUIContent(label), target.PeerId.HasValue && target.PeerId == activePeer,
                        () => Observe(ConnectPlayerAsync(captured, owner)));
                else menu.AddDisabledItem(new GUIContent(label));
            }
            if (connections.Targets.Count == 0) menu.AddDisabledItem(new GUIContent("未发现可连接设备"));
            Rect screenAnchor = GUIUtility.GUIToScreenRect(anchor);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("通过 IP 连接…"), false, () => RemoteDebugAddressWindow.Open(screenAnchor, this, owner));
            menu.AddItem(new GUIContent("刷新设备列表"), false, () => {
                connections.Refresh();
                RemoteDebugEditorHost.Client.Discover();
                _report("设备列表已刷新。");
            });
            if (IsConnecting) menu.AddItem(new GUIContent("取消连接"), false, Cancel);
        }

        public Task<bool> ConnectPlayerAsync(RemoteDebugConnectionTarget target, EditorWindow owner, CancellationToken token = default) =>
            Run(cancellation => RemoteDebugEditorHost.ConnectPlayerAsync(target, _service, _version, cancellation), target.Name, owner, token);

        public Task<bool> ConnectAddressAsync(string address, EditorWindow owner, CancellationToken token = default) =>
            Run(cancellation => RemoteDebugEditorHost.ConnectAddressAsync(address, _service, _version, cancellation), address, owner, token);

        private async Task<bool> Run(Func<CancellationToken, Task<RemoteDebugTarget>> connect, string name, EditorWindow owner, CancellationToken token)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RemoteDebugTargetMenu));
            Cancel();
            var request = CancellationTokenSource.CreateLinkedTokenSource(token);
            _request = request;
            _owner = owner;
            _hasOwner = owner != null;
            int generation = _generation;
            LastError = null;
            _report("正在连接 " + name + "…");
            try
            {
                var target = await connect(request.Token);
                RequireCurrent(request, generation);
                await _connect(target, request.Token);
                RequireCurrent(request, generation);
                _report("已连接 " + target.Name);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception error)
            {
                if (ReferenceEquals(_request, request) && generation == _generation && (! _hasOwner || _owner != null))
                {
                    LastError = error.Message;
                    _report(error.Message);
                }
                return false;
            }
            finally
            {
                if (ReferenceEquals(_request, request))
                {
                    _request = null;
                    _owner = null;
                    _hasOwner = false;
                }
                request.Dispose();
            }
        }

        private void RequireCurrent(CancellationTokenSource request, int generation)
        {
            request.Token.ThrowIfCancellationRequested();
            if (_disposed || ! ReferenceEquals(_request, request) || generation != _generation || (_hasOwner && _owner == null)) throw new OperationCanceledException();
        }

        private static async void Observe(Task<bool> task)
        {
            await task;
        }

        private void CheckOwner()
        {
            if (_request != null && _hasOwner && _owner == null) Cancel();
        }

        public void Cancel()
        {
            _generation++;
            var previous = _request;
            _request = null;
            _owner = null;
            _hasOwner = false;
            previous?.Cancel();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Cancel();
            EditorApplication.update -= CheckOwner;
        }
    }
}