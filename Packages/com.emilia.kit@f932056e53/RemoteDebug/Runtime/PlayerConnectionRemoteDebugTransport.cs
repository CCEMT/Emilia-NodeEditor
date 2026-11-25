using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;

namespace Emilia.RemoteDebug
{
    public sealed class PlayerConnectionRemoteDebugTransport : ScriptableObject, IRemoteDebugTransport
    {
        private readonly HashSet<int> _peers = new();
        private bool _registered;
        public IReadOnlyList<RemoteDebugPeer> Peers => _peers.Select(peer => new RemoteDebugPeer(peer, "Editor")).ToArray();
        public event Action<int, RemoteDebugLane, byte[]> Received;
        public event Action<int> Disconnected;

        public static PlayerConnectionRemoteDebugTransport Create()
        {
            foreach (var stale in Resources.FindObjectsOfTypeAll<PlayerConnectionRemoteDebugTransport>())
                if (stale != null && ! stale._registered && stale.hideFlags == HideFlags.HideAndDontSave)
                    DestroyImmediate(stale);
            var transport = CreateInstance<PlayerConnectionRemoteDebugTransport>();
            transport.hideFlags = HideFlags.HideAndDontSave;
            transport.Register();
            return transport;
        }

        private void Register()
        {
            if (_registered) return;
            _registered = true;
            PlayerConnection.instance.Register(RemoteDebugWire.Channels[0], OnControl);
            PlayerConnection.instance.Register(RemoteDebugWire.Channels[1], OnCommand);
            PlayerConnection.instance.Register(RemoteDebugWire.Channels[2], OnState);
            PlayerConnection.instance.RegisterConnection(OnConnected);
            PlayerConnection.instance.RegisterDisconnection(OnDisconnected);
        }

        private void OnConnected(int peer) => _peers.Add(peer);

        private void OnDisconnected(int peer)
        {
            _peers.Remove(peer);
            Disconnected?.Invoke(peer);
        }

        private void OnControl(MessageEventArgs args) => Received?.Invoke(args.playerId, RemoteDebugLane.Control, args.data);
        private void OnCommand(MessageEventArgs args) => Received?.Invoke(args.playerId, RemoteDebugLane.Command, args.data);

        private void OnState(MessageEventArgs args) => Received?.Invoke(args.playerId, RemoteDebugLane.State, args.data);

        // Unity 2021 的 PlayerConnection 只提供广播；收件方按信封中的会话 ID 丢弃其它会话。
        public bool TrySend(int peer, RemoteDebugLane lane, byte[] frame) => _registered && PlayerConnection.instance.TrySend(RemoteDebugWire.Channels[(int) lane], frame);
        private void OnDisable() => Unregister();
        public void Tick() { }

        private void Unregister()
        {
            if (! _registered) return;
            _registered = false;
            PlayerConnection.instance.Unregister(RemoteDebugWire.Channels[0], OnControl);
            PlayerConnection.instance.Unregister(RemoteDebugWire.Channels[1], OnCommand);
            PlayerConnection.instance.Unregister(RemoteDebugWire.Channels[2], OnState);
            PlayerConnection.instance.UnregisterConnection(OnConnected);
            PlayerConnection.instance.UnregisterDisconnection(OnDisconnected);
            _peers.Clear();
            Received = null;
            Disconnected = null;
        }

        public void Dispose()
        {
            Unregister();
            DestroyImmediate(this);
        }
    }
}