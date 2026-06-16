using System;
using System.IO;
using System.Text;

namespace Shapez2Multiplayer.Packets
{
    public enum Packet : byte
    {
        Savegame,
        PlayerAction,
        SendToAll,
        Pause,
        FinishedConnecting,
        PlayerInfo,
        DisconnectReason,
        UpdateConnectionInfo,
        SyncResearchManager,
        PinChange,
        PlacementIndicatorData,
        UniversalID,
        UpdateBuildingMassSelection,
        UpdateIslandMassSelection,
        Chunked,
        ChunkReceived,
        Cursor,
        LevelUpPlayerLevelGoal,
        ViewportPropertyChanged,
        PlayerInteractionStateChanged,
        UpdateBuildingConfiguration
    }
    public static class PacketExtensions
    {
        public static Type GetType(this Packet packet) =>
            packet switch
            {
                Packet.Savegame => typeof(SavegamePacket),
                Packet.PlayerAction => typeof(PlayerActionPacket),
                Packet.SendToAll => typeof(SendToAllPacket),
                Packet.Pause => typeof(PausePacket),
                Packet.FinishedConnecting => typeof(FinishedConnectingPacket),
                Packet.PlayerInfo => typeof(PlayerInfoPacket),
                Packet.DisconnectReason => typeof(DisconnectReasonPacket),
                Packet.UpdateConnectionInfo => typeof(UpdateConnectionInfoPacket),
                Packet.SyncResearchManager => typeof(SyncResearchManagerPacket),
                Packet.PinChange => typeof(PinChangePacket),
                Packet.PlacementIndicatorData => typeof(PlacementIndicatorDataPacket),
                Packet.UniversalID => typeof(UniversalIDPacket),
                Packet.UpdateBuildingMassSelection => typeof(UpdateBuildingMassSelectionPacket),
                Packet.UpdateIslandMassSelection => typeof(UpdateIslandMassSelectionPacket),
                Packet.Chunked => typeof(ChunkedPacket),
                Packet.ChunkReceived => typeof(ChunkReceivedPacket),
                Packet.Cursor => typeof(CursorPacket),
                Packet.LevelUpPlayerLevelGoal => typeof(LevelUpPlayerLevelGoalPacket),
                Packet.ViewportPropertyChanged => typeof(ViewportPropertyChangedPacket),
                Packet.PlayerInteractionStateChanged => typeof(PlayerInteractionStateChangedPacket),
                Packet.UpdateBuildingConfiguration => typeof(UpdateBuildingConfigurationPacket),
                _ => throw new ArgumentException("Invalid packet"),
            };
        public static Packet GetFromType(Type type)
        {
            if (type == typeof(SavegamePacket)) return Packet.Savegame;
            else if (type == typeof(PlayerActionPacket)) return Packet.PlayerAction;
            else if (type == typeof(SendToAllPacket)) return Packet.SendToAll;
            else if (type == typeof(PausePacket)) return Packet.Pause;
            else if (type == typeof(FinishedConnectingPacket)) return Packet.FinishedConnecting;
            else if (type == typeof(PlayerInfoPacket)) return Packet.PlayerInfo;
            else if (type == typeof(DisconnectReasonPacket)) return Packet.DisconnectReason;
            else if (type == typeof(UpdateConnectionInfoPacket)) return Packet.UpdateConnectionInfo;
            else if (type == typeof(SyncResearchManagerPacket)) return Packet.SyncResearchManager;
            else if (type == typeof(PinChangePacket)) return Packet.PinChange;
            else if (type == typeof(PlacementIndicatorDataPacket)) return Packet.PlacementIndicatorData;
            else if (type == typeof(UniversalIDPacket)) return Packet.UniversalID;
            else if (type == typeof(UpdateBuildingMassSelectionPacket)) return Packet.UpdateBuildingMassSelection;
            else if (type == typeof(UpdateIslandMassSelectionPacket)) return Packet.UpdateIslandMassSelection;
            else if (type == typeof(ChunkedPacket)) return Packet.Chunked; 
            else if (type == typeof(ChunkReceivedPacket)) return Packet.ChunkReceived;
            else if (type == typeof(CursorPacket)) return Packet.Cursor;
            else if (type == typeof(LevelUpPlayerLevelGoalPacket)) return Packet.LevelUpPlayerLevelGoal;
            else if (type == typeof(ViewportPropertyChangedPacket)) return Packet.ViewportPropertyChanged;
            else if (type == typeof(PlayerInteractionStateChangedPacket)) return Packet.PlayerInteractionStateChanged;
            else if (type == typeof(UpdateBuildingConfigurationPacket)) return Packet.UpdateBuildingConfiguration;
            throw new ArgumentException("Invalid packet type");
        }
        public static byte[]? Encode(IPacket packet, uint? from = null)
        {
            using var stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, true);
            writer.Write(from != null);
            if (from != null) writer.Write(from.Value);
            stream.WriteByte((byte)GetFromType(packet.GetType()));
            if (!packet.Encode(stream)) return null;
            return stream.ToArray();
        }
        public static IPacket Decode(byte[] data, out uint? from)
        {
            using var stream = new MemoryStream(data);
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, true);
            from = null;
            if (reader.ReadBoolean()) from = reader.ReadUInt32();
            var packet = GetPacket((Packet)stream.ReadByte());
            packet.Decode(stream);
            return packet;
        }
        public static IPacket Decode(byte[] data)
        {
            return Decode(data, out uint? _);
        }
        public static IPacket GetPacket(this Packet packet)
        {
#if DEBUG
            Shapez2Multiplayer.logger.Info?.Log($"Got packet of type {packet}");
#endif
            return (IPacket)GetType(packet).GetConstructor(new Type[] { }).Invoke(new object[] { });
        }
        public static Packet PacketFromData(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, true);
            if (reader.ReadBoolean()) reader.ReadUInt32();
            return (Packet)reader.ReadByte();
        }
    }
}
