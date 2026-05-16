using Game.Core.Research;
using Game.HUD.QuestArea.PinnedShapes;
using Game.Placement.Data;
using Shapez2Multiplayer.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shapez2Multiplayer
{
    public static class MultiplayerEvents
    {
        public static void OnPinAdded(IPin pin)
        {
            if (Shapez2Multiplayer.IgnorePinEvents) return;
            MultiplayerCore.SendToAll(new PinChangePacket(pin, false));
        }
        public static void OnPinRemoved(IPin pin)
        {
            if (Shapez2Multiplayer.IgnorePinEvents) return;
            MultiplayerCore.SendToAll(new PinChangePacket(pin, true));
        }
        public static void OnPlacementDataChanged(IPlacementData placementData, PlacementInputHolder placementInput)
        {
            var packet = new PlacementIndicatorDataPacket(placementData, placementInput);
            var previousChunkedPackets = ChunkedPacket.ToSend.Where(c => c.Item3 == Packet.PlacementIndicatorData).Select(c => c.Item1.Id).Distinct().ToList();
            MultiplayerCore.SendToAll(packet);
            if (packet.Result)
            {
                foreach (var chunkId in previousChunkedPackets)
                {
                    ChunkedPacket.Cancel(chunkId);
                }
            }
        }
        public static void OnResearchLinearUpgradeManagerChanged(ResearchLinearUpgradeId researchLinearUpgradeId, int level)
        {
            if (!MultiplayerCore.Hosting) throw new Exception("OnResearchLinearUpgradeManagerChanged Should Only Be Called On Host");
            MultiplayerCore.socketManager.SyncResearchTimer = 0.0f;
            MultiplayerCore.socketManager.SendToAll(new SyncResearchManagerPacket(Shapez2Multiplayer.Research));
        }
        public static void OnResearchPlayerLevelManagerChanged()
        {
            if (!MultiplayerCore.Hosting) throw new Exception("OnResearchPlayerLevelManagerChanged Should Only Be Called On Host");
            MultiplayerCore.socketManager.SyncResearchTimer = 0.0f;
            MultiplayerCore.socketManager.SendToAll(new SyncResearchManagerPacket(Shapez2Multiplayer.Research));
        }
        public static void OnResearchPlayerLevelGoalManagerChanged()
        {
            if (!MultiplayerCore.Hosting) throw new Exception("OnResearchPlayerLevelGoalManagerChanged Should Only Be Called On Host");
            MultiplayerCore.socketManager.SyncResearchTimer = 0.0f;
            MultiplayerCore.socketManager.SendToAll(new SyncResearchManagerPacket(Shapez2Multiplayer.Research));
        }
        public static void OnResearchUnlockProgressManagerChanged()
        {
            if (!MultiplayerCore.Hosting) throw new Exception("OnResearchUnlockProgressManagerChanged Should Only Be Called On Host");
            MultiplayerCore.socketManager.SyncResearchTimer = 0.0f;
            MultiplayerCore.socketManager.SendToAll(new SyncResearchManagerPacket(Shapez2Multiplayer.Research));
        }
        public static void OnResearchUnlockManagerResearchManuallyUnlockedByPlayer(IResearchUpgrade upgrade)
        {
            if (!MultiplayerCore.Hosting) throw new Exception("OnResearchUnlockManagerResearchManuallyUnlockedByPlayer Should Only Be Called On Host");
            MultiplayerCore.socketManager.SyncResearchTimer = 0.0f;
            MultiplayerCore.socketManager.SendToAll(new SyncResearchManagerPacket(Shapez2Multiplayer.Research));
        }
    }
}
