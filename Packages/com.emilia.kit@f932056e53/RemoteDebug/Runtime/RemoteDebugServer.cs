using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Emilia.RemoteDebug
{
    public sealed class RemoteDebugServer : IDisposable
    {
        private sealed class Cached
        {
            public byte[] Hash;
            public RemoteDebugEnvelope Reply;
            public int Size;
        }

        private sealed class Registration : IDisposable
        {
            public RemoteDebugServer Server;
            public string Id;
            public int Version;
            public IRemoteDebugService Service;
            public IRemoteDebugCodec Codec;
            public Session Session;

            public void Dispose()
            {
                Server?.RemoveService(this);
                Server = null;
            }
        }

        private sealed class Session
        {
            public Registration Registration;
            public RemoteDebugEndpoint Endpoint;
            public RemoteDebugContext Context;
            public double LastSeen;
            public long HighWater;
            public int CachedBytes;
            public readonly Queue<RemoteDebugEnvelope> Requests = new();
            public readonly Dictionary<long, Cached> Cache = new();
            public readonly Queue<long> CacheOrder = new();
        }

        private readonly Dictionary<string, Registration> _services = new(StringComparer.Ordinal);
        private readonly List<RemoteDebugEndpoint> _endpoints = new();
        private readonly RemoteDebugOptions _options;
        private readonly Func<double> _clock;
        private readonly Action<Exception> _log;
        private readonly JsonRemoteDebugCodec _discoveryCodec = new();
        private bool _disposed;

        public RemoteDebugServer(Func<double> clock, RemoteDebugOptions options = null, Action<Exception> log = null)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _options = options ?? new RemoteDebugOptions();
            _options.Validate();
            _log = log;
        }

        public IDisposable AddTransport(IRemoteDebugTransport transport)
        {
            ThrowIfDisposed();
            var endpoint = new RemoteDebugEndpoint(transport, _options, _clock, _log);
            endpoint.Received += (peer, message) => Receive(endpoint, peer, message);
            endpoint.Disconnected += peer => Disconnect(endpoint, peer);
            _endpoints.Add(endpoint);
            return new TransportLease(this, endpoint);
        }

        private sealed class TransportLease : IDisposable
        {
            private RemoteDebugServer _server;
            private readonly RemoteDebugEndpoint _endpoint;

            public TransportLease(RemoteDebugServer server, RemoteDebugEndpoint endpoint)
            {
                _server = server;
                _endpoint = endpoint;
            }

            public void Dispose()
            {
                if (_server == null) return;
                _server.Disconnect(_endpoint, null);
                _server._endpoints.Remove(_endpoint);
                _endpoint.Dispose();
                _server = null;
            }
        }

        public IDisposable RegisterService(string serviceId, int serviceVersion, IRemoteDebugService service, IRemoteDebugCodec codec)
        {
            ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(serviceId) || serviceId.Length > 128 || serviceVersion <= 0 || service == null || codec == null) throw new ArgumentException("Invalid service registration.");
            if (_services.ContainsKey(serviceId)) throw new InvalidOperationException("RemoteDebug service already registered: " + serviceId);
            var registration = new Registration {Server = this, Id = serviceId, Version = serviceVersion, Service = service, Codec = codec};
            _services.Add(serviceId, registration);
            return registration;
        }

        private RemoteDebugServiceDescription Describe(Registration registration)
        {
            var description = registration.Service.Describe() ?? new RemoteDebugServiceDescription();
            description.Id = registration.Id;
            description.Version = registration.Version;
            return description;
        }

        private void Receive(RemoteDebugEndpoint endpoint, int peer, RemoteDebugEnvelope message)
        {
            if (_disposed) return;
            try
            {
                if (message.Kind == RemoteDebugMessageKind.Discover)
                {
                    var discovery = new RemoteDebugDiscovery();
                    foreach (var registration in _services.Values) discovery.Services.Add(Describe(registration));
                    endpoint.Send(peer, RemoteDebugLane.Control, new RemoteDebugEnvelope {Kind = RemoteDebugMessageKind.Discovered, Payload = _discoveryCodec.Encode(discovery), Control = true});
                    return;
                }
                if (! _services.TryGetValue(message.Service, out var service)) throw new RemoteDebugException("ServiceMissing", "RemoteDebug service is unavailable.");
                if (message.ServiceVersion != service.Version) throw new RemoteDebugException("ServiceVersionMismatch", "RemoteDebug service version mismatch.");
                if (message.Kind == RemoteDebugMessageKind.Attach)
                {
                    if (string.IsNullOrEmpty(message.Session)) throw new RemoteDebugException("SessionMissing", "Session ID is required.");
                    if (service.Session != null && _clock() - service.Session.LastSeen > _options.SessionTimeoutSeconds) End(service.Session, RemoteDebugEndReason.TimedOut);
                    if (service.Session != null && ! Owns(service.Session, endpoint, peer, message.Session))
                        throw new RemoteDebugException("ServiceBusy", "RemoteDebug service is occupied by another session.");
                    if (service.Session == null)
                    {
                        var session = new Session {Registration = service, Endpoint = endpoint, LastSeen = _clock()};
                        session.Context = new RemoteDebugContext(peer, message.Session, service.Codec,
                            (topic, value) => Publish(session, topic, value), topic => ! endpoint.HasQueued(peer, service.Id, message.Session, RemoteDebugLane.State));
                        session.Context.IsInProcess = endpoint.Transport is MemoryRemoteDebugTransport;
                        service.Session = session;
                        try
                        {
                            service.Service.OnSessionStarted(session.Context);
                        }
                        catch
                        {
                            End(session, RemoteDebugEndReason.AttachFailed);
                            throw;
                        }
                    }
                    service.Session.LastSeen = _clock();
                    Reply(endpoint, peer, message, _discoveryCodec.Encode(Describe(service)));
                    return;
                }
                var active = service.Session;
                if (active != null && _clock() - active.LastSeen > _options.SessionTimeoutSeconds)
                {
                    End(active, RemoteDebugEndReason.TimedOut);
                    active = null;
                }
                if (active == null || ! Owns(active, endpoint, peer, message.Session)) throw new RemoteDebugException("SessionExpired", "RemoteDebug session expired.");
                active.LastSeen = _clock();
                switch (message.Kind)
                {
                    case RemoteDebugMessageKind.Heartbeat:
                        Reply(endpoint, peer, message);
                        break;
                    case RemoteDebugMessageKind.Subscribe:
                        if (string.IsNullOrEmpty(message.Command)) throw new ArgumentException("Topic is required.");
                        if (message.Payload.Length == 1 && message.Payload[0] != 0) active.Context.Topics.Add(message.Command);
                        else active.Context.Topics.Remove(message.Command);
                        Reply(endpoint, peer, message);
                        break;
                    case RemoteDebugMessageKind.Detach:
                        End(active, RemoteDebugEndReason.Detached);
                        Reply(endpoint, peer, message);
                        break;
                    case RemoteDebugMessageKind.Request:
                        if (message.Request <= 0) throw new RemoteDebugException("InvalidRequest", "Request ID must be positive.");
                        var queued = active.Requests.FirstOrDefault(value => value.Request == message.Request);
                        if (queued != null)
                        {
                            if (! Hash(queued).SequenceEqual(Hash(message))) throw new RemoteDebugException("RequestIdReused", "Request ID was reused with different content.");
                            break;
                        }
                        if (active.Requests.Count >= _options.MaxPendingRequests) throw new RemoteDebugException("QueueFull", "RemoteDebug request queue is full.");
                        active.Requests.Enqueue(message);
                        break;
                    default:
                        throw new RemoteDebugException("InvalidMessage", "Unsupported client message.");
                }
            }
            catch (Exception error)
            {
                Reply(endpoint, peer, message, error: error);
            }
        }

        private static bool Owns(Session session, RemoteDebugEndpoint endpoint, int peer, string id) =>
            ReferenceEquals(session.Endpoint, endpoint) && session.Context.PeerId == peer && session.Context.SessionId == id;

        private static byte[] Hash(RemoteDebugEnvelope message)
        {
            using var sha = SHA256.Create();
            byte[] command = Encoding.UTF8.GetBytes(message.Command);
            byte[] input = new byte[command.Length + 1 + message.Payload.Length];
            Buffer.BlockCopy(command, 0, input, 0, command.Length);
            Buffer.BlockCopy(message.Payload, 0, input, command.Length + 1, message.Payload.Length);
            return sha.ComputeHash(input);
        }

        private static RemoteDebugEnvelope Response(RemoteDebugEnvelope request, byte[] payload, Exception error) =>
            new() {
                Kind = RemoteDebugMessageKind.Reply, Service = request.Service, ServiceVersion = request.ServiceVersion,
                Session = request.Session, Request = request.Request, Control = request.Control, Payload = payload ?? Array.Empty<byte>(),
                Error = error == null ? string.Empty : error is RemoteDebugException remote ? remote.Error : error.GetType().Name,
                Message = RemoteDebugWire.LimitError(error?.Message)
            };

        private void Reply(RemoteDebugEndpoint endpoint, int peer, RemoteDebugEnvelope request, byte[] payload = null, Exception error = null)
        {
            endpoint.Send(peer, request.Control ? RemoteDebugLane.Control : RemoteDebugLane.Command, Response(request, payload, error));
        }

        private void Execute(Session session, RemoteDebugEnvelope request)
        {
            byte[] hash = Hash(request);
            if (session.Cache.TryGetValue(request.Request, out var cached))
            {
                if (hash.SequenceEqual(cached.Hash)) session.Endpoint.Send(session.Context.PeerId, RemoteDebugLane.Command, cached.Reply);
                else Reply(session.Endpoint, session.Context.PeerId, request, error: new RemoteDebugException("RequestIdReused", "Request ID was reused with different content."));
                return;
            }
            if (request.Request <= session.HighWater)
            {
                Reply(session.Endpoint, session.Context.PeerId, request, error: new RemoteDebugException("ResultExpired", "Result expired; refresh state before issuing a new operation."));
                return;
            }
            session.HighWater = request.Request;
            RemoteDebugEnvelope response;
            try
            {
                object result = session.Registration.Service.HandleRequest(session.Context, request.Command, request.Payload);
                byte[] bytes = result == null ? Array.Empty<byte>() : session.Registration.Codec.Encode(result);
                if (bytes.Length > _options.MaxMessageBytes) throw new RemoteDebugException("ResponseTooLarge", "RemoteDebug response exceeds limit; publish large state separately.");
                response = Response(request, bytes, null);
            }
            catch (Exception error)
            {
                response = Response(request, null, error);
            }
            int size = RemoteDebugWire.Encode(response).Length + hash.Length;
            if (size <= _options.MaxCachedResponseBytes)
            {
                session.Cache.Add(request.Request, new Cached {Hash = hash, Reply = response, Size = size});
                session.CacheOrder.Enqueue(request.Request);
                session.CachedBytes += size;
                while (session.Cache.Count > _options.MaxCachedResponses || session.CachedBytes > _options.MaxCachedResponseBytes)
                {
                    long id = session.CacheOrder.Dequeue();
                    session.CachedBytes -= session.Cache[id].Size;
                    session.Cache.Remove(id);
                }
            }
            session.Endpoint.Send(session.Context.PeerId, RemoteDebugLane.Command, response);
        }

        private bool Publish(Session session, string topic, object value)
        {
            if (! session.Context.CanPublish(topic)) return false;
            session.Endpoint.Send(session.Context.PeerId, RemoteDebugLane.State, new RemoteDebugEnvelope {
                Kind = RemoteDebugMessageKind.Event, Service = session.Registration.Id, ServiceVersion = session.Registration.Version,
                Session = session.Context.SessionId, Command = topic, Payload = session.Registration.Codec.Encode(value)
            });
            return true;
        }

        public void Tick()
        {
            if (_disposed) return;
            foreach (var endpoint in _endpoints.ToArray()) endpoint.Tick();
            double now = _clock();
            foreach (var registration in _services.Values.ToArray())
            {
                var session = registration.Session;
                if (session == null) continue;
                if (now - session.LastSeen > _options.SessionTimeoutSeconds)
                {
                    End(session, RemoteDebugEndReason.TimedOut);
                    continue;
                }
                try
                {
                    if (session.Requests.Count > 0 && ! session.Endpoint.HasQueued(session.Context.PeerId, registration.Id, session.Context.SessionId, RemoteDebugLane.Command))
                        Execute(session, session.Requests.Dequeue());
                    if (session.Context.IsActive) registration.Service.Tick(session.Context, now);
                }
                catch (Exception error)
                {
                    _log?.Invoke(error);
                    End(session, RemoteDebugEndReason.Disposed);
                }
            }
        }

        private void End(Session session, RemoteDebugEndReason reason)
        {
            if (! session.Context.IsActive) return;
            session.Context.IsActive = false;
            session.Registration.Session = null;
            session.Endpoint.Cancel(session.Context.PeerId, session.Registration.Id, session.Context.SessionId);
            session.Requests.Clear();
            session.Cache.Clear();
            session.CacheOrder.Clear();
            session.Context.Topics.Clear();
            try
            {
                session.Registration.Service.OnSessionEnded(session.Context, reason);
            }
            catch (Exception error)
            {
                _log?.Invoke(error);
            }
        }

        private void Disconnect(RemoteDebugEndpoint endpoint, int? peer)
        {
            foreach (var registration in _services.Values.ToArray())
                if (registration.Session != null && ReferenceEquals(registration.Session.Endpoint, endpoint) && (! peer.HasValue || registration.Session.Context.PeerId == peer))
                    End(registration.Session, RemoteDebugEndReason.ConnectionLost);
        }

        private void RemoveService(Registration registration)
        {
            if (registration.Session != null) End(registration.Session, RemoteDebugEndReason.ServiceRemoved);
            if (_services.TryGetValue(registration.Id, out var current) && ReferenceEquals(current, registration)) _services.Remove(registration.Id);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RemoteDebugServer));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            foreach (var registration in _services.Values.ToArray())
            {
                if (registration.Session != null) End(registration.Session, RemoteDebugEndReason.Disposed);
                registration.Server = null;
            }
            _services.Clear();
            foreach (var endpoint in _endpoints) endpoint.Dispose();
            _endpoints.Clear();
        }
    }
}