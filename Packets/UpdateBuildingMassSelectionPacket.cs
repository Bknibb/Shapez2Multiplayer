using Core.Collections;
using Game.Core.Coordinates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Shapez2Multiplayer.Packets
{
    public class UpdateBuildingMassSelectionPacket : IPacket
    {
        public HUDBuildingMassSelection HUDBuildingMassSelection;
        public List<BuildingModel> BuildingSelection;
        public UpdateBuildingMassSelectionPacket() { }
        public UpdateBuildingMassSelectionPacket(HUDBuildingMassSelection hudBuildingMassSelection, List<BuildingModel> buildingSelection)
        {
            HUDBuildingMassSelection = hudBuildingMassSelection;
            BuildingSelection = buildingSelection;
        }
        public void Decode(Stream stream)
        {
            Encoding.serializationVisitor = new BinarySerializationVisitor(false, false, Savegame.CurrentVersion, stream, Shapez2Multiplayer.GameSessionOrchestrator.DataSerializers, Shapez2Multiplayer.logger);
            using BinaryReader reader = new BinaryReader(stream);
            HUDBuildingMassSelection = Encoding.DecodeHUDBuildingMassSelection(stream);
            BuildingSelection = new List<BuildingModel>();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                if (Shapez2Multiplayer.MapModel.TryGetBuilding(Encoding.DecodeGlobalTileCoordinate(stream), out BuildingModel building))
                {
                    BuildingSelection.Add(building);
                }
            }
        }

        public void Encode(Stream stream)
        {
            Encoding.serializationVisitor = new BinarySerializationVisitor(true, false, Savegame.CurrentVersion, stream, Shapez2Multiplayer.GameSessionOrchestrator.DataSerializers, Shapez2Multiplayer.logger);
            using BinaryWriter writer = new BinaryWriter(stream);
            Encoding.Encode(HUDBuildingMassSelection, stream);
            writer.Write(BuildingSelection.Count);
            foreach (var building in BuildingSelection)
            {
                Encoding.Encode(building.Tile_G, stream);
            }
        }

        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            var buildingMassSelection = routedFrom != null ? MultiplayerCore.connectionManager.PlayersBuildingMassSelections[routedFrom.UniversalId] : connection == null ? MultiplayerCore.connectionManager.HostBuildingMassSelection : MultiplayerCore.socketManager.PlayersBuildingMassSelections[connection.UniversalId];
            buildingMassSelection.Update(HUDBuildingMassSelection);
            buildingMassSelection.Selection.Set(BuildingSelection);
        }
    }
}
