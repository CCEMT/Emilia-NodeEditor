using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Emilia.RemoteDebug
{
    public enum RemoteDebugLane
    {
        Control,
        Command,
        State
    }

    public enum RemoteDebugEndReason
    {
        Detached,
        ConnectionLost,
        TimedOut,
        ServiceRemoved,
        Disposed,
        AttachFailed
    }

    public sealed class RemoteDebugOptions
    {
        public int ChunkBytes = 64 * 1024;
        public int MaxMessageBytes = 1024 * 1024;
        public int MaxStateBytes = 64 * 1024 * 1024;
        public int MaxPendingRequests = 32;
        public int MaxCachedResponses = 256;
        public int MaxCachedResponseBytes = 8 * 1024 * 1024;
        public int MaxAssemblies = 8;
        public int MaxAssemblyBytes = 128 * 1024 * 1024;
        public int FramesPerTick = 4;
        public double HeartbeatSeconds = 1;
        public double SessionTimeoutSeconds = 10;
        public double AssemblyTimeoutSeconds = 30;
        public double RetrySeconds = 3;
        public double RequestTimeoutSeconds = 6;
        public double DiscoverySeconds = 3;

        public void Validate()
        {
            if (ChunkBytes <= 0 || MaxMessageBytes < ChunkBytes || MaxStateBytes < MaxMessageBytes ||
                MaxPendingRequests <= 0 || MaxCachedResponses <= 0 || MaxCachedResponseBytes <= 0 ||
                MaxAssemblies <= 0 || MaxAssemblyBytes < MaxStateBytes || FramesPerTick <= 0 ||
                HeartbeatSeconds <= 0 || SessionTimeoutSeconds <= HeartbeatSeconds || AssemblyTimeoutSeconds <= 0 || RetrySeconds <= 0 ||
                RequestTimeoutSeconds <= RetrySeconds || DiscoverySeconds <= 0) throw new ArgumentOutOfRangeException(nameof(RemoteDebugOptions));
        }
    }

    public readonly struct RemoteDebugPeer
    {
        public int Id { get; }
        public string Name { get; }

        public RemoteDebugPeer(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    public interface IRemoteDebugTransport : IDisposable
    {
        IReadOnlyList<RemoteDebugPeer> Peers { get; }
        event Action<int, RemoteDebugLane, byte[]> Received;
        event Action<int> Disconnected;
        bool TrySend(int peer, RemoteDebugLane lane, byte[] frame);
        void Tick();
    }

    public interface IRemoteDebugCodec
    {
        byte[] Encode(object value);
        T Decode<T>(byte[] bytes);
    }

    public sealed class JsonRemoteDebugCodec : IRemoteDebugCodec
    {
        public byte[] Encode(object value) => Encoding.UTF8.GetBytes(JsonUtility.ToJson(value));
        public T Decode<T>(byte[] bytes) => JsonUtility.FromJson<T>(Encoding.UTF8.GetString(bytes));
    }

    [Serializable]
    public sealed class RemoteDebugServiceDescription
    {
        public string Id;
        public string Name;
        public int Version;
        public bool Ready;
        public bool CanControl;
        public string Reason;
        public byte[] Metadata;
    }

    [Serializable]
    public sealed class RemoteDebugDiscovery
    {
        public List<RemoteDebugServiceDescription> Services = new();
    }

    public sealed class RemoteDebugTarget
    {
        public int PeerId { get; }
        public string Name { get; }
        public IReadOnlyList<RemoteDebugServiceDescription> Services { get; set; }

        public RemoteDebugTarget(int peerId, string name)
        {
            PeerId = peerId;
            Name = name;
            Services = Array.Empty<RemoteDebugServiceDescription>();
        }

        public RemoteDebugServiceDescription FindService(string id)
        {
            foreach (var service in Services)
                if (service.Id == id)
                    return service;
            return null;
        }
    }

    public interface IRemoteDebugService
    {
        RemoteDebugServiceDescription Describe();
        void OnSessionStarted(RemoteDebugContext context);
        object HandleRequest(RemoteDebugContext context, string command, byte[] payload);
        void Tick(RemoteDebugContext context, double now);
        void OnSessionEnded(RemoteDebugContext context, RemoteDebugEndReason reason);
    }

    public sealed class RemoteDebugException : Exception
    {
        public string Error { get; }

        public RemoteDebugException(string error, string message) : base(message)
        {
            Error = error;
        }
    }

    public sealed class RemoteDebugContext
    {
        private readonly Func<string, object, bool> _publish;
        private readonly Func<string, bool> _canPublish;
        public readonly HashSet<string> Topics = new(StringComparer.Ordinal);
        public int PeerId { get; }
        public string SessionId { get; }
        public IRemoteDebugCodec Codec { get; }
        public bool IsInProcess { get; set; }
        public bool IsActive { get; set; } = true;

        public RemoteDebugContext(int peer, string session, IRemoteDebugCodec codec,
            Func<string, object, bool> publish, Func<string, bool> canPublish)
        {
            PeerId = peer;
            SessionId = session;
            Codec = codec;
            _publish = publish;
            _canPublish = canPublish;
        }

        public bool IsSubscribed(string topic) => IsActive && Topics.Contains(topic);
        public bool CanPublish(string topic) => IsSubscribed(topic) && _canPublish(topic);
        public bool TryPublish<T>(string topic, T value) => CanPublish(topic) && _publish(topic, value);
    }
}