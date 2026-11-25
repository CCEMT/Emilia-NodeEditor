using System;
using System.Collections.Generic;
using System.Linq;

namespace Emilia.RemoteDebug
{
    public sealed class RemoteDebugEndpoint : IDisposable
    {
        private sealed class Outbound
        {
            public int Peer;
            public RemoteDebugEnvelope Envelope;
            public IEnumerator<byte[]> Frames;
            public byte[] Current;
        }

        private readonly Queue<Outbound>[] _outgoing = {new(), new(), new()};
        private readonly RemoteDebugWire.Assembler _assembler;
        private readonly Func<double> _clock;
        private readonly Action<Exception> _log;
        private readonly RemoteDebugOptions _options;
        private bool _disposed;
        public IRemoteDebugTransport Transport { get; }
        public event Action<int, RemoteDebugEnvelope> Received;
        public event Action<int> Disconnected;

        public RemoteDebugEndpoint(IRemoteDebugTransport transport, RemoteDebugOptions options, Func<double> clock, Action<Exception> log)
        {
            Transport = transport;
            _options = options;
            _clock = clock;
            _log = log;
            _assembler = new RemoteDebugWire.Assembler(options);
            transport.Received += OnReceived;
            transport.Disconnected += OnDisconnected;
        }

        public void Send(int peer, RemoteDebugLane lane, RemoteDebugEnvelope envelope)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RemoteDebugEndpoint));
            int limit = lane == RemoteDebugLane.State ? _options.MaxStateBytes : _options.MaxMessageBytes;
            if (envelope.Payload.Length > limit) throw new RemoteDebugException("MessageTooLarge", "RemoteDebug payload exceeds limit.");
            var frames = RemoteDebugWire.Split(RemoteDebugWire.Encode(envelope), _options.ChunkBytes).GetEnumerator();
            frames.MoveNext();
            _outgoing[(int) lane].Enqueue(new Outbound {Peer = peer, Envelope = envelope, Frames = frames, Current = frames.Current});
        }

        public bool HasQueued(int peer, string service, string session, RemoteDebugLane lane) =>
            _outgoing[(int) lane].Any(value => Matches(value, peer, service, session));

        private static bool Matches(Outbound value, int peer, string service, string session) => value.Peer == peer &&
                                                                                                 (service == null || value.Envelope.Service == service) &&
                                                                                                 (session == null || value.Envelope.Session == session);

        public void Cancel(int peer, string service = null, string session = null, long? request = null)
        {
            foreach (var queue in _outgoing)
            {
                int count = queue.Count;
                while (count-- > 0)
                {
                    var item = queue.Dequeue();
                    if (Matches(item, peer, service, session) && (! request.HasValue || item.Envelope.Request == request)) item.Frames.Dispose();
                    else queue.Enqueue(item);
                }
            }
        }

        public void Tick()
        {
            if (_disposed) return;
            Transport.Tick();
            _assembler.Expire(_clock());
            for (int budget = 0; budget < _options.FramesPerTick; budget++)
            {
                bool sent = false;
                for (int lane = 0; lane < _outgoing.Length && ! sent; lane++)
                {
                    var queue = _outgoing[lane];
                    int attempts = queue.Count;
                    while (attempts-- > 0 && ! sent)
                    {
                        var item = queue.Dequeue();
                        if (! Transport.TrySend(item.Peer, (RemoteDebugLane) lane, item.Current))
                        {
                            queue.Enqueue(item);
                            continue;
                        }
                        sent = true;
                        if (item.Frames.MoveNext())
                        {
                            item.Current = item.Frames.Current;
                            queue.Enqueue(item);
                        }
                        else item.Frames.Dispose();
                    }
                }
                if (! sent) break;
            }
        }

        private void OnReceived(int peer, RemoteDebugLane lane, byte[] frame)
        {
            try
            {
                var bytes = _assembler.Accept(peer, lane, frame, _clock());
                if (bytes != null)
                    Received?.Invoke(peer, RemoteDebugWire.Decode(bytes,
                        lane == RemoteDebugLane.State ? _options.MaxStateBytes : _options.MaxMessageBytes));
            }
            catch (Exception error)
            {
                _log?.Invoke(error);
            }
        }

        private void OnDisconnected(int peer)
        {
            Cancel(peer);
            _assembler.Expire(_clock(), peer);
            Disconnected?.Invoke(peer);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Transport.Received -= OnReceived;
            Transport.Disconnected -= OnDisconnected;
            foreach (var queue in _outgoing)
            {
                foreach (var item in queue) item.Frames.Dispose();
                queue.Clear();
            }
            _assembler.Dispose();
            Transport.Dispose();
            Received = null;
            Disconnected = null;
        }
    }
}