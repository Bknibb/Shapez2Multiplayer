using Core.Collections;
using Game.Core.Coordinates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Shapez2Multiplayer.Packets
{
    public class UpdateIslandMassSelectionPacket : IPacket
    {
        public HUDIslandMassSelection HUDIslandMassSelection;
        public List<IslandModel> IslandSelection;
        public UpdateIslandMassSelectionPacket() { }
        public UpdateIslandMassSelectionPacket(HUDIslandMassSelection hudIslandMassSelection, List<IslandModel> islandSelection)
        {
            HUDIslandMassSelection = hudIslandMassSelection;
            IslandSelection = islandSelection;
        }
        public void Decode(Stream stream)
        {
            Encoding.serializationVisitor = new BinarySerializationVisitor(false, false, Savegame.CurrentVersion, stream, Shapez2Multiplayer.GameSessionOrchestrator.DataSerializers, Shapez2Multiplayer.logger);
            using BinaryReader reader = new BinaryReader(stream);
            HUDIslandMassSelection = Encoding.DecodeHUDIslandMassSelection(stream);
            IslandSelection = new List<IslandModel>();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                if (Shapez2Multiplayer.MapModel.TryGetIsland(Encoding.DecodeGlobalChunkCoordinate(stream), out IslandModel island))
                {
                    IslandSelection.Add(island);
                }
            }
        }

        public void Encode(Stream stream)
        {
            Encoding.serializationVisitor = new BinarySerializationVisitor(true, false, Savegame.CurrentVersion, stream, Shapez2Multiplayer.GameSessionOrchestrator.DataSerializers, Shapez2Multiplayer.logger);
            using BinaryWriter writer = new BinaryWriter(stream);
            Encoding.Encode(HUDIslandMassSelection, stream);
            writer.Write(IslandSelection.Count);
            foreach (var island in IslandSelection)
            {
                Encoding.Encode(island.Position, stream);
            }
        }

        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            var islandMassSelection = routedFrom != null ? MultiplayerCore.connectionManager.PlayersIslandMassSelections[routedFrom.UniversalId] : connection == null ? MultiplayerCore.connectionManager.HostIslandMassSelection : MultiplayerCore.socketManager.PlayersIslandMassSelections[connection.UniversalId];
            islandMassSelection.Update(HUDIslandMassSelection);
            islandMassSelection.Selection.Set(IslandSelection);
        }
    }
}
