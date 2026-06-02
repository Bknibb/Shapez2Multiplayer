using System.IO;
using Unity.Mathematics;

namespace Shapez2Multiplayer.Packets
{
    public class CursorPacket : IPacket
    {
        public float3 Position { get; set; }
        public CursorHoverState State { get; set; }
        public CursorPacket() { }
        public CursorPacket(float3 position, CursorHoverState state)
        {
            Position = position;
            State = state;
        }
        public void Decode(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream);
            Position = Encoding.DecodeFloat3(stream);
            State = (CursorHoverState)stream.ReadByte();
        }

        public bool Encode(Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream);
            Encoding.Encode(Position, stream);
            stream.WriteByte((byte)State);
            return true;
        }

        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            var cursor = routedFrom != null ? HUDMultiplayerCursors.Instance.GetOrAddCursor(routedFrom) : connection == null ? HUDMultiplayerCursors.Instance.GetOrAddHostCursor() : HUDMultiplayerCursors.Instance.GetOrAddCursor(connection);
            //cursor.SetFromWorldPosition(Position);
            cursor.SetWorldPosition(Position);
            cursor.UpdateImageFromState(State);
        }
    }
}
