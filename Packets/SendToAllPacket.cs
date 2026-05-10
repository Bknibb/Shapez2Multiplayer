using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Shapez2Multiplayer.Packets
{
    public class SendToAllPacket : IPacket
    {
        public IPacket Packet { get; set; }
        public SendToAllPacket() { }
        public SendToAllPacket(IPacket packet)
        {
            Packet = packet;
        }
        public void Decode(Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                Packet = PacketExtensions.Decode(ms.ToArray());
            }
        }

        public void Encode(Stream stream)
        {
            stream.Write(PacketExtensions.Encode(Packet));
        }

        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            MultiplayerCore.socketManager.SendToAllExcept(Packet, connection);
            Packet.Handle(connection);
        }
    }
}
