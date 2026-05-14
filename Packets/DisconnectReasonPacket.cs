using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Shapez2Multiplayer.Packets
{
    public class DisconnectReasonPacket : IPacket
    {
        public MultiplayerCore.DisconnectReason DisconnectReason { get; set; }
        public DisconnectReasonPacket() { }
        public DisconnectReasonPacket(MultiplayerCore.DisconnectReason reason) {
            DisconnectReason = reason;
        }
        public void Decode(Stream stream)
        {
            DisconnectReason = (MultiplayerCore.DisconnectReason)stream.ReadByte();
        }

        public bool Encode(Stream stream)
        {
            stream.WriteByte((byte)DisconnectReason);
            return true;
        }

        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            if (MultiplayerCore.Client)
            {
                MultiplayerCore.connectionManager.RecievedDisconnectReason = DisconnectReason;
            }
        }
    }
}
