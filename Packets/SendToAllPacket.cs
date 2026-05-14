using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Shapez2Multiplayer.Packets
{
    public class SendToAllPacket : IPacket
    {
        public IPacket Packet { get; set; }
        public byte[]? PacketData;
        public SendToAllPacket() { }
        public SendToAllPacket(IPacket packet)
        {
            Packet = packet;
        }
        public SendToAllPacket(byte[] packet)
        {
            PacketData = packet;
        }
        public void Decode(Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                Packet = PacketExtensions.Decode(ms.ToArray());
            }
        }

        public bool Encode(Stream stream)
        {
            if (PacketData != null)
            {
                stream.Write(PacketData);
                return true;
            }
            var data = PacketExtensions.Encode(Packet);
            if (data == null) return false;
            stream.Write(data);
            return true;
        }

        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            MultiplayerCore.socketManager.SendToAllExcept(Packet, connection);
            Packet.Handle(connection);
        }
    }
}
