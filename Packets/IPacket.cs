using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Shapez2Multiplayer.Packets
{
    public interface IPacket
    {
        public void Encode(Stream stream);
        public void Decode(Stream stream);
        public void Handle(IConnection? connection, InfoConnection? routedFrom = null);
    }
}
