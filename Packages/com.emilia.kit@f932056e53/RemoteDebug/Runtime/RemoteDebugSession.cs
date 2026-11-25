using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Emilia.RemoteDebug
{
    public sealed class RemoteDebugSession : IDisposable
    {
        private sealed class Pending
        {
            public RemoteDebugEnvelope Message;
            public double Started;
            public bool Retried;
            public readonly TaskCompletionSource<byte[]> Completion = new();
        }

        private readonly RemoteDebugEndpoint _endpoint;
        private readonly Func<double> _clock;
        private readonly RemoteDebugOptions _options;
        private readonly Queue<Pending> _requests = new();
        private readonly Dictionary<long, Pending> _controls = new();
        private readonly IRemoteDebugCodec _codec;
        private Pending _active;
        private long _sequence;
        private double _lastReply, _nextHeartbeat;
        private bool _heartbeatPending, _disposed, _disconnecting;
        public string Id { get; } = Guid.NewGuid().ToString("N");
        public string ServiceId { get; }
        public int ServiceVersion { get; }
        public RemoteDebugTarget Target { get; }
        public RemoteDebugServiceDescription Description { get; private set; }
        public bool IsConnected { get; private set; }
        public event Action<string, byte[]> EventReceived;
        public event Action<Exception> RequestFailed;
        public event Action Closed;

        public RemoteDebugSession(RemoteDebugEndpoint endpoint, RemoteDebugTarget target, string service, int version,
            IRemoteDebugCodec codec, Func<double> clock, RemoteDebugOptions options)
        {
            _endpoint = endpoint;
            Target = target;
            ServiceId = service;
            ServiceVersion = version;
            _codec = codec;
            _clock = clock;
            _options = options;
            _lastReply = clock();
        }

        private RemoteDebugEnvelope Message(RemoteDebugMessageKind kind, bool control) => new() {
            Kind = kind, Service = ServiceId, ServiceVersion = ServiceVersion, Session = Id, Request = ++_sequence, Control = control
        };

        public async Task AttachAsync()
        {
            byte[] bytes = await Control(Message(RemoteDebugMessageKind.Attach, true));
            Description = new JsonRemoteDebugCodec().Decode<RemoteDebugServiceDescription>(bytes);
            if (_disposed) throw new ObjectDisposedException(nameof(RemoteDebugSession));
            IsConnected = true;
            _lastReply = _clock();
        }

        public async Task<TResponse> RequestAsync<TRequest, TResponse>(string command, TRequest request)
        {
            RequireConnected();
            RemoteDebugWire.ValidateName(command, nameof(command));
            if (_requests.Count + (_active == null ? 0 : 1) >= _options.MaxPendingRequests) throw new RemoteDebugException("QueueFull", "RemoteDebug request queue is full.");
            var message = Message(RemoteDebugMessageKind.Request, false);
            message.Command = command;
            message.Payload = _codec.Encode(request);
            if (message.Payload.Length > _options.MaxMessageBytes) throw new RemoteDebugException("MessageTooLarge", "RemoteDebug request exceeds limit.");
            var pending = new Pending {Message = message};
            _requests.Enqueue(pending);
            byte[] bytes = await pending.Completion.Task;
            return bytes.Length == 0 ? default : _codec.Decode<TResponse>(bytes);
        }

        public Task SubscribeAsync(string topic, bool enabled = true)
        {
            RequireConnected();
            RemoteDebugWire.ValidateName(topic, nameof(topic));
            var message = Message(RemoteDebugMessageKind.Subscribe, true);
            message.Command = topic;
            message.Payload = new[] {enabled ? (byte) 1 : (byte) 0};
            return Control(message);
        }

        private Task<byte[]> Control(RemoteDebugEnvelope message)
        {
            if (_disposed) return Task.FromException<byte[]>(new ObjectDisposedException(nameof(RemoteDebugSession)));
            var pending = new Pending {Message = message, Started = _clock()};
            _controls.Add(message.Request, pending);
            _endpoint.Send(Target.PeerId, RemoteDebugLane.Control, message);
            return pending.Completion.Task;
        }

        public async Task DisconnectAsync()
        {
            if (_disposed || _disconnecting) return;
            _disconnecting = true;
            try
            {
                if (IsConnected) await Control(Message(RemoteDebugMessageKind.Detach, true));
            }
            finally
            {
                Finish(null);
            }
        }

        public void Tick(double now)
        {
            if (_disposed) return;
            foreach (var pending in new List<Pending>(_controls.Values))
            {
                if (now - pending.Started >= _options.RequestTimeoutSeconds)
                {
                    Finish(new TimeoutException("RemoteDebug control request timed out."));
                    return;
                }
                Retry(pending, now, RemoteDebugLane.Control);
            }
            if (! IsConnected || _disconnecting) return;
            if (now - _lastReply > _options.SessionTimeoutSeconds)
            {
                Finish(new TimeoutException("RemoteDebug session timed out."));
                return;
            }
            if (_active == null && _requests.Count > 0)
            {
                _active = _requests.Dequeue();
                _active.Started = now;
                _endpoint.Send(Target.PeerId, RemoteDebugLane.Command, _active.Message);
            }
            if (_active != null)
            {
                if (now - _active.Started >= _options.RequestTimeoutSeconds)
                {
                    var expired = _active;
                    _active = null;
                    _endpoint.Cancel(Target.PeerId, ServiceId, Id, expired.Message.Request);
                    while (_requests.Count > 0) _requests.Dequeue().Completion.TrySetCanceled();
                    var error = new TimeoutException("RemoteDebug result is unknown. Refresh state before issuing another command.");
                    expired.Completion.TrySetException(error);
                    RequestFailed?.Invoke(error);
                }
                else Retry(_active, now, RemoteDebugLane.Command);
            }
            if (! _heartbeatPending && now >= _nextHeartbeat)
            {
                _nextHeartbeat = now + _options.HeartbeatSeconds;
                Heartbeat();
            }
        }

        private void Retry(Pending pending, double now, RemoteDebugLane lane)
        {
            if (pending.Retried || now - pending.Started < _options.RetrySeconds) return;
            pending.Retried = true;
            _endpoint.Cancel(Target.PeerId, ServiceId, Id, pending.Message.Request);
            _endpoint.Send(Target.PeerId, lane, pending.Message);
        }

        private async void Heartbeat()
        {
            _heartbeatPending = true;
            try
            {
                await Control(Message(RemoteDebugMessageKind.Heartbeat, true));
            }
            catch (Exception error)
            {
                if (! _disposed) Finish(error);
            }
            finally
            {
                _heartbeatPending = false;
            }
        }

        public void Receive(RemoteDebugEnvelope message)
        {
            if (_disposed) return;
            _lastReply = _clock();
            if (message.Kind == RemoteDebugMessageKind.Event)
            {
                EventReceived?.Invoke(message.Command, message.Payload);
                return;
            }
            if (message.Kind != RemoteDebugMessageKind.Reply) return;
            Pending pending;
            if (message.Control)
            {
                if (! _controls.TryGetValue(message.Request, out pending)) return;
                _controls.Remove(message.Request);
            }
            else
            {
                if (_active == null || _active.Message.Request != message.Request) return;
                pending = _active;
                _active = null;
            }
            _endpoint.Cancel(Target.PeerId, ServiceId, Id, message.Request);
            if (string.IsNullOrEmpty(message.Error)) pending.Completion.TrySetResult(message.Payload);
            else pending.Completion.TrySetException(new RemoteDebugException(message.Error, message.Message));
        }

        private void RequireConnected()
        {
            if (_disposed || ! IsConnected || _disconnecting) throw new InvalidOperationException("RemoteDebug session is not connected.");
        }

        public void LoseConnection() => Finish(new RemoteDebugException("ConnectionLost", "RemoteDebug connection lost."));

        private void Finish(Exception error)
        {
            if (_disposed) return;
            _disposed = true;
            IsConnected = false;
            _endpoint.Cancel(Target.PeerId, ServiceId, Id);
            var all = new List<Pending>(_controls.Values);
            _controls.Clear();
            if (_active != null)
            {
                all.Add(_active);
                _active = null;
            }
            while (_requests.Count > 0) all.Add(_requests.Dequeue());
            foreach (var pending in all)
            {
                if (error == null) pending.Completion.TrySetCanceled();
                else pending.Completion.TrySetException(error);
            }
            if (error != null) RequestFailed?.Invoke(error);
            EventReceived = null;
            RequestFailed = null;
            Closed?.Invoke();
            Closed = null;
        }

        public void Dispose()
        {
            if (_disposed) return;
            bool detach = IsConnected || _controls.Count > 0;
            var message = Message(RemoteDebugMessageKind.Detach, true);
            Finish(null);
            // 不等待业务队列；物理连接由传输宿主管理，不能 DisconnectAll。
            if (detach) _endpoint.Send(Target.PeerId, RemoteDebugLane.Control, message);
        }
    }
}