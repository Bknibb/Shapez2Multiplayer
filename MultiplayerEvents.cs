using Game.Core.Research;
using Game.HUD.QuestArea.PinnedShapes;
using Game.Placement.Data;
using Shapez2Multiplayer.Packets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shapez2Multiplayer
{
    public static class MultiplayerEvents
    {
        public static void OnPinAdded(IPin pin)
        {
            MultiplayerCore.socketManager.SendToAll(new PinChangePacket(pin, false));
        }
        public static void OnPinRemoved(IPin pin)
        {
            MultiplayerCore.socketManager.SendToAll(new PinChangePacket(pin, true));
        }
        public static void OnPlacementDataChanged(IPlacementData placementData, PlacementInputHolder placementInput)
        {
            MultiplayerCore.SendToAll(new PlacementIndicatorDataPacket(placementData, placementInput));
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
