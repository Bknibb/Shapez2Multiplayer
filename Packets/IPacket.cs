using System.IO;

namespace Shapez2Multiplayer.Packets
{
    public interface IPacket
    {
        public bool Encode(Stream stream);
        public void Decode(Stream stream);
        public void Handle(IConnection? connection, InfoConnection? routedFrom = null);
    }
}
