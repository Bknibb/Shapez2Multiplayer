using Game.Core.Research;
using System.IO;

namespace Shapez2Multiplayer.Packets
{
    public class LevelUpPlayerLevelGoalPacket : IPacket
    {
        public PlayerLevelGoalId PlayerLevelGoalId;
        public LevelUpPlayerLevelGoalPacket() { }
        public LevelUpPlayerLevelGoalPacket(PlayerLevelGoalId playerLevelGoalId)
        {
            PlayerLevelGoalId = playerLevelGoalId;
        }

        public void Decode(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream);
            PlayerLevelGoalId = new PlayerLevelGoalId(reader.ReadString());
        }

        public bool Encode(Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(PlayerLevelGoalId.Id);
            return true;
        }

        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            if (connection == null) Shapez2Multiplayer.logger.Error.Log("LevelUpPlayerLevelGoalPacket should only be received on host");
            if (!Shapez2Multiplayer.Research.PlayerLevelGoals.TryLevelUp(PlayerLevelGoalId))
            {
                Shapez2Multiplayer.logger.Warning.Log("Client tried to level up player level goal when not able to, likely desync, research manager will be resynced now.");
                MultiplayerCore.socketManager.SyncResearchTimer = 0.0f;
                MultiplayerCore.socketManager.SendToAll(new SyncResearchManagerPacket(Shapez2Multiplayer.Research));
            }
        }
    }
}
