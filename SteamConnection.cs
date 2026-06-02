using K4os.Compression.LZ4;
using Shapez2Multiplayer.Packets;
using Steamworks.Data;
using System.Collections.Generic;

namespace Shapez2Multiplayer
{
    public class SteamConnection : IConnection
    {
        public Connection Connection;
        public static readonly Dictionary<uint, string> NameCache = new Dictionary<uint, string>();

        public SteamConnection(Connection connection)
        {
            Connection = connection;
        }

        public uint Id { get => Connection.Id; set => Connection.Id = value; }
        public uint UniversalId { get; } = MultiplayerCore.CurrentUniversalId++;
        public int Ping => Connection.QuickStatus().Ping;

        public string? Name => NameCache.GetValueOrDefault(Id, $"Player {UniversalId}");

        public void Close()
        {
            Connection.Close();
        }

        public bool Send(byte[] data, Packet packet)
        {
            var compressed = LZ4Pickler.Pickle(data);
            if (compressed.Length > ChunkedPacket.ChunkThreshold)
            {
                Shapez2Multiplayer.logger.Warning.Log($"Packet too large, sending as chunked");
                ChunkedPacket.Send(compressed, this, packet);
                return true;
            }
            var result = Connection.SendMessage(compressed);
            if (result != Steamworks.Result.OK)
            {
                Shapez2Multiplayer.logger.Warning.Log($"Failed to send steam packet with error {result}");
            }
            return result == Steamworks.Result.OK;
        }

        public static implicit operator Connection(SteamConnection connection) => connection.Connection;
        public bool Equals(IConnection? other)
        {
            return other is SteamConnection connection && Equals(connection);
        }
        public bool Equals(SteamConnection? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Connection == other.Connection;
        }
        public override bool Equals(object obj)
        {
            return obj is SteamConnection connection && Equals(connection);
        }
        public override int GetHashCode()
        {
            return Connection.GetHashCode();
        }
        public static bool operator ==(SteamConnection? left, SteamConnection? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null) return false;
            return left.Equals(right);
        }
        public static bool operator !=(SteamConnection? left, SteamConnection? right) => !(left == right);
    }
}
