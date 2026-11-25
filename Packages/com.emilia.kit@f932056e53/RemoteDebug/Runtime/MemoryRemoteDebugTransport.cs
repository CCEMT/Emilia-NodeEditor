using System;
using System.Collections.Generic;

namespace Emilia.RemoteDebug
{
    public sealed class MemoryRemoteDebugTransport : IRemoteDebugTransport
    {
        private readonly Queue<(RemoteDebugLane lane, byte[] frame)> _incoming = new();
        private MemoryRemoteDebugTransport _other;
        private bool _disposed;
        private static readonly RemoteDebugPeer[] InProcessPeer = {new(1, "In Process")};

        public bool Blocked { get; set; }

        // 返回 false 模拟丢包；TrySend 仍成功，便于验证回执丢失后的重试。
        public Func<RemoteDebugLane, byte[], bool> Deliver { get; set; }

        public IReadOnlyList<RemoteDebugPeer> Peers => _disposed || _other == null || _other._disposed
            ? Array.Empty<RemoteDebugPeer>()
            : InProcessPeer;

        public event Action<int, RemoteDebugLane, byte[]> Received;
        public event Action<int> Disconnected;

        public static void CreatePair(out MemoryRemoteDebugTransport first, out MemoryRemoteDebugTransport second)
        {
            first = new MemoryRemoteDebugTransport();
            second = new MemoryRemoteDebugTransport();
            first._other = second;
            second._other = first;
        }

        public bool TrySend(int peer, RemoteDebugLane lane, byte[] frame)
        {
            if (peer != 1 || Blocked || Peers.Count == 0) return false;
            if (Deliver == null || Deliver(lane, frame)) _other._incoming.Enqueue((lane, (byte[]) frame.Clone()));
            return true;
        }

        public void Tick()
        {
            int count = _incoming.Count;
            while (! _disposed && count-- > 0)
            {
                var item = _incoming.Dequeue();
                Received?.Invoke(1, item.lane, item.frame);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _incoming.Clear();
            Disconnected?.Invoke(1);
            if (_other != null && ! _other._disposed) _other.Disconnected?.Invoke(1);
            Received = null;
            Disconnected = null;
        }
    }
}