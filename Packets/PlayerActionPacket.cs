using System.IO;
using System.Linq;

namespace Shapez2Multiplayer.Packets
{
    public class PlayerActionPacket : IPacket
    {
        public IPlayerAction PlayerAction { get; set; }
        public PlayerActionPacket() { }
        public PlayerActionPacket(IPlayerAction playerAction)
        {
            PlayerAction = playerAction;
        }
        public void Decode(Stream stream)
        {
            Encoding.serializationVisitor = new BinarySerializationVisitor(false, false, Savegame.CurrentVersion, stream, Shapez2Multiplayer.GameSessionOrchestrator.DataSerializers, Shapez2Multiplayer.logger);
            if (stream.Position >= stream.Length)
            {
                Shapez2Multiplayer.logger.Error.Log("Recieved empty player action packet");
                return;
            }
            PlayerAction = Encoding.DecodePlayerAction(stream);
        }

        public bool Encode(Stream stream)
        {
            Encoding.serializationVisitor = new BinarySerializationVisitor(true, false, Savegame.CurrentVersion, stream, Shapez2Multiplayer.GameSessionOrchestrator.DataSerializers, Shapez2Multiplayer.logger);
            Encoding.Encode(PlayerAction, stream);
            return true;
        }

        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            if (PlayerAction == null) return;
#if DEBUG
            Shapez2Multiplayer.DebugLastAction = PlayerAction;
#endif
            Shapez2Multiplayer.WaitingActions.Add(PlayerAction);
            if ((PlayerAction is ResearchUpgradePlayerAction || PlayerAction is LevelUpLinearUpgradePlayerAction) && connection != null)
            {
                if (!Shapez2Multiplayer.PlayerActions.TryScheduleAction(PlayerAction))
                {
                    Shapez2Multiplayer.logger.Warning.Log("Action Failed, Likely Desync");
                    Shapez2Multiplayer.WaitingActions.Remove(PlayerAction);
                    MultiplayerCore.socketManager.SendToAll(new SyncResearchManagerPacket(Shapez2Multiplayer.Research));
                }
            }
            if (PlayerAction is ActionModifyBuildings actionModifyBuildings)
            {
                foreach (var delete in actionModifyBuildings.Data.Delete)
                {
                    Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer.InteractionState.BuildingSelection.Remove(Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer.InteractionState.BuildingSelection.Where(b => b.Id == delete.BuildingId));
                }
            } else if (PlayerAction is ActionModifyIsland actionModifyIsland)
            {
                foreach (var delete in actionModifyIsland.Data.Delete)
                {
                    Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer.InteractionState.BuildingSelection.Remove(Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer.InteractionState.BuildingSelection.Where(b => b.Island.Id == delete.IslandId));
                    Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer.InteractionState.IslandSelection.Remove(Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer.InteractionState.IslandSelection.Where(i => i.Id == delete.IslandId));
                }
            }
            if (!Shapez2Multiplayer.PlayerActions.TryScheduleActionNoDetection(PlayerAction))
            {
                Shapez2Multiplayer.logger.Warning.Log("Action Failed, Likely Desync");
                Shapez2Multiplayer.WaitingActions.Remove(PlayerAction);
            }
        }
    }
}
