using Game.Placement.Data;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Shapez2Multiplayer.Packets
{
    public class PlacementIndicatorDataPacket : IPacket
    {
        public IPlacementData PlacementData;
        public PlacementInputHolder PlacementInputHolder;
        public PlacementIndicatorDataPacket() { }
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

        public void Encode(Stream stream)
        {
            Encoding.serializationVisitor = new BinarySerializationVisitor(true, false, Savegame.CurrentVersion, stream, Shapez2Multiplayer.GameSessionOrchestrator.DataSerializers, Shapez2Multiplayer.logger);
            Encoding.Encode(PlacementData, stream);
            Encoding.Encode(PlacementInputHolder, stream);
        }
        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            var drawer = routedFrom != null ? MultiplayerCore.connectionManager.PlayersDrawers[routedFrom.UniversalId] : connection == null ? MultiplayerCore.connectionManager.HostDrawer : MultiplayerCore.socketManager.PlayersDrawers[connection.UniversalId];
            drawer.OnPlacementDataChanged(PlacementData, PlacementInputHolder);
            drawer.HasData = Encoding.PlacementInputHolderInputInfo.GetValue(PlacementInputHolder) != null;
        }
    }
}
