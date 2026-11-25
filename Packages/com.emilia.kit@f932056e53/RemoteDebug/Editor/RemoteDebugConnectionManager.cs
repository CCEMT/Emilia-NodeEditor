using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Emilia.RemoteDebug.Editor
{
    public sealed class RemoteDebugConnectionManager : IDisposable
    {
        private sealed class Attempt
        {
            public RemoteDebugConnectionTarget Target;
            public string Service;
            public int Version;
            public CancellationToken Token;
            public bool Armed, Started, Probing;
            public int Peer;
            public double Deadline, NextProbe;
            public IReadOnlyList<RemoteDebugServiceDescription> PreviousServices;
            public readonly TaskCompletionSource<RemoteDebugTarget> Completion = new();
        }

        public const double ConnectionTimeoutSeconds = 10;
        public const double RefreshSeconds = 1;
        private readonly IRemoteDebugConnectionBackend _backend;
        private readonly Func<double> _clock;
        private readonly Action<string> _saveAddress;
        private IReadOnlyList<RemoteDebugConnectionTarget> _targets = Array.Empty<RemoteDebugConnectionTarget>();
        private Attempt _pending;
        private bool _disposed;
        private double _nextRefresh;
        public IReadOnlyList<RemoteDebugConnectionTarget> Targets => _targets;
        public bool IsConnecting => _pending != null;
        public string ConnectingTargetKey => _pending?.Target.Key;
        public string LastAddress { get; private set; }
        public event Action Changed;

        public RemoteDebugConnectionManager(IRemoteDebugConnectionBackend backend, Func<double> clock,
            string lastAddress = null, Action<string> saveAddress = null)
        {
            _backend = backend ?? throw new ArgumentNullException(nameof(backend));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            LastAddress = lastAddress ?? string.Empty;
            _saveAddress = saveAddress;
        }

        public void Refresh()
        {
            if (_disposed) return;
            var available = _backend.GetAvailableTargets();
            var merged = new List<RemoteDebugConnectionTarget>();
            // USB 信息比通用的已连接 Player 名称更明确，优先保留同一 Peer 的 USB 项。
            foreach (var target in available.OrderBy(value => value.Kind == RemoteDebugConnectionKind.Device ? 0 : 1))
            {
                if (merged.Any(value => value.Key == target.Key || (target.PeerId.HasValue && value.PeerId == target.PeerId))) continue;
                merged.Add(target);
            }
            foreach (var peer in _backend.ConnectedPeers)
                if (! merged.Any(target => target.PeerId == peer.Id))
                    merged.Add(new RemoteDebugConnectionTarget("peer:" + peer.Id, peer.Name, RemoteDebugConnectionKind.ConnectedPlayer, peerId: peer.Id));
            var next = merged.OrderBy(value => value.Name, StringComparer.OrdinalIgnoreCase).ThenBy(value => value.Key, StringComparer.Ordinal).ToArray();
            bool changed = next.Length != _targets.Count || next.Where((target, index) => ! Equivalent(target, _targets[index])).Any();
            _targets = next;
            _nextRefresh = _clock() + RefreshSeconds;
            if (changed) Changed?.Invoke();
        }

        private static bool Equivalent(RemoteDebugConnectionTarget first, RemoteDebugConnectionTarget second) => first.Key == second.Key &&
                                                                                                                 first.Name == second.Name && first.Kind == second.Kind &&
                                                                                                                 first.PeerId == second.PeerId && first.NativeId == second.NativeId &&
                                                                                                                 first.Address == second.Address && first.CanConnect == second.CanConnect &&
                                                                                                                 first.UnavailableReason == second.UnavailableReason;

        public Task<RemoteDebugTarget> ConnectPlayerAsync(RemoteDebugConnectionTarget target, string serviceId, int version,
            CancellationToken cancellationToken = default)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (! target.CanConnect) return Task.FromException<RemoteDebugTarget>(new RemoteDebugException("TargetUnavailable", target.UnavailableReason));
            return Begin(target, serviceId, version, cancellationToken);
        }

        public Task<RemoteDebugTarget> ConnectAddressAsync(string address, string serviceId, int version,
            CancellationToken cancellationToken = default)
        {
            if (! RemoteDebugConnectionAddress.TryParse(address, out var parsed, out string error)) return Task.FromException<RemoteDebugTarget>(new ArgumentException(error, nameof(address)));
            return Begin(new RemoteDebugConnectionTarget("address:" + parsed.Value, parsed.Value, RemoteDebugConnectionKind.Address,
                address: parsed.Value), serviceId, version, cancellationToken);
        }

        private Task<RemoteDebugTarget> Begin(RemoteDebugConnectionTarget target, string serviceId, int version, CancellationToken token)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RemoteDebugConnectionManager));
            if (string.IsNullOrWhiteSpace(serviceId) || version <= 0) throw new ArgumentException("Invalid service identifier or version.");
            if (token.IsCancellationRequested) return Task.FromCanceled<RemoteDebugTarget>(token);
            CancelPending();
            var attempt = new Attempt {Target = target, Service = serviceId, Version = version, Token = token};
            _pending = attempt;
            Changed?.Invoke();
            return attempt.Completion.Task;
        }

        public void Tick()
        {
            if (_disposed) return;
            if (_clock() >= _nextRefresh) Refresh();
            var attempt = _pending;
            if (attempt == null) return;
            if (attempt.Token.IsCancellationRequested)
            {
                CancelPending();
                return;
            }
            if (! attempt.Armed)
            {
                attempt.Armed = true;
                return;
            }
            try
            {
                if (! attempt.Started)
                {
                    if (attempt.Target.PeerId.HasValue && _backend.ConnectedPeers.Any(peer => peer.Id == attempt.Target.PeerId.Value)) attempt.Peer = attempt.Target.PeerId.Value;
                    else if (attempt.Target.Kind == RemoteDebugConnectionKind.Address) attempt.Peer = ConnectAddress(attempt.Target.Address);
                    else attempt.Peer = _backend.ConnectPlayer(attempt.Target);
                    attempt.Started = true;
                    attempt.Deadline = _clock() + ConnectionTimeoutSeconds;
                    // 原生连接可能阻塞；返回后再次检查，避免取消过的操作进入业务会话。
                    if (_disposed || ! ReferenceEquals(_pending, attempt)) return;
                    if (attempt.Token.IsCancellationRequested)
                    {
                        CancelPending();
                        return;
                    }
                }
                bool connected = _backend.ConnectedPeers.Any(peer => peer.Id == attempt.Peer);
                if (! connected && attempt.Probing) throw new RemoteDebugException("ConnectionLost", "设备连接已断开，请重新连接。");
                if (connected)
                {
                    var target = _backend.GetServiceTarget(attempt.Peer);
                    if (! attempt.Probing)
                    {
                        attempt.PreviousServices = target?.Services;
                        attempt.Probing = true;
                        attempt.NextProbe = _clock();
                    }
                    // Discover 先创建 Services=Array.Empty 的占位目标；它尚未收到设备回执。
                    else if (target != null && ! ReferenceEquals(target.Services, Array.Empty<RemoteDebugServiceDescription>()) &&
                             ! ReferenceEquals(target.Services, attempt.PreviousServices))
                    {
                        var service = target.FindService(attempt.Service);
                        if (service == null) throw new RemoteDebugException("ServiceMissing", "设备已连接，但未启用当前调试服务。");
                        if (service.Version != attempt.Version) throw new RemoteDebugException("ServiceVersionMismatch", "设备的调试服务版本与编辑器不一致。");
                        _pending = null;
                        if (attempt.Target.Kind == RemoteDebugConnectionKind.Address)
                        {
                            LastAddress = attempt.Target.Address;
                            _saveAddress?.Invoke(LastAddress);
                        }
                        attempt.Completion.TrySetResult(target);
                        Changed?.Invoke();
                        return;
                    }
                    if (_clock() >= attempt.NextProbe)
                    {
                        attempt.NextProbe = _clock() + RefreshSeconds;
                        _backend.DiscoverServices();
                    }
                }
                if (_clock() >= attempt.Deadline)
                    throw new RemoteDebugException(connected ? "ServiceTimeout" : "ConnectionTimeout", connected
                        ? "设备已连接，但没有收到调试服务响应，请检查客户端是否启用了真机调试。"
                        : "连接设备超时，请确认客户端正在运行。");
            }
            catch (Exception error)
            {
                if (! ReferenceEquals(_pending, attempt)) return;
                _pending = null;
                attempt.Completion.TrySetException(error);
                Changed?.Invoke();
            }
        }

        private int ConnectAddress(string text)
        {
            RemoteDebugConnectionAddress.TryParse(text, out var address, out _);
            return _backend.ConnectAddress(address);
        }

        public void CancelPending()
        {
            var previous = _pending;
            _pending = null;
            if (previous == null) return;
            previous.Completion.TrySetCanceled();
            Changed?.Invoke();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            CancelPending();
            Changed = null;
        }
    }
}