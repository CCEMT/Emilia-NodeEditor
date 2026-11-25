using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using UnityEditor.Hardware;
using UnityEditor.Networking.PlayerConnection;
using UnityEditorInternal;

namespace Emilia.RemoteDebug.Editor
{
    public interface IRemoteDebugConnectionBackend
    {
        IReadOnlyList<RemoteDebugConnectionTarget> GetAvailableTargets();
        IReadOnlyList<RemoteDebugPeer> ConnectedPeers { get; }
        int ConnectPlayer(RemoteDebugConnectionTarget target);
        int ConnectAddress(RemoteDebugConnectionAddress address);
        RemoteDebugTarget GetServiceTarget(int peerId);
        void DiscoverServices();
    }

    public sealed class UnityRemoteDebugConnectionBackend : IRemoteDebugConnectionBackend
    {
        private const BindingFlags StaticFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        private static readonly MethodInfo ConnectionIP = typeof(ProfilerDriver).GetMethod("GetConnectionIP", StaticFlags);
        private static readonly MethodInfo ConnectionPort = typeof(ProfilerDriver).GetMethod("GetConnectionPort", StaticFlags);
        private static readonly PropertyInfo DirectURL = typeof(ProfilerDriver).GetProperty("directConnectionUrl", StaticFlags);
        private static readonly MethodInfo IsLocalhost = typeof(ProfilerDriver).GetMethod("IsIdentifierOnLocalhost", StaticFlags);
        private static readonly MethodInfo IsEditor = typeof(ProfilerDriver).GetMethod("IsConnectionEditor", StaticFlags);
        private readonly RemoteDebugClient _client;

        public UnityRemoteDebugConnectionBackend(RemoteDebugClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public IReadOnlyList<RemoteDebugPeer> ConnectedPeers => EditorConnection.instance.ConnectedPlayers
            .Select(player => new RemoteDebugPeer(player.playerId, player.name)).ToArray();

        public IReadOnlyList<RemoteDebugConnectionTarget> GetAvailableTargets()
        {
            var targets = new List<RemoteDebugConnectionTarget>();
            var peers = ConnectedPeers;
            foreach (int id in ProfilerDriver.GetAvailableProfilers())
            {
                string name = ProfilerDriver.GetConnectionIdentifier(id);
                // Editor 由业务菜单的“本机 Play Mode”入口处理。
                if (id < 0 || name.StartsWith("Editor", StringComparison.Ordinal) || name.StartsWith("Profiler-Editor", StringComparison.Ordinal)) continue;
                bool prohibited = ReadNativeBoolean(IsLocalhost, id) && (name.Contains("MetroPlayerX") || name.Contains("UWPPlayerX"));
                bool connectable = ! prohibited && ProfilerDriver.IsIdentifierConnectable(id);
                targets.Add(new RemoteDebugConnectionTarget("player:" + id, name, RemoteDebugConnectionKind.Player,
                    nativeId: id, peerId: peers.Any(peer => peer.Id == id) ? id : (int?) null,
                    canConnect: connectable, unavailableReason: prohibited ? "此目标禁止本机地址连接" : connectable ? null : "Unity 连接版本不匹配"));
            }
            string currentURL = ReadDirectURL();
            int currentPeer = ProfilerDriver.connectedProfiler;
            foreach (var device in DevDeviceList.GetDevices())
            {
                if (! device.isConnected || (device.features & DevDeviceFeatures.PlayerConnection) == 0) continue;
                string url = "device://" + device.id;
                int? peer = currentURL == url && peers.Any(value => value.Id == currentPeer) ? currentPeer : (int?) null;
                targets.Add(new RemoteDebugConnectionTarget(url, device.name, RemoteDebugConnectionKind.Device, peerId: peer, address: url));
            }
            return targets;
        }

        public int ConnectPlayer(RemoteDebugConnectionTarget target)
        {
            if (! target.CanConnect) throw new RemoteDebugException("TargetUnavailable", target.UnavailableReason);
            if (target.PeerId.HasValue && ConnectedPeers.Any(peer => peer.Id == target.PeerId.Value)) return target.PeerId.Value;
            if (target.Kind == RemoteDebugConnectionKind.Player)
            {
                ProfilerDriver.connectedProfiler = target.NativeId;
                if (ProfilerDriver.connectedProfiler != target.NativeId) throw new RemoteDebugException("ConnectionFailed", "未能连接所选设备，请确认客户端仍在运行。");
                return target.NativeId;
            }
            if (target.Kind != RemoteDebugConnectionKind.Device) throw new RemoteDebugException("ConnectionLost", "设备已断开，请刷新目标列表后重新选择。");
            ProfilerDriver.DirectURLConnect(target.Address);
            if (ReadDirectURL() != target.Address || ReadNativeBoolean(IsEditor)) throw new RemoteDebugException("ConnectionFailed", "USB 设备连接失败，请确认设备上的调试客户端正在运行。");
            return ProfilerDriver.connectedProfiler;
        }

        public int ConnectAddress(RemoteDebugConnectionAddress address)
        {
            int current = ProfilerDriver.connectedProfiler;
            if (ConnectedPeers.Any(peer => peer.Id == current) && MatchesAddress(current, address)) return current;
            ProfilerDriver.DirectIPConnect(address.Value);
            int peer = ProfilerDriver.connectedProfiler;
            if (ReadNativeBoolean(IsEditor) || ! MatchesAddress(peer, address)) throw new RemoteDebugException("ConnectionFailed", "IP 连接失败，请检查地址、端口和设备网络。");
            return peer;
        }

        private static bool MatchesAddress(int peer, RemoteDebugConnectionAddress address)
        {
            if (ConnectionIP == null || (address.Port.HasValue && ConnectionPort == null)) throw new NotSupportedException("当前 Unity 版本无法确认 IP 连接目标。");
            string ip = ConnectionIP.Invoke(null, new object[] {peer}) as string;
            if (! IPAddress.TryParse(ip, out var connectedIP) || ! connectedIP.Equals(address.IP)) return false;
            return ! address.Port.HasValue || Convert.ToInt32(ConnectionPort.Invoke(null, new object[] {peer})) == address.Port.Value;
        }

        private static string ReadDirectURL() => DirectURL?.GetValue(null) as string;

        private static bool ReadNativeBoolean(MethodInfo method, params object[] arguments)
        {
            if (method == null) throw new NotSupportedException("当前 Unity 版本缺少原生设备连接查询接口。");
            return (bool) method.Invoke(null, arguments);
        }

        public RemoteDebugTarget GetServiceTarget(int peerId) => _client.Targets.FirstOrDefault(target => target.PeerId == peerId);
        public void DiscoverServices() => _client.Discover();
    }
}