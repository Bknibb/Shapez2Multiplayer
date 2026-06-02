using Game.Placement.Data;
using K4os.Hash.xxHash;
using System.IO;

namespace Shapez2Multiplayer.Packets
{
    public class PlacementIndicatorDataPacket : IPacket
    {
        public IPlacementData PlacementData;
        public PlacementInputHolder PlacementInputHolder;
        public ulong Hash;
        public bool Result;
        public PlacementIndicatorDataPacket() { }
        public static ulong LastHash = 0;
        public static bool SentToAllConnections = false;
        public PlacementIndicatorDataPacket(IPlacementData placementData, PlacementInputHolder placementInputHolder)
        {
            PlacementData = placementData;
            PlacementInputHolder = placementInputHolder;
        }

        public void Decode(Stream stream)
        {
            Encoding.serializationVisitor = new BinarySerializationVisitor(false, false, Savegame.CurrentVersion, stream, Shapez2Multiplayer.GameSessionOrchestrator.DataSerializers, Shapez2Multiplayer.logger);
            PlacementData = Encoding.DecodePlacementData(stream);
            PlacementInputHolder = Encoding.DecodePlacementInputHolder(stream);
        }

        public bool Encode(Stream stream)
        {
            Encoding.serializationVisitor = new BinarySerializationVisitor(true, false, Savegame.CurrentVersion, stream, Shapez2Multiplayer.GameSessionOrchestrator.DataSerializers, Shapez2Multiplayer.logger);
            Encoding.Encode(PlacementData, stream);
            Encoding.Encode(PlacementInputHolder, stream);
            if (stream is MemoryStream ms) Hash = XXH64.DigestOf(ms.ToArray());
            Result = Hash != LastHash || !SentToAllConnections;
            LastHash = Hash;
            return Result;
        }
        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            var drawer = routedFrom != null ? MultiplayerCore.connectionManager.PlayersDrawers[routedFrom.UniversalId] : connection == null ? MultiplayerCore.connectionManager.HostDrawer : MultiplayerCore.socketManager.PlayersDrawers[connection.UniversalId];
            drawer.OnPlacementDataChanged(PlacementData, PlacementInputHolder);
            drawer.HasData = Encoding.PlacementInputHolderInputInfo.GetValue(PlacementInputHolder) != null;
        }
    }
}
