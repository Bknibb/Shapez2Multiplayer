using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Shapez2Multiplayer.Packets
{
    public class ChunkReceivedPacket : IPacket
    {
        public ChunkReceivedPacket() { }
        public void Decode(Stream stream)
        {
            
        }

        public bool Encode(Stream stream)
        {
            return true;
        }

        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            if (connection == null || (ChunkedPacket.WaitingFromId.HasValue && ChunkedPacket.WaitingFromId.Value == connection.UniversalId))
            {
                ChunkedPacket.WaitingFromId = null;
                ChunkedPacket.SendOne();
            }
        }
    }
}
