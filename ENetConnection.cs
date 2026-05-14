using ENet;
using K4os.Compression.LZ4;
using Shapez2Multiplayer.Packets;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shapez2Multiplayer
{
    public class ENetConnection : IConnection, IEquatable<ENetConnection>
    {
        public Peer Peer;
        public static readonly Dictionary<uint, string> NameCache = new Dictionary<uint, string>();
        public ENetConnection(Peer peer)
        {
            Peer = peer;
        }
        public bool Host { get; }
        public ENetConnection(Peer peer, bool host)
        {
            Peer = peer;
            Host = host;
            if (host) UniversalId = 0;
        }

        public uint Id { get => Peer.ID; }
        public uint UniversalId { get; } = MultiplayerCore.CurrentUniversalId++;
        public string? Name => NameCache.GetValueOrDefault(Id, $"Player {UniversalId}");
        public int Ping => (int)Peer.RoundTripTime;

        public void Close()
        {
            Peer.Disconnect(0);
        }

        public bool Send(byte[] data, Packets.Packet type)
        {
            var compressed = LZ4Pickler.Pickle(data);
            if (compressed.Length > ChunkedPacket.ChunkThreshold)
            {
                Shapez2Multiplayer.logger.Warning.Log($"Packet too large, sending as chunked");
                ChunkedPacket.Send(compressed, this, type);
                return true;
            }
            ENet.Packet packet = new ENet.Packet();
            packet.Create(compressed);
            return Peer.Send(0, ref packet);
        }

        public static implicit operator Peer(ENetConnection connection) => connection.Peer;

        public bool Equals(IConnection? other)
        {
            return other is ENetConnection connection && Equals(connection);
        }
        public bool Equals(ENetConnection? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }
        public override bool Equals(object obj)
        {
            return obj is ENetConnection connection && Equals(connection);
        }
        public override int GetHashCode()
        {
            return (int)Id;
        }
        public static bool operator ==(ENetConnection? left, ENetConnection? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null) return false;
            return left.Equals(right);
        }
        public static bool operator !=(ENetConnection? left, ENetConnection? right) => !(left == right);
    }
}
