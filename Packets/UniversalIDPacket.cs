using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Shapez2Multiplayer.Packets
{
    public class UniversalIDPacket : IPacket
    {
        public uint UniversalId;
        public UniversalIDPacket() { }
        public UniversalIDPacket(uint universalId)
        {
            UniversalId = universalId;
        }

        public void Decode(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream);
            UniversalId = reader.ReadUInt32();
        }

        public bool Encode(Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(UniversalId);
            return true;
        }

        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            if (MultiplayerCore.connectionManager != null) MultiplayerCore.connectionManager.UniversalId = UniversalId;
        }
    }
}
