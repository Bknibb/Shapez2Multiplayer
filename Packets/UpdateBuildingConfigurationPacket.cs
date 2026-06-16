using Game.Core.Coordinates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Shapez2Multiplayer.Packets
{
    public class UpdateBuildingConfigurationPacket : IPacket
    {
        public GlobalTileCoordinate TileCoordinate { get; set; }
        public IBuildingConfiguration BuildingConfiguration { get; set; }
        public byte[] RemainingData { get; set; }
        public UpdateBuildingConfigurationPacket() { }
        public UpdateBuildingConfigurationPacket(GlobalTileCoordinate tileCoordinate, IBuildingConfiguration buildingConfiguration)
        {
            TileCoordinate = tileCoordinate;
            BuildingConfiguration = buildingConfiguration;
        }
        public void Decode(Stream stream)
        {
            using var reader = new BinaryReader(stream);
            TileCoordinate = Encoding.DecodeGlobalTileCoordinate(stream);
            RemainingData = reader.ReadBytes((int)(stream.Length - stream.Position));
        }

        public bool Encode(Stream stream)
        {
            var serializationVisitor = new BinarySerializationVisitor(true, false, Savegame.CurrentVersion, stream, Shapez2Multiplayer.GameSessionOrchestrator.DataSerializers, Shapez2Multiplayer.logger);
            Encoding.Encode(TileCoordinate, stream);
            BuildingConfiguration.Sync(serializationVisitor);
            return true;
        }

        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            if (Shapez2Multiplayer.MapModel.TryGetBuilding(TileCoordinate, out var building))
            {
                using var memoryStream = new MemoryStream(RemainingData);
                var serializationVisitor = new BinarySerializationVisitor(false, false, Savegame.CurrentVersion, memoryStream, Shapez2Multiplayer.GameSessionOrchestrator.DataSerializers, Shapez2Multiplayer.logger);
                building.Configuration.Sync(serializationVisitor);
            }
        }
    }
}
