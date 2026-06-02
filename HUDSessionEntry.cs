using Core.Dependency;
using Core.Localization;
using Game.Core.GameData.GameModeDefinition;
using Game.Core.GameData.Presets;
using Game.Core.Modding;
using Game.Core.Mode;
using Game.Core.Research;
using HarmonyLib;
using Shapez2UILib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using static Shapez2Multiplayer.HUDMenuMultiplayerState;

namespace Shapez2Multiplayer
{
    public class HUDSessionEntry : HUDComponent
    {
        private ILobbyData _Entry;
        private IGameData GameData;
        private GeneralGameSettings GeneralSettings;
        private ILocalizationResolver LocalizationResolver;
        private Core.Logging.ILogger Logger;
        private IMainMenuStateControl MainMenu;
        private ISavegameManager SavegameManager;
        private SavegameModdingContextDivergenceBarrier SavegameModdingContextDivergenceBarrier;
        private HUDButton UIBtnJoinGame;
        private GameObject UICompletedIndicator;
        private GameObject UILoadFailedOverlay;
        private HUDTooltipTarget UIModList;
        private HUDLocalizedText UIModsCount;
        private HUDLocalizedText UINameText;
        private HUDLocalizedText UISavegameUIDText;
        private IUISoundPlayer UISoundPlayer;
        private HUDLocalizedText UIStatDifficulty;
        private HUDLocalizedText UIStatGameRules;
        private HUDLocalizedText UIStatMode;
        private HUDLocalizedText UIStatPlaytime;
        private HUDLocalizedText UIStatResearchProgress;
        private HUDLocalizedText UIStatScenario;
        private HUDLocalizedText UIStatStructureCount;
        private GameObject UIVersionMismatchOverlay;
        private readonly MethodInfo CreateModList = AccessTools.Method(typeof(HUDSavegameEntryPrefab), "CreateModList");
        public ILobbyData Entry
        {
            get
            {
                return this._Entry;
            }
            set
            {
                SetSession(value);
            }
        }
        [Construct]
        private void Construct(IHUDDialogStack dialogStack, ISavegameManager savegameManager, IMainMenuStateControl mainMenu, IUISoundPlayer uiSoundPlayer, Core.Logging.ILogger logger, ILocalizationResolver localizationResolver, IGameData gameData, GeneralGameSettings generalSettings, IModdingFrameworkEnvironment moddingEnvironment)
        {
            this.SavegameManager = savegameManager;
            this.MainMenu = mainMenu;
            this.UISoundPlayer = uiSoundPlayer;
            this.Logger = logger;
            this.LocalizationResolver = localizationResolver;
            this.GameData = gameData;
            this.GeneralSettings = generalSettings;
            this.SavegameModdingContextDivergenceBarrier = new SavegameModdingContextDivergenceBarrier(moddingEnvironment, dialogStack);
        }

        protected override void OnDispose()
        {
            this.UIBtnJoinGame.OnClick.RemoveListener(new UnityAction(this.OnClickJoinButton));
        }

