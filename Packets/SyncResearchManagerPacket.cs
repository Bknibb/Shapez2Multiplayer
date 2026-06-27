using Core.Events;
using Core.Localization;
using Game.Core.Research;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Shapez2Multiplayer.Packets
{
    public class SyncResearchManagerPacket : IPacket
    {
        public ResearchManager.SerializedData ResearchManagerSerializedData;
        public SyncResearchManagerPacket() { }
        public SyncResearchManagerPacket(ResearchManager.SerializedData researchManagerSerializedData)
        {
            ResearchManagerSerializedData = researchManagerSerializedData;
        }
        public SyncResearchManagerPacket(ResearchManager researchManager)
        {
            ResearchManagerSerializedData = Encoding.SerializeResearchManager(researchManager);
        }

        public void Decode(Stream stream)
        {
            ResearchManagerSerializedData = Encoding.DecodeResearchManagerSerializedData(stream);
        }

        public bool Encode(Stream stream)
        {
            Encoding.Encode(ResearchManagerSerializedData, stream);
            return true;
        }
        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            if (connection != null)
            {
                Shapez2Multiplayer.logger.Warning.Log("Tried to handle SyncResearchManagerPacket on server");
                return;
            }
            var researchManager = Shapez2Multiplayer.Research;
            foreach (var id in ResearchManagerSerializedData.ResearchProgress.UnlockedUpgradeIds)
            {
                var upgradeId = new Game.Core.Research.ResearchUpgradeId(id);
                if (!researchManager.Progress.IsManuallyUnlocked(upgradeId))
                {
                    var upgrade = researchManager.Layout.GetUpgrade(upgradeId);
                    researchManager.UnlockManager._OnPlayerAboutToUnlockResearch.Invoke(upgrade);
                    researchManager.UnlockManager.TryUnlock(upgrade, true);
                    researchManager.UnlockManager._OnResearchManuallyUnlockedByPlayer.Invoke(upgrade);
                }
            }
            var ShapeIdManager = researchManager.ShapeStorage.ShapeIdManager;
            foreach (var kvp in ResearchManagerSerializedData.Shapes.StoredShapes)
            {
                var shapeId = ShapeIdManager.Resolve(kvp.Key);
                var current = researchManager.ShapeStorage.GetAmount(shapeId);
                if (current < kvp.Value)
                {
                    researchManager.ShapeStorage.Add(shapeId, (uint)(kvp.Value - current));
                } else if (current > kvp.Value)
                {
                    researchManager.ShapeStorage.TryTake(shapeId, (uint)(current - kvp.Value));
                }
            }
            researchManager.BlueprintCurrencyManager.SetBlueprintCurrency(ResearchManagerSerializedData.BlueprintCurrency.BlueprintCurrency);
            researchManager.BlueprintCurrencyManager.TotalAmountSpent = ResearchManagerSerializedData.BlueprintCurrency.TotalAmountSpent;
            if (researchManager.PointStorage.Points.Amount != ResearchManagerSerializedData.PointCurrency.Points) researchManager.PointStorage.Set(new ResearchPointCurrency(ResearchManagerSerializedData.PointCurrency.Points));
            researchManager.PointStorage.TotalSpent = new ResearchPointCurrency(ResearchManagerSerializedData.PointCurrency.TotalSpent);
            foreach (var kvp in ResearchManagerSerializedData.LinearUpgrades.UpgradeLevels)
            {
                var linearUpgradeId = new ResearchLinearUpgradeId(kvp.Key);
                if (!researchManager.LinearUpgradeManager.Levels.TryGetValue(linearUpgradeId, out int level) || level != kvp.Value)
                {
                    researchManager.LinearUpgradeManager.SetLevel(linearUpgradeId, kvp.Value);
                    //if (researchManager.LinearUpgradeManager.TryGetUpgrade(linearUpgradeId, out var _Upgrade))
                    //{
                    //    Shapez2Multiplayer.PassiveEventBus.Emit<PlayerUpgradedLinearUpgradeEvent>(new PlayerUpgradedLinearUpgradeEvent(Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer, _Upgrade));
                    //    Shapez2Multiplayer.HudEvents.ShowEpicNotification.Invoke(new HUDEpicNotificationData("research.research-linear-upgrade-improved-notification.title".T(),
                    //    "research.research-linear-upgrade-improved-notification.description".T().Bind("name", _Upgrade.Title).Bind("level", StringFormatting.FormatGenericCount(kvp.Value + 1))));
                    //}
                }
            }
            for (int i = researchManager.PlayerLevel.Level; i < ResearchManagerSerializedData.PlayerLevel.Level; i++)
            {
                researchManager.PlayerLevel.GrantPlayerLevel();
            }
            var levels = researchManager.PlayerLevelGoals.Levels;
            var ResearchPlayerLevelGoalManagerOnLeveledUp = researchManager.PlayerLevelGoals._OnLeveledUp;
            var ResearchPlayerLevelGoalManagerOnChanged = researchManager.PlayerLevelGoals._OnChanged;
            foreach (var kvp in ResearchManagerSerializedData.PlayerLevelGoals.GoalLevels)
            {
                var levelGoalId = new PlayerLevelGoalId(kvp.Key);
                var currentLevel = researchManager.PlayerLevelGoals.GetLevel(levelGoalId);
                if (currentLevel < kvp.Value)
                {
                    //for (int i = currentLevel; i < kvp.Value; i++)
                    //{
                    //    if (!researchManager.PlayerLevelGoals.TryLevelUp(levelGoalId))
                    //    {
                    //        Shapez2Multiplayer.logger.Warning.Log("Failed To Level Up, Likely Desync");
                    //        break;
                    //    }
                    //    else
                    //    {
                    //        Shapez2Multiplayer.GameSessionOrchestratorDependencyContainer.Resolve<IUISoundPlayer>().PlayResearchUnlocked();
                    //    }
                    //}
                    levels[levelGoalId] = kvp.Value;
                    ResearchPlayerLevelGoalManagerOnLeveledUp.Invoke(levelGoalId, kvp.Value);
                    ResearchPlayerLevelGoalManagerOnChanged.Invoke();
                    Shapez2Multiplayer.GameSessionOrchestratorDependencyContainer.Resolve<IUISoundPlayer>().PlayResearchUnlocked();
                } else if (currentLevel > kvp.Value)
                {
                    levels[levelGoalId] = kvp.Value;
                    ResearchPlayerLevelGoalManagerOnChanged.Invoke();
                }
            }
        }
    }
}
