using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Emilia.RemoteDebug
{
    public enum RemoteDebugMessageKind
    {
        Discover,
        Discovered,
        Attach,
        Heartbeat,
        Subscribe,
        Detach,
        Request,
        Reply,
        Event
    }

    public sealed class RemoteDebugEnvelope
    {
        public RemoteDebugMessageKind Kind;
        public string Service = string.Empty, Session = string.Empty, Command = string.Empty, Error = string.Empty, Message = string.Empty;
        public int ServiceVersion;
        public long Request;
        public byte[] Payload = Array.Empty<byte>();
        public bool Control;
    }

    public static class RemoteDebugWire
    {
        public const int Version = 1;
        public const int HeaderBytes = 32;
        public const int MaxMetadataBytes = 4096;
        private const int Magic = 0x45524431;
        private const int MetadataAllowance = 32 * 1024;
        public static readonly Guid[] Channels = {
            new("fe26fdb9-9b68-4a02-b271-ff57f30bbda0"),
            new("d491ca90-4f15-4597-ade2-e98937a705c4"),
            new("3828019c-b545-4af9-903b-fb088eb4f701")
        };

        public static byte[] Encode(RemoteDebugEnvelope value)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            writer.Write(Version);
            writer.Write((int) value.Kind);
            writer.Write(value.Control);
            WriteString(writer, value.Service);
            WriteString(writer, value.Session);
            writer.Write(value.ServiceVersion);
            writer.Write(value.Request);
            WriteString(writer, value.Command);
            WriteString(writer, value.Error);
            WriteString(writer, value.Message);
            writer.Write(value.Payload.Length);
            writer.Write(value.Payload);
            return stream.ToArray();
        }

        public static RemoteDebugEnvelope Decode(byte[] bytes, int maxPayload)
        {
            using var stream = new MemoryStream(bytes, false);
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            if (reader.ReadInt32() != Version) throw new InvalidDataException("RemoteDebug protocol version mismatch.");
            var value = new RemoteDebugEnvelope {
                Kind = (RemoteDebugMessageKind) reader.ReadInt32(), Control = reader.ReadBoolean(),
                Service = ReadString(reader), Session = ReadString(reader), ServiceVersion = reader.ReadInt32(), Request = reader.ReadInt64(),
                Command = ReadString(reader), Error = ReadString(reader), Message = ReadString(reader)
            };
            if (! Enum.IsDefined(typeof(RemoteDebugMessageKind), value.Kind)) throw new InvalidDataException("Invalid message kind.");
            int length = reader.ReadInt32();
            if (length < 0 || length > maxPayload || length != stream.Length - stream.Position) throw new InvalidDataException("Invalid payload length.");
            value.Payload = reader.ReadBytes(length);
            return value;
        }

        private static void WriteString(BinaryWriter writer, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
            if (bytes.Length > MaxMetadataBytes) throw new InvalidDataException("RemoteDebug metadata exceeds limit.");
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        private static string ReadString(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            if (count < 0 || count > MaxMetadataBytes || count > reader.BaseStream.Length - reader.BaseStream.Position) throw new InvalidDataException("Invalid metadata length.");
            return new UTF8Encoding(false, true).GetString(reader.ReadBytes(count));
        }

        public static void ValidateName(string value, string parameter)
        {
            if (string.IsNullOrWhiteSpace(value) || Encoding.UTF8.GetByteCount(value) > MaxMetadataBytes)
                throw new ArgumentException("RemoteDebug command or topic is empty or exceeds the metadata limit.", parameter);
        }

        public static string LimitError(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            if (bytes.Length <= MaxMetadataBytes) return value;
            int count = MaxMetadataBytes;
            // 截断点不能落在 UTF-8 多字节字符中间。
            while ((bytes[count] & 0xc0) == 0x80) count--;
            return Encoding.UTF8.GetString(bytes, 0, count);
        }

        public static IEnumerable<byte[]> Split(byte[] bytes, int chunkBytes)
        {
            byte[] id = Guid.NewGuid().ToByteArray();
            for (int offset = 0; offset < bytes.Length; offset += chunkBytes)
            {
                int count = Math.Min(chunkBytes, bytes.Length - offset);
                using var stream = new MemoryStream(HeaderBytes + count);
                using var writer = new BinaryWriter(stream);
                writer.Write(Magic);
                writer.Write(Version);
                writer.Write(id);
                writer.Write(bytes.Length);
                writer.Write(offset);
                writer.Write(bytes, offset, count);
                yield return stream.ToArray();
            }
        }

        public sealed class Assembler : IDisposable
        {
            private sealed class Buffer
            {
                public byte[] Bytes;
                public int Received;
                public double Started;
            }

            private readonly Dictionary<(int peer, RemoteDebugLane lane, Guid id), Buffer> _buffers = new();
            private readonly RemoteDebugOptions _options;
            private int _bytes;

            public Assembler(RemoteDebugOptions options)
            {
                _options = options;
            }

            public byte[] Accept(int peer, RemoteDebugLane lane, byte[] frame, double now)
            {
                if (frame == null || frame.Length <= HeaderBytes || frame.Length > HeaderBytes + _options.ChunkBytes) throw new InvalidDataException("Invalid fragment size.");
                using var reader = new BinaryReader(new MemoryStream(frame, false));
                if (reader.ReadInt32() != Magic || reader.ReadInt32() != Version) throw new InvalidDataException("RemoteDebug fragment version mismatch.");
                var key = (peer, lane, new Guid(reader.ReadBytes(16)));
                int length = reader.ReadInt32(), offset = reader.ReadInt32(), count = frame.Length - HeaderBytes;
                int max = lane == RemoteDebugLane.State ? _options.MaxStateBytes : _options.MaxMessageBytes;
                if (length <= 0 || length > max + MetadataAllowance || offset < 0 || offset > length - count) throw new InvalidDataException("Invalid fragment bounds.");
                if (! _buffers.TryGetValue(key, out var buffer))
                {
                    if (offset != 0 || _buffers.Count >= _options.MaxAssemblies || length > _options.MaxAssemblyBytes - _bytes)
                        throw new InvalidDataException("RemoteDebug assembly budget exceeded or first fragment missing.");
                    buffer = new Buffer {Bytes = new byte[length], Started = now};
                    _buffers.Add(key, buffer);
                    _bytes += length;
                }
                if (length != buffer.Bytes.Length || offset != buffer.Received || now - buffer.Started > _options.AssemblyTimeoutSeconds)
                {
                    Remove(key);
                    throw new InvalidDataException("RemoteDebug fragment is out of order or expired.");
                }
                System.Buffer.BlockCopy(frame, HeaderBytes, buffer.Bytes, offset, count);
                buffer.Received += count;
                if (buffer.Received != length) return null;
                Remove(key);
                return buffer.Bytes;
            }

            private void Remove((int peer, RemoteDebugLane lane, Guid id) key)
            {
                _bytes -= _buffers[key].Bytes.Length;
                _buffers.Remove(key);
            }

            public void Expire(double now, int? peer = null)
            {
                var expired = new List<(int peer, RemoteDebugLane lane, Guid id)>();
                foreach (var pair in _buffers)
                    if (peer == pair.Key.peer || now - pair.Value.Started > _options.AssemblyTimeoutSeconds)
                        expired.Add(pair.Key);
                foreach (var key in expired) Remove(key);
            }

            public void Dispose()
            {
                _buffers.Clear();
                _bytes = 0;
            }
        }
    }
}