        private void SetSession(ILobbyData lobby)
        {
            _Entry = lobby;
            SetUIToFailureState();
            try
            {
                UINameText.Text = new RawText(lobby.GetData("name") + " - " + lobby.AdditionalTitle);
                if (!IsVerCompatible((GameVersion)int.Parse(lobby.GetData("gamever")), lobby.GetData("mode"), lobby.GetData("scenario")))
                {
                    UIVersionMismatchOverlay.SetActiveSelfExt(true);
                }
                string text = "UID " + lobby.GetData("uid") + " | v" + lobby.GetData("gamever") + " | src " + lobby.GetData("appsourceversion") + " | " + lobby.GetData("appsourceenvironment") + " | " + lobby.GetData("appsourcestore");
                if (bool.Parse(lobby.GetData("cheatsused")))
                {
                    text += " [cheat]";
                }
                UICompletedIndicator.gameObject.SetActiveSelfExt(bool.Parse(lobby.GetData("completed")));
                UISavegameUIDText.Text = new RawText(text);
                GameScenario scenario = GameData.GetScenario(new ScenarioId(lobby.GetData("scenario")));
                UIStatScenario.Text = scenario.Title;
                GameModeId gameModeId = scenario.SupportedGameModes.First<GameModeId>();
                GameModeDefinition gameModeDefinition = GameData.GetGameModeDefinition(gameModeId);
                UIStatMode.Text = gameModeDefinition.Title;
                UIBtnJoinGame.Interactable = true;
                DifficultyGameParameters difficultyGameParameters = new DifficultyGameParameters(new DifficultyGameParameters.SerializedData() { ResearchShapeCostMultiplier = int.Parse(lobby.GetData("difficultyresearchshapecost")), ChunkLimitMultiplier = int.Parse(lobby.GetData("difficultychunklimit")), BlueprintCostMultiplier = int.Parse(lobby.GetData("difficultyblueprintcost")) });
                GameDifficultyPreset gameDifficultyPreset = null;
                foreach (GameDifficultyPreset gameDifficultyPreset2 in GameData.DifficultyPresets)
                {
                    if (gameDifficultyPreset2.Parameters.Equals(difficultyGameParameters))
                    {
                        gameDifficultyPreset = gameDifficultyPreset2;
                        break;
                    }
                }
                UIStatDifficulty.Text = ((gameDifficultyPreset == null) ? "difficulty-preset.custom.title".T() : gameDifficultyPreset.Title);
                UIStatPlaytime.Text = StringFormatting.FormatDurationSeconds(float.Parse(lobby.GetData("playtime")));
                UIStatResearchProgress.Text = StringFormatting.FormatGeneralPercentage(math.floor(float.Parse(lobby.GetData("research")) * 100f) / 100f);
                UIStatStructureCount.Text = StringFormatting.FormatIntegerMax4Digits(int.Parse(lobby.GetData("structurecount")), false);
                UIStatGameRules.Text = StringFormatting.FormatGenericCount(int.Parse(lobby.GetData("gamerules")));
                SerializableSavegameModsContext mods = JsonUtility.FromJson<SerializableSavegameModsContext>(lobby.GetData("mods"));
                if (mods.ModSignatures.Count > 0)
                {
                    UIModsCount.Text = StringFormatting.FormatGenericCount(mods.ModSignatures.Count);
                    try
                    {
                        UIModList.Description = (IText)CreateModList.Invoke(null, new object[] { mods.ModSignatures });
                    } catch (Exception ex)
                    {
                        Logger.Exception?.LogException(ex);
                        UIModList.Description = new RawText("Error loading mod list");
                        return;
                    }
                } else
                {
                    UIModsCount.Text = StringFormatting.FormatGenericCount(0);
                }
            } catch (Exception ex)
            {
                Logger.Warning?.Log("Failed to read/render lobby data " + lobby.AdditionalTitle + ": " + ex.Message);
                this.SetUIToFailureState();
                return;
            }
            UILoadFailedOverlay.SetActiveSelfExt(false);
        }
        private void OnClickJoinButton()
        {
            if (GameEnvironment.IgnoreModsIncompatibility)
            {
                StartJoin();
                return;
            }
            SavegameModdingContextDivergenceBarrier.RequestConfirmationIfModContextDiverge(new SerializedSavegameMetadata() { ModContext = JsonUtility.FromJson<SerializableSavegameModsContext>(_Entry.GetData("mods")) }, new Action(StartJoin));
        }
        private void StartJoin()
        {
            if (Entry is SteamLobby steamLobby) MultiplayerCore.JoinLobby(steamLobby.Lobby);
            else if (Entry is DiscoveredServer discoveredServer) MultiplayerCore.DirectConnect(discoveredServer.Address);
        }
        private static readonly FieldInfo UIBtnResumeGameInfo = AccessTools.Field(typeof(HUDSavegameEntryPrefab), "UIBtnResumeGame");
        private static readonly FieldInfo UICompletedIndicatorInfo = AccessTools.Field(typeof(HUDSavegameEntryPrefab), "UICompletedIndicator");
        private static readonly FieldInfo UILoadFailedOverlayInfo = AccessTools.Field(typeof(HUDSavegameEntryPrefab), "UILoadFailedOverlay");
        private static readonly FieldInfo UIModListInfo = AccessTools.Field(typeof(HUDSavegameEntryPrefab), "UIModList");
        private static readonly FieldInfo UIModsCountInfo = AccessTools.Field(typeof(HUDSavegameEntryPrefab), "UIModsCount");
        private static readonly FieldInfo UINameTextInfo = AccessTools.Field(typeof(HUDSavegameEntryPrefab), "UINameText");
        private static readonly FieldInfo UISavegameUIDTextInfo = AccessTools.Field(typeof(HUDSavegameEntryPrefab), "UISavegameUIDText");
        private static readonly FieldInfo UIStatDifficultyInfo = AccessTools.Field(typeof(HUDSavegameEntryPrefab), "UIStatDifficulty");
        private static readonly FieldInfo UIStatGameRulesInfo = AccessTools.Field(typeof(HUDSavegameEntryPrefab), "UIStatGameRules");
        private static readonly FieldInfo UIStatModeInfo = AccessTools.Field(typeof(HUDSavegameEntryPrefab), "UIStatMode");
        private static readonly FieldInfo UIStatPlaytimeInfo = AccessTools.Field(typeof(HUDSavegameEntryPrefab), "UIStatPlaytime");
        private static readonly FieldInfo UIStatResearchProgressInfo = AccessTools.Field(typeof(HUDSavegameEntryPrefab), "UIStatResearchProgress");
        private static readonly FieldInfo UIStatScenarioInfo = AccessTools.Field(typeof(HUDSavegameEntryPrefab), "UIStatScenario");
        private static readonly FieldInfo UIStatStructureCountInfo = AccessTools.Field(typeof(HUDSavegameEntryPrefab), "UIStatStructureCount");
        private static readonly FieldInfo UIVersionMismatchOverlayInfo = AccessTools.Field(typeof(HUDSavegameEntryPrefab), "UIVersionMismatchOverlay");
        public void FromSavegameEntry(HUDSavegameEntryPrefab savegameEntry)
        {
            UIBtnJoinGame = (HUDButton)UIBtnResumeGameInfo.GetValue(savegameEntry);
            UIBtnJoinGame.Text = "menu.multiplayer.join".T();
            UIBtnJoinGame.OnClick.AddListener(new UnityAction(this.OnClickJoinButton));
            UICompletedIndicator = (GameObject)UICompletedIndicatorInfo.GetValue(savegameEntry);
            UILoadFailedOverlay = (GameObject)UILoadFailedOverlayInfo.GetValue(savegameEntry);
            UILoadFailedOverlay.GetComponentInChildren<HUDLocalizedText>().Text = "menu.multiplayer.session-load-fail".T();
            UIModList = (HUDTooltipTarget)UIModListInfo.GetValue(savegameEntry);
            UIModsCount = (HUDLocalizedText)UIModsCountInfo.GetValue(savegameEntry);
            UINameText = (HUDLocalizedText)UINameTextInfo.GetValue(savegameEntry);
            UISavegameUIDText = (HUDLocalizedText)UISavegameUIDTextInfo.GetValue(savegameEntry);
            UIStatDifficulty = (HUDLocalizedText)UIStatDifficultyInfo.GetValue(savegameEntry);
            UIStatGameRules = (HUDLocalizedText)UIStatGameRulesInfo.GetValue(savegameEntry);
            UIStatMode = (HUDLocalizedText)UIStatModeInfo.GetValue(savegameEntry);
            UIStatPlaytime = (HUDLocalizedText)UIStatPlaytimeInfo.GetValue(savegameEntry);
            UIStatResearchProgress = (HUDLocalizedText)UIStatResearchProgressInfo.GetValue(savegameEntry);
            UIStatScenario = (HUDLocalizedText)UIStatScenarioInfo.GetValue(savegameEntry);
            UIStatStructureCount = (HUDLocalizedText)UIStatStructureCountInfo.GetValue(savegameEntry);
            UIVersionMismatchOverlay = (GameObject)UIVersionMismatchOverlayInfo.GetValue(savegameEntry);
            List<HUDComponent> components = new List<HUDComponent>(savegameEntry.GetChildComponentReferences());
            for (int i = 0; i < transform.Find("Actions").childCount; i++)
            {
                var child = transform.Find("Actions").GetChild(i);
                components.Remove(child.GetComponent<HUDComponent>());
            }
            Destroy(transform.Find("Actions").gameObject);
            components.Remove(transform.Find("BtnDelete").GetComponent<HUDComponent>());
            Destroy(transform.Find("BtnDelete").gameObject);
            savegameEntry.SetChildComponentReferences(components.ToArray());
        }
        private void SetUIToFailureState()
        {
            this.UILoadFailedOverlay.SetActiveSelfExt(true);
            this.UIVersionMismatchOverlay.SetActiveSelfExt(false);
            this.UICompletedIndicator.SetActiveSelfExt(false);
            this.UIStatScenario.Text = null;
            this.UIStatMode.Text = null;
            this.UIStatDifficulty.Text = null;
            this.UIStatPlaytime.Text = null;
            this.UIStatResearchProgress.Text = null;
            this.UIStatStructureCount.Text = null;
            this.UIStatGameRules.Text = null;
            this.UIModsCount.Text = null;
        }
        private bool IsVerCompatible(GameVersion lobbyVersion, string gameModeId, string scenarioId)
        {
            return lobbyVersion >= Savegame.LowestSupportedVersion && lobbyVersion <= Savegame.CurrentVersion && GameData.GameModeIds.Contains(new Game.Core.Research.GameModeId(gameModeId)) && GameData.TryGetScenario(new ScenarioId(scenarioId), out _);
        }
    }
}
