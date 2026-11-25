using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace Emilia.RemoteDebug.Editor
{
    public enum RemoteDebugConnectionKind
    {
        Player,
        Device,
        ConnectedPlayer,
        Address
    }

    public sealed class RemoteDebugConnectionTarget
    {
        public string Key { get; }
        public string Name { get; }
        public RemoteDebugConnectionKind Kind { get; }
        public int NativeId { get; }
        public int? PeerId { get; }
        public string Address { get; }
        public bool CanConnect { get; }
        public string UnavailableReason { get; }

        public RemoteDebugConnectionTarget(string key, string name, RemoteDebugConnectionKind kind,
            int nativeId = 0, int? peerId = null, string address = null, bool canConnect = true, string unavailableReason = null)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Name = name ?? key;
            Kind = kind;
            NativeId = nativeId;
            PeerId = peerId;
            Address = address;
            CanConnect = canConnect;
            UnavailableReason = unavailableReason;
        }
    }

    public readonly struct RemoteDebugConnectionAddress
    {
        public IPAddress IP { get; }
        public int? Port { get; }
        public string Value { get; }

        private RemoteDebugConnectionAddress(IPAddress ip, int? port)
        {
            IP = ip;
            Port = port;
            string host = ip.ToString();
            Value = port.HasValue
                ? (ip.AddressFamily == AddressFamily.InterNetworkV6 ? "[" + host + "]" : host) + ":" + port.Value.ToString(CultureInfo.InvariantCulture)
                : host;
        }

        public static bool TryParse(string input, out RemoteDebugConnectionAddress address, out string error)
        {
            address = default;
            error = "请输入有效的 IP 地址，可在地址后填写端口。";
            string text = input?.Trim();
            if (string.IsNullOrEmpty(text)) return false;
            string host = text;
            string portText = null;
            if (text.StartsWith("[", StringComparison.Ordinal))
            {
                int end = text.IndexOf(']');
                if (end <= 1) return false;
                host = text.Substring(1, end - 1);
                if (end + 1 < text.Length)
                {
                    if (text[end + 1] != ':') return false;
                    portText = text.Substring(end + 2);
                }
            }
            else if (text.IndexOf(':') == text.LastIndexOf(':') && text.Contains(":"))
            {
                int separator = text.LastIndexOf(':');
                host = text.Substring(0, separator);
                portText = text.Substring(separator + 1);
            }
            if (! IPAddress.TryParse(host, out IPAddress ip)) return false;
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                string[] parts = host.Split('.');
                if (parts.Length != 4) return false;
                var bytes = new byte[4];
                for (int index = 0; index < parts.Length; index++)
                    if (! byte.TryParse(parts[index], NumberStyles.None, CultureInfo.InvariantCulture, out bytes[index]))
                        return false;
                ip = new IPAddress(bytes);
            }
            int? port = null;
            if (portText != null)
            {
                if (! int.TryParse(portText, NumberStyles.None, CultureInfo.InvariantCulture, out int value) || value < 1 || value > ushort.MaxValue)
                {
                    error = "端口必须是 1 到 65535 之间的整数。";
                    return false;
                }
                port = value;
            }
            address = new RemoteDebugConnectionAddress(ip, port);
            error = null;
            return true;
        }
    }
}