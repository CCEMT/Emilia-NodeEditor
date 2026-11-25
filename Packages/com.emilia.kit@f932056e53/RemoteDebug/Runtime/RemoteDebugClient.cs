using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Emilia.RemoteDebug
{
    public sealed class RemoteDebugClient : IDisposable
    {
        private readonly RemoteDebugEndpoint _endpoint;
        private readonly Dictionary<int, RemoteDebugTarget> _targets = new();
        private readonly Dictionary<string, RemoteDebugSession> _sessions = new();
        private readonly JsonRemoteDebugCodec _discoveryCodec = new();
        private readonly Func<double> _clock;
        private readonly RemoteDebugOptions _options;
        private double _nextDiscovery;
        private bool _disposed;
        public IReadOnlyList<RemoteDebugTarget> Targets => _targets.Values.ToArray();
        public event Action TargetsChanged;

        public RemoteDebugClient(IRemoteDebugTransport transport, Func<double> clock, RemoteDebugOptions options = null, Action<Exception> log = null)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _options = options ?? new RemoteDebugOptions();
            _options.Validate();
            _endpoint = new RemoteDebugEndpoint(transport, _options, clock, log);
            _endpoint.Received += Receive;
            _endpoint.Disconnected += Disconnected;
        }

        public async Task<RemoteDebugSession> ConnectAsync(RemoteDebugTarget target, string serviceId, IRemoteDebugCodec codec, int serviceVersion = 1)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RemoteDebugClient));
            if (target == null || codec == null || string.IsNullOrEmpty(serviceId)) throw new ArgumentException("Target, service and codec are required.");
            var session = new RemoteDebugSession(_endpoint, target, serviceId, serviceVersion, codec, _clock, _options);
            _sessions.Add(session.Id, session);
            session.Closed += () => _sessions.Remove(session.Id);
            try
            {
                await session.AttachAsync();
                return session;
            }
            catch
            {
                session.Dispose();
                throw;
            }
        }

        public void Discover()
        {
            if (_disposed) return;
            var peers = _endpoint.Transport.Peers;
            foreach (int id in _targets.Keys.Where(id => ! peers.Any(peer => peer.Id == id)).ToArray()) Disconnected(id);
            foreach (var peer in peers)
            {
                if (! _targets.ContainsKey(peer.Id)) _targets.Add(peer.Id, new RemoteDebugTarget(peer.Id, peer.Name));
                if (! _endpoint.HasQueued(peer.Id, string.Empty, string.Empty, RemoteDebugLane.Control))
                    _endpoint.Send(peer.Id, RemoteDebugLane.Control, new RemoteDebugEnvelope {Kind = RemoteDebugMessageKind.Discover, Control = true});
            }
        }

        private void Receive(int peer, RemoteDebugEnvelope message)
        {
            if (message.Kind == RemoteDebugMessageKind.Discovered)
            {
                if (! _targets.TryGetValue(peer, out var target)) return;
                target.Services = _discoveryCodec.Decode<RemoteDebugDiscovery>(message.Payload).Services;
                TargetsChanged?.Invoke();
                return;
            }
            if (_sessions.TryGetValue(message.Session, out var session) && session.Target.PeerId == peer && session.ServiceId == message.Service && session.ServiceVersion == message.ServiceVersion)
                session.Receive(message);
        }

        private void Disconnected(int peer)
        {
            foreach (var session in _sessions.Values.Where(value => value.Target.PeerId == peer).ToArray()) session.LoseConnection();
            if (_targets.Remove(peer)) TargetsChanged?.Invoke();
        }

        public void Tick()
        {
            if (_disposed) return;
            _endpoint.Tick();
            double now = _clock();
            if (now >= _nextDiscovery)
            {
                _nextDiscovery = now + _options.DiscoverySeconds;
                Discover();
            }
            foreach (var session in _sessions.Values.ToArray()) session.Tick(now);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            foreach (var session in _sessions.Values.ToArray()) session.Dispose();
            _endpoint.Tick();
            _endpoint.Dispose();
            _sessions.Clear();
            _targets.Clear();
            TargetsChanged = null;
        }
    }
}