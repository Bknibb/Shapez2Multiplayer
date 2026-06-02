using System.IO;

namespace Shapez2Multiplayer.Packets
{
    public class ViewportPropertyChangedPacket : IPacket
    {
        public short IslandLayer;
        public short BuildingLayer;
        public bool ShowAllBuildingLayers;
        public bool ShowAllIslandLayers;
        public ViewportPropertyChangedPacket() { }
        public ViewportPropertyChangedPacket(short islandLayer, short buildingLayer, bool showAllBuildingLayers, bool showAllIslandLayers)
        {
            IslandLayer = islandLayer;
            BuildingLayer = buildingLayer;
            ShowAllBuildingLayers = showAllBuildingLayers;
            ShowAllIslandLayers = showAllIslandLayers;
        }
        public void Decode(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream);
            IslandLayer = reader.ReadInt16();
            BuildingLayer = reader.ReadInt16();
            ShowAllBuildingLayers = reader.ReadBoolean();
            ShowAllIslandLayers = reader.ReadBoolean();
        }

        public bool Encode(Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(IslandLayer);
            writer.Write(BuildingLayer);
            writer.Write(ShowAllBuildingLayers);
            writer.Write(ShowAllIslandLayers);
            return true;
        }

        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            var cursor = routedFrom != null ? HUDMultiplayerCursors.Instance.Cursors.FirstOrDefault(c => c.Connection.Equals(routedFrom)) : connection == null ? HUDMultiplayerCursors.Instance.HostCursor : HUDMultiplayerCursors.Instance.Cursors.FirstOrDefault(c => c.Connection.Equals(connection));
            if (cursor == null) return;
            cursor.ViewportIslandLayer = IslandLayer;
            cursor.ViewportBuildingLayer = BuildingLayer;
            cursor.ViewportShowAllBuildingLayers = ShowAllBuildingLayers;
            cursor.ViewportShowAllIslandLayers = ShowAllIslandLayers;
        }
    }
}
