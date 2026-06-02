using Core.Events;
using Game.Core.Research;
using HarmonyLib;
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
        public static readonly FieldInfo ShapeIdManagerInfo = AccessTools.Field(typeof(ResearchShapeStorage), "ShapeIdManager");
        public static readonly PropertyInfo BlueprintCurrencyManagerTotalAmountSpentInfo = AccessTools.Property(typeof(BlueprintCurrencyManager), nameof(BlueprintCurrencyManager.TotalAmountSpent));
        public static readonly PropertyInfo ResearchPointStorageTotalSpentInfo = AccessTools.Property(typeof(ResearchPointStorage), nameof(ResearchPointStorage.TotalSpent));
        public static readonly MethodInfo ResearchLinearUpgradeManagerSetLevelInfo = AccessTools.Method(typeof(ResearchLinearUpgradeManager), "SetLevel");
        public static readonly FieldInfo ResearchUnlockManager_OnPlayerAboutToUnlockResearchInfo = AccessTools.Field(typeof(ResearchUnlockManager), "_OnPlayerAboutToUnlockResearch");
        public static readonly FieldInfo ResearchUnlockManager_OnResearchManuallyUnlockedByPlayerInfo = AccessTools.Field(typeof(ResearchUnlockManager), "_OnResearchManuallyUnlockedByPlayer");
        public static readonly FieldInfo ResearchPlayerLevelGoalManagerLevelsInfo = AccessTools.Field(typeof(ResearchPlayerLevelGoalManager), "Levels");
        public static readonly FieldInfo ResearchPlayerLevelGoalManager_OnLeveledUpInfo = AccessTools.Field(typeof(ResearchPlayerLevelGoalManager), "_OnLeveledUp");
        public static readonly FieldInfo ResearchPlayerLevelGoalManager_OnChangedInfo = AccessTools.Field(typeof(ResearchPlayerLevelGoalManager), "_OnChanged");
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
                    ((MultiRegisterEvent<IResearchUpgrade>)ResearchUnlockManager_OnPlayerAboutToUnlockResearchInfo.GetValue(researchManager.UnlockManager)).Invoke(upgrade);
                    researchManager.UnlockManager.TryUnlock(upgrade, true);
                    ((MultiRegisterEvent<IResearchUpgrade>)ResearchUnlockManager_OnResearchManuallyUnlockedByPlayerInfo.GetValue(researchManager.UnlockManager)).Invoke(upgrade);
                }
            }
            var ShapeIdManager = (IShapeIdManager)ShapeIdManagerInfo.GetValue(researchManager.ShapeStorage);
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
            BlueprintCurrencyManagerTotalAmountSpentInfo.SetValue(researchManager.BlueprintCurrencyManager, ResearchManagerSerializedData.BlueprintCurrency.TotalAmountSpent);
            if (researchManager.PointStorage.Points.Amount != ResearchManagerSerializedData.PointCurrency.Points) researchManager.PointStorage.Set(new ResearchPointCurrency(ResearchManagerSerializedData.PointCurrency.Points));
            ResearchPointStorageTotalSpentInfo.SetValue(researchManager.PointStorage, new ResearchPointCurrency(ResearchManagerSerializedData.PointCurrency.TotalSpent));
            foreach (var kvp in ResearchManagerSerializedData.LinearUpgrades.UpgradeLevels)
            {
                var linearUpgradeId = new ResearchLinearUpgradeId(kvp.Key);
                if (!researchManager.LinearUpgradeManager.Levels.TryGetValue(linearUpgradeId, out int level) || level != kvp.Value)
                {
                    ResearchLinearUpgradeManagerSetLevelInfo.Invoke(linearUpgradeId, new object[] { linearUpgradeId, kvp.Value });
                }
            }
            for (int i = researchManager.PlayerLevel.Level; i < ResearchManagerSerializedData.PlayerLevel.Level; i++)
            {
                researchManager.PlayerLevel.GrantPlayerLevel();
            }
            var levels = (Dictionary<PlayerLevelGoalId, int>)ResearchPlayerLevelGoalManagerLevelsInfo.GetValue(researchManager.PlayerLevelGoals);
            var ResearchPlayerLevelGoalManagerOnLeveledUp = (MultiRegisterEvent<PlayerLevelGoalId, int>)ResearchPlayerLevelGoalManager_OnLeveledUpInfo.GetValue(researchManager.PlayerLevelGoals);
            var ResearchPlayerLevelGoalManagerOnChanged = (MultiRegisterEvent)ResearchPlayerLevelGoalManager_OnChangedInfo.GetValue(researchManager.PlayerLevelGoals);
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
