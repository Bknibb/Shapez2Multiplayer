using System.IO;
using System.Text;

namespace Shapez2Multiplayer
{
    public class InfoConnection : IConnection
    {
        public InfoConnection()
        {
            
        }
        public InfoConnection(IConnection connection)
        {
            Id = connection.Id;
            UniversalId = connection.UniversalId;
            Name = connection.Name;
            Ping = connection.Ping;
        }

        public uint Id { get; private set; }
        public uint UniversalId { get; private set; }
        public string? Name { get; private set; }
        public int Ping { get; private set; }

        public void Close()
        {
            
        }

        public bool Send(byte[] data, Packets.Packet packet)
        {
            return false;
        }
        public void Encode(Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, true);
            writer.Write(Id);
            writer.Write(UniversalId);
            writer.Write(Name != null);
            if (Name != null) writer.Write(Name);
            writer.Write(Ping);
        }
        public static InfoConnection Decode(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, true);
            InfoConnection connection = new InfoConnection();
            connection.Id = reader.ReadUInt32();
            connection.UniversalId = reader.ReadUInt32();
            if (reader.ReadBoolean())
            {
                connection.Name = reader.ReadString();
            }
            else
            {
                connection.Name = null;
            }
            connection.Ping = reader.ReadInt32();
            return connection;
        }
        public void Update(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, true);
            Id = reader.ReadUInt32();
            UniversalId = reader.ReadUInt32();
            if (reader.ReadBoolean())
            {
                Name = reader.ReadString();
            } else
            {
                Name = null;
            }
            Ping = reader.ReadInt32();
        }
        public void Update(InfoConnection infoConnection)
        {
            Id = infoConnection.Id;
            UniversalId = infoConnection.UniversalId;
            Name = infoConnection.Name;
            Ping = infoConnection.Ping;
        }

        public bool Equals(IConnection? other)
        {
            return other is InfoConnection connection && Equals(connection);
        }
        public bool Equals(InfoConnection? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return UniversalId == other.UniversalId;
        }
        public override bool Equals(object obj)
        {
            return obj is InfoConnection connection && Equals(connection);
        }
        public override int GetHashCode()
        {
            return (int)UniversalId;
        }
        public static bool operator ==(InfoConnection? left, InfoConnection? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null) return false;
            return left.Equals(right);
        }
        public static bool operator !=(InfoConnection? left, InfoConnection? right) => !(left == right);
    }
}
