using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;

namespace Emilia.RemoteDebug.Editor
{
    public sealed class EditorConnectionRemoteDebugTransport : ScriptableObject, IRemoteDebugTransport
    {
        private bool _registered;
        public IReadOnlyList<RemoteDebugPeer> Peers => EditorConnection.instance.ConnectedPlayers.Select(player => new RemoteDebugPeer(player.playerId, player.name)).ToArray();
        public event Action<int, RemoteDebugLane, byte[]> Received;
        public event Action<int> Disconnected;

        public static EditorConnectionRemoteDebugTransport Create()
        {
            foreach (var stale in Resources.FindObjectsOfTypeAll<EditorConnectionRemoteDebugTransport>())
                if (stale != null && ! stale._registered && stale.hideFlags == HideFlags.HideAndDontSave)
                    DestroyImmediate(stale);
            var transport = CreateInstance<EditorConnectionRemoteDebugTransport>();
            transport.hideFlags = HideFlags.HideAndDontSave;
            transport.Register();
            return transport;
        }

        private void Register()
        {
            if (_registered) return;
            _registered = true;
            EditorConnection.instance.Initialize();
            EditorConnection.instance.Register(RemoteDebugWire.Channels[0], OnControl);
            EditorConnection.instance.Register(RemoteDebugWire.Channels[1], OnCommand);
            EditorConnection.instance.Register(RemoteDebugWire.Channels[2], OnState);
            EditorConnection.instance.RegisterDisconnection(OnDisconnected);
        }

        private void OnControl(MessageEventArgs args) => Received?.Invoke(args.playerId, RemoteDebugLane.Control, args.data);
        private void OnCommand(MessageEventArgs args) => Received?.Invoke(args.playerId, RemoteDebugLane.Command, args.data);
        private void OnState(MessageEventArgs args) => Received?.Invoke(args.playerId, RemoteDebugLane.State, args.data);
        private void OnDisconnected(int peer) => Disconnected?.Invoke(peer);
        public bool TrySend(int peer, RemoteDebugLane lane, byte[] frame) => _registered && EditorConnection.instance.TrySend(RemoteDebugWire.Channels[(int) lane], frame, peer);
        private void OnDisable() => Unregister();
        public void Tick() { }

        private void Unregister()
        {
            if (! _registered) return;
            _registered = false;
            EditorConnection.instance.Unregister(RemoteDebugWire.Channels[0], OnControl);
            EditorConnection.instance.Unregister(RemoteDebugWire.Channels[1], OnCommand);
            EditorConnection.instance.Unregister(RemoteDebugWire.Channels[2], OnState);
            EditorConnection.instance.UnregisterDisconnection(OnDisconnected);
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