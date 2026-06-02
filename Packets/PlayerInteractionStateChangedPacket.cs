using System.IO;

namespace Shapez2Multiplayer.Packets
{
    public class PlayerInteractionStateChangedPacket : IPacket
    {
        public PlayerInteractionState State;
        public PlayerInteractionStateChangedPacket() { }
        public PlayerInteractionStateChangedPacket(PlayerInteractionState state)
        {
            State = state;
        }
        public void Decode(Stream stream)
        {
            State = (PlayerInteractionState)stream.ReadByte();
        }

        public bool Encode(Stream stream)
        {
            stream.WriteByte((byte)State);
            return true;
        }

        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            var cursor = routedFrom != null ? HUDMultiplayerCursors.Instance.Cursors.FirstOrDefault(c => c.Connection.Equals(routedFrom)) : connection == null ? HUDMultiplayerCursors.Instance.HostCursor : HUDMultiplayerCursors.Instance.Cursors.FirstOrDefault(c => c.Connection.Equals(connection));
            if (cursor == null) return;
            cursor.PlayerInteractionState = State;
        }
    }
}
