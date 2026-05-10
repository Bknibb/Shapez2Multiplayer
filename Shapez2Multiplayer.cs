using Core.Collections.Disposables;
using Core.Dependency;
using Core.Localization;
using DG.Tweening;
using Game.Core.Modding;
using Game.HUD.QuestArea.PinnedShapes;
using Game.Orchestration;
using Game.Placement.Data;
using HarmonyLib;
using HarmonyLib.Tools;
using Menu.MainMenu;
using Shapez2Multiplayer.Packets;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using TMPro;
using Unity.Core.Prefabs;
using Unity.Core.View;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static System.Collections.Specialized.BitVector32;
using ILogger = Core.Logging.ILogger;

namespace Shapez2Multiplayer
{
    public class Shapez2Multiplayer : IMod
    {
        public static ILogger logger;
        private readonly Harmony harmony;
        public static GameObject dontDestroyObject;
        public static GameOrchestrator GameOrchestrator;
        public static Game.Orchestration.Game Game;
        public static PrefabViewReference<HUDSavegameEntryPrefab> UISavegamePrefab;
        private static readonly FieldInfo CurrentSubOrchestratorInfo = AccessTools.Field(typeof(GameOrchestrator), "CurrentSubOrchestrator");
        private static readonly FieldInfo SavegameInfo = AccessTools.Field(typeof(GameSessionOrchestrator), "Savegame");
        private static readonly FieldInfo SavegameOptionsManagerInfo = AccessTools.Field(typeof(GameSessionOrchestrator), "SavegameOptionsManager");
        private static readonly FieldInfo ModeInfo = AccessTools.Field(typeof(GameSessionOrchestrator), "Mode");
        private static readonly FieldInfo ResearchInfo = AccessTools.Field(typeof(GameSessionOrchestrator), "Research");
        private static readonly FieldInfo MapModelInfo = AccessTools.Field(typeof(GameSessionOrchestrator), "MapModel");
        private static readonly FieldInfo FileAccessorInfo = AccessTools.Field(typeof(GameSessionOrchestrator), "FileAccessor");
        private static readonly FieldInfo PlayerInteractionOrchestratorInfo = AccessTools.Field(typeof(GameSessionOrchestrator), "PlayerInteractionOrchestrator");
        private static readonly FieldInfo EntityPlacementRunnerInfo = AccessTools.Field("PlayerInteractionOrchestrator:EntityPlacementRunner");
        private static readonly MethodInfo CreatePlacementActionInfo = AccessTools.Method(typeof(EntityPlacementRunner), "CreatePlacementAction");
        private static readonly FieldInfo PlayerActionsInfo = AccessTools.Field(typeof(EntityPlacementRunner), "PlayerActions");
        private static readonly FieldInfo ActiveRulesInfo = AccessTools.Field(typeof(GameRuleManager), "ActiveRules");
        private static readonly MethodInfo CreateModSignatureInfo = AccessTools.Method(typeof(Savegame), "CreateModSignature");
        private static readonly MethodInfo WriteToStreamInfo = AccessTools.Method(typeof(SaveFileAccessor), "WriteToStream");
        private static readonly FieldInfo UISavegamePrefabInfo = AccessTools.Field(typeof(HUDMenuPlayState), "UISavegamePrefab");
        private static readonly FieldInfo UIDialogModifySavegame = AccessTools.Field(typeof(HUDSavegameEntryPrefab), "UIDialogModifySavegame");
        private static readonly FieldInfo GameSessionOrchestratorDependencyContainerInfo = AccessTools.Field(typeof(GameSessionOrchestrator), "DependencyContainer");
        private static readonly FieldInfo GameSessionOrchestratorGameFlowNavigatorInfo = AccessTools.Field(typeof(GameSessionOrchestrator), "GameFlowNavigator");
        private static readonly FieldInfo GameSessionOrchestratorSimulationSpeedInfo = AccessTools.Field(typeof(GameSessionOrchestrator), "SimulationSpeed");
        private static readonly FieldInfo GameSessionOrchestratorDialogStackInfo = AccessTools.Field(typeof(GameSessionOrchestrator), "DialogStack");
        private static readonly FieldInfo GameSessionOrchestratorEntityPlacementDrawerInfo = AccessTools.Field(typeof(GameSessionOrchestrator), "EntityPlacementDrawer");
        private static readonly FieldInfo GameSessionOrchestratorLegacyHudDrawOptionsInfo = AccessTools.Field(typeof(GameSessionOrchestrator), "LegacyHudDrawOptions");
        private static readonly FieldInfo GameSessionOrchestratorHubObserverInfo = AccessTools.Field(typeof(GameSessionOrchestrator), "HubObserver");
        private static readonly FieldInfo GameSessionOrchestratorThemeInfo = AccessTools.Field(typeof(GameSessionOrchestrator), "Theme");
        private static readonly FieldInfo GameSessionOrchestratorBuildingPlacementIndicatorsInfo = AccessTools.Field(typeof(GameSessionOrchestrator), "BuildingPlacementIndicators");
        private static readonly FieldInfo GameSessionOrchestratorTutorialInfo = AccessTools.Field(typeof(GameSessionOrchestrator), "Tutorial");
        private static readonly FieldInfo GameSessionOrchestratorLoggerInfo = AccessTools.Field(typeof(GameSessionOrchestrator), "Logger");
        private static readonly FieldInfo GameSessionOrchestratorHUDInfo = AccessTools.Field(typeof(GameSessionOrchestrator), "HUD");
        private static readonly FieldInfo HUDRootInfo = AccessTools.Field(typeof(HUD), "Root");
        private static readonly FieldInfo MainMenuOrchestratorBackgroundGameOrchestratorInfo = AccessTools.Field(typeof(MainMenuOrchestrator), "BackgroundGameOrchestrator");
        private static readonly FieldInfo MainMenuOrchestratorStateManagerInfo = AccessTools.Field(typeof(MainMenuOrchestrator), "StateManager");
        private static readonly FieldInfo HUDMainMenuStateDialogStackInfo = AccessTools.Field(typeof(HUDMainMenuState), "DialogStack");
        private static readonly FieldInfo MainMenuStateManagerUISoundPlayerInfo = AccessTools.Field(typeof(MainMenuStateManager), "UISoundPlayer");
        private static readonly MethodInfo MainMenuOrchestratorFadeOutInfo = AccessTools.Method(typeof(MainMenuOrchestrator), "FadeOut");
        private static readonly FieldInfo MainMenuOrchestratorLoggerInfo = AccessTools.Field(typeof(MainMenuOrchestrator), "Logger");
        private static readonly FieldInfo MainMenuOrchestratorFlowNavigatorInfo = AccessTools.Field(typeof(MainMenuOrchestrator), "FlowNavigator");
        private static readonly FieldInfo MainMenuOrchestratorAnalyticsTrackerInfo = AccessTools.Field(typeof(MainMenuOrchestrator), "AnalyticsTracker");
        public static readonly FieldInfo HUDLocalizedTextUITextInfo = AccessTools.Field(typeof(HUDLocalizedText), "UIText");
        public static readonly FieldInfo HUDButtonUITextInfo = AccessTools.Field(typeof(HUDButton), "UIText");
        public static readonly FieldInfo HUDButtonUIButtonInfo = AccessTools.Field(typeof(HUDButton), "UIButton");
        public static readonly FieldInfo HUDButtonUIMainGroupInfo = AccessTools.Field(typeof(HUDButton), "UIMainGroup");
        public static readonly FieldInfo HUDButtonUIHoverIndicatorGroupInfo = AccessTools.Field(typeof(HUDButton), "UIHoverIndicatorGroup");
        public static readonly FieldInfo HUDButtonUIMainTransformInfo = AccessTools.Field(typeof(HUDButton), "UIMainTransform");
        public static readonly MethodInfo BuiltinPlacementDrawersGetDrawersInfo = AccessTools.Method("BuiltinPlacementDrawers:GetDrawers", new Type[] { typeof(IMapModel), typeof(GameMode), typeof(IHubObserver), typeof(VisualTheme), typeof(IBuildingPlacementIndicatorAccessor), typeof(IIslandPreviewDrawer), typeof(ITutorialHighlightProvider), typeof(ILogger) });
        public static ITickableOrchestrator CurrentSubOrchestrator => (ITickableOrchestrator)CurrentSubOrchestratorInfo.GetValue(GameOrchestrator);
        public static GameSessionOrchestrator? GameSessionOrchestrator => CurrentSubOrchestrator is GameSessionOrchestrator gameSessionOrchestrator ? gameSessionOrchestrator : null;
        public static MainMenuOrchestrator? MainMenuOrchestrator => CurrentSubOrchestrator is MainMenuOrchestrator mainMenuOrchestrator ? mainMenuOrchestrator : null;
        public static GameSessionOrchestrator? MainMenuOrchestratorBackgroundGameOrchestrator => CurrentSubOrchestrator is MainMenuOrchestrator mainMenuOrchestrator ? (GameSessionOrchestrator)MainMenuOrchestratorBackgroundGameOrchestratorInfo.GetValue(mainMenuOrchestrator) : null;
        public static MainMenuStateManager? MainMenuOrchestratorStateManager => CurrentSubOrchestrator is MainMenuOrchestrator mainMenuOrchestrator ? (MainMenuStateManager)MainMenuOrchestratorStateManagerInfo.GetValue(mainMenuOrchestrator) : null;
        public static IHUDDialogStack? MainMenuOrchestratorDialogStack => CurrentSubOrchestrator is MainMenuOrchestrator mainMenuOrchestrator ? (IHUDDialogStack)HUDMainMenuStateDialogStackInfo.GetValue(((MainMenuStateManager)MainMenuOrchestratorStateManagerInfo.GetValue(mainMenuOrchestrator)).CurrentState) : null;
        public static IUISoundPlayer? MainMenuStateManagerUISoundPlayer => CurrentSubOrchestrator is MainMenuOrchestrator mainMenuOrchestrator ? (IUISoundPlayer)MainMenuStateManagerUISoundPlayerInfo.GetValue(MainMenuOrchestratorStateManagerInfo.GetValue(mainMenuOrchestrator)) : null;
        public static ILogger? MainMenuOrchestratorLogger => CurrentSubOrchestrator is MainMenuOrchestrator mainMenuOrchestrator ? (ILogger)MainMenuOrchestratorLoggerInfo.GetValue(mainMenuOrchestrator) : null;
        public static IGameFlowNavigator? MainMenuOrchestratorFlowNavigator => CurrentSubOrchestrator is MainMenuOrchestrator mainMenuOrchestrator ? (IGameFlowNavigator)MainMenuOrchestratorFlowNavigatorInfo.GetValue(mainMenuOrchestrator) : null;
        public static IAnalyticsTracker? MainMenuOrchestratorAnalyticsTracker => CurrentSubOrchestrator is MainMenuOrchestrator mainMenuOrchestrator ? (IAnalyticsTracker)MainMenuOrchestratorAnalyticsTrackerInfo.GetValue(mainMenuOrchestrator) : null;
        public static Savegame? Savegame => CurrentSubOrchestrator is GameSessionOrchestrator gameSessionOrchestrator ? (Savegame)SavegameInfo.GetValue(gameSessionOrchestrator) : null;
        public static SavegameOptionsManager? SavegameOptionsManager => CurrentSubOrchestrator is GameSessionOrchestrator gameSessionOrchestrator ? (SavegameOptionsManager)SavegameOptionsManagerInfo.GetValue(gameSessionOrchestrator) : null;
        public static GameMode? Mode => CurrentSubOrchestrator is GameSessionOrchestrator gameSessionOrchestrator ? (GameMode)ModeInfo.GetValue(gameSessionOrchestrator) : null;
        public static ResearchManager? Research => CurrentSubOrchestrator is GameSessionOrchestrator gameSessionOrchestrator ? (ResearchManager)ResearchInfo.GetValue(gameSessionOrchestrator) : null;
        public static MapModel? MapModel => CurrentSubOrchestrator is GameSessionOrchestrator gameSessionOrchestrator ? (MapModel)MapModelInfo.GetValue(gameSessionOrchestrator) : null;
        public static SaveFileAccessor? FileAccessor => CurrentSubOrchestrator is GameSessionOrchestrator gameSessionOrchestrator ? (SaveFileAccessor)FileAccessorInfo.GetValue(gameSessionOrchestrator) : null;
        public static EntityPlacementRunner? EntityPlacementRunner => CurrentSubOrchestrator is GameSessionOrchestrator gameSessionOrchestrator ? (EntityPlacementRunner)EntityPlacementRunnerInfo.GetValue(PlayerInteractionOrchestratorInfo.GetValue(gameSessionOrchestrator)) : null;
        public static PlayerActionManager? PlayerActions => CurrentSubOrchestrator is GameSessionOrchestrator gameSessionOrchestrator ? (PlayerActionManager)PlayerActionsInfo.GetValue(EntityPlacementRunnerInfo.GetValue(PlayerInteractionOrchestratorInfo.GetValue(gameSessionOrchestrator))) : null;
        public static List<GameRule>? ActiveRules => Mode != null ? (List<GameRule>)ActiveRulesInfo.GetValue(Mode.GameRules) : null;
        public static DependencyContainer? GameSessionOrchestratorDependencyContainer => CurrentSubOrchestrator is GameSessionOrchestrator gameSessionOrchestrator ? (DependencyContainer)GameSessionOrchestratorDependencyContainerInfo.GetValue(gameSessionOrchestrator) : null;
        public static IGameFlowNavigator? GameFlowNavigator => CurrentSubOrchestrator is GameSessionOrchestrator gameSessionOrchestrator ? (IGameFlowNavigator)GameSessionOrchestratorGameFlowNavigatorInfo.GetValue(gameSessionOrchestrator) : null;
        public static SimulationSpeedManager? SimulationSpeed => CurrentSubOrchestrator is GameSessionOrchestrator gameSessionOrchestrator ? (SimulationSpeedManager)GameSessionOrchestratorSimulationSpeedInfo.GetValue(gameSessionOrchestrator) : null;
        public static HUDDialogStack? DialogStack => CurrentSubOrchestrator is GameSessionOrchestrator gameSessionOrchestrator ? (HUDDialogStack)GameSessionOrchestratorDialogStackInfo.GetValue(gameSessionOrchestrator) : null;
        public static EntityPlacementDrawer? EntityPlacementDrawer => CurrentSubOrchestrator is GameSessionOrchestrator gameSessionOrchestrator ? (EntityPlacementDrawer)GameSessionOrchestratorEntityPlacementDrawerInfo.GetValue(gameSessionOrchestrator) : null;
        public static FrameDrawOptions? LegacyHudDrawOptions => CurrentSubOrchestrator is GameSessionOrchestrator gameSessionOrchestrator ? (FrameDrawOptions)GameSessionOrchestratorLegacyHudDrawOptionsInfo.GetValue(gameSessionOrchestrator) : null;
        public static HubObserver? HubObserver => CurrentSubOrchestrator is GameSessionOrchestrator gameSessionOrchestrator ? (HubObserver)GameSessionOrchestratorHubObserverInfo.GetValue(gameSessionOrchestrator) : null;
        public static VisualTheme? Theme => CurrentSubOrchestrator is GameSessionOrchestrator gameSessionOrchestrator ? (VisualTheme)GameSessionOrchestratorThemeInfo.GetValue(gameSessionOrchestrator) : null;
        public static BuildingPlacementIndicatorAccessor? BuildingPlacementIndicators => CurrentSubOrchestrator is GameSessionOrchestrator gameSessionOrchestrator ? (BuildingPlacementIndicatorAccessor)GameSessionOrchestratorBuildingPlacementIndicatorsInfo.GetValue(gameSessionOrchestrator) : null;
        public static TutorialManager? Tutorial => CurrentSubOrchestrator is GameSessionOrchestrator gameSessionOrchestrator ? (TutorialManager)GameSessionOrchestratorTutorialInfo.GetValue(gameSessionOrchestrator) : null;
        public static ILogger? GameSessionOrchestratorLogger => CurrentSubOrchestrator is GameSessionOrchestrator gameSessionOrchestrator ? (ILogger)GameSessionOrchestratorLoggerInfo.GetValue(gameSessionOrchestrator) : null;
        public static HUD? HUD => CurrentSubOrchestrator is GameSessionOrchestrator gameSessionOrchestrator ? (HUD)GameSessionOrchestratorHUDInfo.GetValue(gameSessionOrchestrator) : null;
        public static Transform? HUDRoot => CurrentSubOrchestrator is GameSessionOrchestrator gameSessionOrchestrator ? (Transform)HUDRootInfo.GetValue(GameSessionOrchestratorHUDInfo.GetValue(gameSessionOrchestrator)) : null;
        public static readonly Sprite HUDButtonBase = Resources.FindObjectsOfTypeAll<Sprite>().First(t => t.name == "HUDButtonBase");
        public static readonly Sprite HUDSecondaryButtonBase = Resources.FindObjectsOfTypeAll<Sprite>().First(t => t.name == "HUDSecondaryButtonBase");
        public static readonly Sprite HUDButtonHover = Resources.FindObjectsOfTypeAll<Sprite>().First(t => t.name == "HUDButtonHover");
        public static readonly Sprite HUDPrimaryLightPanelMask = Resources.FindObjectsOfTypeAll<Sprite>().First(t => t.name == "HUDPrimaryLightPanelMask");
        public static readonly Sprite HUDPrimaryLightPanel = Resources.FindObjectsOfTypeAll<Sprite>().First(t => t.name == "HUDPrimaryLightPanel");
        public static readonly Sprite HUDScrollbarPanelBg = Resources.FindObjectsOfTypeAll<Sprite>().First(t => t.name == "HUDScrollbarPanelBg");
        public static readonly Sprite HUDAntiAliasedHorizontalScrollDivider = Resources.FindObjectsOfTypeAll<Sprite>().First(t => t.name == "HUDAntiAliasedHorizontalScrollDivider");
        public static readonly Material DefaultTranslucent = Resources.FindObjectsOfTypeAll<Material>().First(m => m.name == "Default-Translucent");
        public static readonly Material UIAnimatedPanelMenuMaterial = Resources.FindObjectsOfTypeAll<Material>().First(m => m.name == "UI-AnimatedPanelMenuMaterial");
        public static readonly Material UISpriteWithMipMapBiasOverride = Resources.FindObjectsOfTypeAll<Material>().First(m => m.name == "UI-SpriteWithMipMapBiasOverride");
        public static readonly Material UIAnimatedButtonMaterial = Resources.FindObjectsOfTypeAll<Material>().First(m => m.name == "UI-AnimatedButtonMaterial");
        public static readonly TMP_FontAsset FontMediumSDF = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().First(m => m.name == "Font-Medium SDF");
        public static readonly TMP_FontAsset FontLightSDF = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().First(m => m.name == "Font-Light SDF");
        public static readonly FieldInfo HUDComponentChildren = AccessTools.Field(typeof(HUDComponent), "Children");
        public static readonly FieldInfo HUDComponentLoadedChildren = AccessTools.Field(typeof(HUDComponent), "LoadedChildren");
        public Shapez2Multiplayer(ILogger logger)
        {
            Shapez2Multiplayer.logger = logger;
            harmony = new Harmony("bknibb.Shapez2Multiplayer");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            harmony.PatchAll(typeof(Shapez2Multiplayer));
            dontDestroyObject = new GameObject("MultiplayerDontDestroyObject");
            GameObject.DontDestroyOnLoad(dontDestroyObject);
            dontDestroyObject.AddComponent<MultiplayerDontDestroyObject>();
            MultiplayerCore.Initialize();
            GameOrchestrator = (GameOrchestrator)AccessTools.Field(typeof(GameBootstrapper), "GameOrchestrator").GetValue(null);
            Game = (Game.Orchestration.Game)AccessTools.Field(typeof(GameOrchestrator), "Game").GetValue(GameOrchestrator);
            Application.quitting += Quitting;
        }

        private void Quitting()
        {
            MultiplayerCore.Disconnect(canReturnToMenu: false);
        }

        public static SavegameModsContext? CreateModSignature(IEnumerable<ResolvedMod> mods)
        {
            return Savegame != null ? (SavegameModsContext)CreateModSignatureInfo.Invoke(Savegame, new object[] { mods }) : null;
        }
        public static void WriteToStream(Stream outputStream, IReadOnlyDictionary<string, byte[]> serializedFileContents)
        {
            WriteToStreamInfo.Invoke(FileAccessor, new object[] { outputStream, serializedFileContents });
        }
        public static Sequence? MainMenuOrchestratorFadeOut(bool useQuitVarient = false)
        {
            return CurrentSubOrchestrator is MainMenuOrchestrator mainMenuOrchestrator ? (Sequence)MainMenuOrchestratorFadeOutInfo.Invoke(mainMenuOrchestrator, new object[] { useQuitVarient }) : null;
        }
        public static IPlayerAction? CreatePlacementAction(IPlacementData placementData, IMapModel map, Player player, out bool anyValidPlacement)
        {
            var args = new object[] { placementData, map, player, null };
            var entityPlacementRunner = EntityPlacementRunner;
            if (entityPlacementRunner == null)
            {
                anyValidPlacement = false;
                return null;
            }
            var result = (IPlayerAction)CreatePlacementActionInfo.Invoke(EntityPlacementRunner, args);
            anyValidPlacement = (bool)args[3];
            return result;
        }
        public static IEnumerable<IPlacementDrawer> GetDrawers(IMapModel map, GameMode mode, IHubObserver hubObserver, VisualTheme theme, IBuildingPlacementIndicatorAccessor buildingPlacementIndicators, IIslandPreviewDrawer islandPreviewDrawer, ITutorialHighlightProvider highlightProvider, ILogger logger)
        {
            return (IEnumerable<IPlacementDrawer>)BuiltinPlacementDrawersGetDrawersInfo.Invoke(null, new object[] { map, mode, hubObserver, theme, buildingPlacementIndicators, islandPreviewDrawer, highlightProvider, logger });
        }
        public static IEnumerable<IPlacementDrawer> GetDrawers()
        {
            return GetDrawers(MapModel, Mode, HubObserver, Theme, BuildingPlacementIndicators, GameSessionOrchestratorDependencyContainer.Resolve<IIslandPreviewDrawer>(), Tutorial, GameSessionOrchestratorLogger);
        }
        public void Dispose()
        {
            MultiplayerCore.Disconnect(canReturnToMenu: false);
            harmony.UnpatchSelf();
        }
        public static HUDMenuMultiplayerState multiplayerMenuState;
        public static readonly FieldInfo componentChildComponentReferences = AccessTools.Field(typeof(HUDComponent), "ChildComponentReferences");
        public static readonly MethodInfo componentAddChildViewInternal = AccessTools.Method(typeof(HUDComponent), "AddChildViewInternal");
        public static readonly FieldInfo componentDependencyResolver = AccessTools.Field(typeof(HUDComponent), "DependencyResolver");
        public static readonly FieldInfo tmpInputFieldRegexValue = AccessTools.Field(typeof(TMP_InputField), "m_RegexValue");
        public static HUDMenuMultiplayerState BuildMultiplayerUI(HUDMenuPlayState hudMenuPlayState)
        {
            GameObject Play = hudMenuPlayState.gameObject;
            GameObject Multiplayer = new GameObject("Multiplayer");
            Multiplayer.transform.SetParent(Play.transform.parent);
            Multiplayer.transform.localPosition = Play.transform.localPosition;
            Multiplayer.transform.localScale = Vector3.one;
            RectTransform fromRect = Play.GetComponent<RectTransform>();
            RectTransform toRect = Multiplayer.AddComponent<RectTransform>();
            toRect.anchorMin = fromRect.anchorMin;
            toRect.anchorMax = fromRect.anchorMax;
            toRect.pivot = fromRect.pivot;
            toRect.anchoredPosition = fromRect.anchoredPosition;
            toRect.sizeDelta = fromRect.sizeDelta;
            Multiplayer.layer = Play.layer;
            Multiplayer.AddComponent<Canvas>();
            Multiplayer.AddComponent<GraphicRaycaster>();
            GameObject BackButton = GameObject.Instantiate(Play.transform.Find("BackButton").gameObject, Multiplayer.transform);
            BackButton.transform.localPosition = Play.transform.Find("BackButton").localPosition;
            GameObject MainContent = GameObject.Instantiate(Play.transform.Find("MainContent").gameObject, Multiplayer.transform);
            MainContent.transform.localPosition = Play.transform.Find("MainContent").localPosition;
            Transform Content = MainContent.GetComponentInChildren<ScrollRect>().content;
            Content.GetChild(0).gameObject.name = "NoSessions";
            for (int i = 2; i < Content.childCount; i++)
            {
                GameObject.Destroy(Content.GetChild(i).gameObject);
            }
            GameObject Panel = GameObject.Instantiate(Play.transform.Find("Panel").gameObject, Multiplayer.transform);
            Panel.transform.localPosition = Play.transform.Find("Panel").localPosition;
            GameObject RefreshButton = Panel.transform.GetChild(1).gameObject;
            RefreshButton.name = "BtnRefresh";
            GameObject ImportButton = Panel.transform.GetChild(2).gameObject;
            RectTransform importButtonRect = ImportButton.GetComponent<RectTransform>();
            UISavegamePrefab = (PrefabViewReference<HUDSavegameEntryPrefab>)UISavegamePrefabInfo.GetValue(Play.GetComponent<HUDMenuPlayState>());
            //GameObject InputField = GameObject.Instantiate(((PrefabReference<HUDDialogConfigureScenario>)UIDialogModifySavegame.GetValue(UISavegamePrefab.Resolve())).Resolve().GetComponentInChildren<HUDInputField>().gameObject, Panel.transform);
            multiplayerMenuState = Multiplayer.AddComponent<HUDMenuMultiplayerState>();
            HUDInputField InputField = UIStuff.AddInputField(Panel.transform, multiplayerMenuState);
            InputField.transform.localScale = Vector3.one;
            InputField.gameObject.name = "DirectConnectInput";
            TMP_InputField tmp_InputField = InputField.GetComponent<TMP_InputField>();
            tmp_InputField.characterValidation = TMP_InputField.CharacterValidation.CustomValidator;
            tmp_InputField.contentType = TMP_InputField.ContentType.Custom;
            tmp_InputField.keyboardType = TouchScreenKeyboardType.DecimalPad;
            tmp_InputField.onValidateInput += (text, charIndex, addedChar) =>
            {
                if (char.IsDigit(addedChar) || addedChar == '.')
                {
                    return addedChar;
                }
                return '\0';
            };
            tmp_InputField.characterLimit = 15;
            RectTransform inputFieldRect = InputField.GetComponent<RectTransform>();
            inputFieldRect.pivot = importButtonRect.pivot;
            inputFieldRect.anchorMin = importButtonRect.anchorMin;
            inputFieldRect.anchorMax = importButtonRect.anchorMax;
            inputFieldRect.offsetMin = importButtonRect.offsetMin;
            inputFieldRect.offsetMax = importButtonRect.offsetMax;
            GameObject.DestroyImmediate(ImportButton);
            GameObject DirectConnectButton = Panel.transform.GetChild(2).gameObject;
            DirectConnectButton.name = "BtnDirectConnect";
            var panelComponents = new List<HUDComponent>();
            for (int i = 0; i < Panel.transform.childCount; i++)
            {
                var component = Panel.transform.GetChild(i).GetComponent<HUDComponent>();
                if (component != null)
                {
                    panelComponents.Add(component);
                }
            }
            multiplayerMenuState.UIBtnBack = BackButton.GetComponent<HUDMenuBackButton>();
            multiplayerMenuState.BtnRefresh = RefreshButton.GetComponent<HUDButton>();
            multiplayerMenuState.Content = Content;
            multiplayerMenuState.BtnDirectConnect = DirectConnectButton.GetComponent<HUDButton>();
            multiplayerMenuState.DirectConnectInput = InputField;
            componentChildComponentReferences.SetValue(multiplayerMenuState, new HUDComponent[] { BackButton.GetComponent<HUDMenuBackButton>(), MainContent.GetComponentInChildren<HUDTranslucentImageWithCameraResultAsImageSource>(), MainContent.GetComponentInChildren<HUDScrollContainer>(), Panel.GetComponentInChildren<HUDTranslucentImageWithCameraResultAsImageSource>() }.Concat(panelComponents).ToArray());
            componentChildComponentReferences.SetValue(BackButton.GetComponent<HUDMenuBackButton>(), new HUDComponent[] { BackButton.GetComponentInChildren<HUDLocalizedText>(), BackButton.GetComponentInChildren<HUDAnimatedRoundButton>() });
            componentChildComponentReferences.SetValue(MainContent.GetComponentInChildren<HUDScrollContainer>(), new HUDComponent[] { Content.GetChild(0).GetComponent<HUDLocalizedText>(), Content.GetChild(1).GetComponent<HUDLocalizedText>() });
            return multiplayerMenuState;
        }
        public static HUDMultiplayerPausePanel BuildMultiplayerPauseUI(HUDPauseMenu hudPauseMenu)
        {
            GameObject MultiplayerPausePanel = new GameObject("Multiplayer Pause Panel");
            MultiplayerPausePanel.transform.SetParent(hudPauseMenu.transform);
            MultiplayerPausePanel.transform.localScale = Vector3.one;
            RectTransform MultiplayerPausePanelRectTransform = MultiplayerPausePanel.AddComponent<RectTransform>();
            MultiplayerPausePanelRectTransform.anchorMin = new Vector2(1, 0);
            MultiplayerPausePanelRectTransform.anchorMax = new Vector2(1, 0);
            MultiplayerPausePanelRectTransform.offsetMin = new Vector2(-500, 150);
            MultiplayerPausePanelRectTransform.offsetMax = new Vector2(-20, 550);
            HUDMultiplayerPausePanel hudMultiplayerPausePanel = MultiplayerPausePanel.AddComponent<HUDMultiplayerPausePanel>();
            //GameObject HUDPrimaryLightPanelMainMenu = GameObject.Instantiate(hudPauseMenu.transform.parent.GetComponentInChildren<HUDIngameSettings>(true).GetComponentInChildren<HUDTranslucentImageWithCameraResultAsImageSource>(true).transform.parent.gameObject, MultiplayerPausePanel.transform);
            GameObject HUDPrimaryLightPanelMainMenu = UIStuff.AddPanel(MultiplayerPausePanel.transform, hudMultiplayerPausePanel);
            //GameObject HostButton = GameObject.Instantiate(hudPauseMenu.GetComponentInChildren<HUDFeedbackButton>().transform.GetChild(0).gameObject, MultiplayerPausePanel.transform);
            HUDButton HostButton = UIStuff.AddButton(MultiplayerPausePanel.transform, hudMultiplayerPausePanel);
            HostButton.name = "HostButton";
            RectTransform HostButtonRectTransform = HostButton.GetComponent<RectTransform>();
            //UnityEngine.Object.Destroy(HostButton.GetComponentInChildren<HUDTooltipTarget>());
            HostButtonRectTransform.anchorMin = new Vector2(0, 0);
            HostButtonRectTransform.anchorMax = new Vector2(1, 0);
            HostButtonRectTransform.offsetMin = new Vector2(20, 100);
            HostButtonRectTransform.offsetMax = new Vector2(-20, 160);
            HUDButton InviteButton = UIStuff.AddButton(MultiplayerPausePanel.transform, hudMultiplayerPausePanel, secondary: true);
            InviteButton.name = "InviteButton";
            RectTransform InviteButtonRectTransform = InviteButton.GetComponent<RectTransform>();
            InviteButtonRectTransform.anchorMin = new Vector2(0, 0);
            InviteButtonRectTransform.anchorMax = new Vector2(0.5f, 0);
            InviteButtonRectTransform.offsetMin = new Vector2(20, 20);
            InviteButtonRectTransform.offsetMax = new Vector2(-10, 80);
            HUDButton ReportIssueButton = UIStuff.AddButton(MultiplayerPausePanel.transform, hudMultiplayerPausePanel, secondary: true);
            ReportIssueButton.name = "ReportIssueButton";
            RectTransform ReportIssueButtonRectTransform = ReportIssueButton.GetComponent<RectTransform>();
            ReportIssueButtonRectTransform.anchorMin = new Vector2(0.5f, 0);
            ReportIssueButtonRectTransform.anchorMax = new Vector2(1, 0);
            ReportIssueButtonRectTransform.offsetMin = new Vector2(10, 20);
            ReportIssueButtonRectTransform.offsetMax = new Vector2(-20, 80);
            //GameObject hudScrollContainer = GameObject.Instantiate(hudPauseMenu.transform.parent.GetComponentInChildren<HUDIngameSettings>(true).GetComponentInChildren<HUDScrollContainer>(true).gameObject, MultiplayerPausePanel.transform);
            HUDScrollContainer hudScrollContainer = UIStuff.AddScrollContainer(MultiplayerPausePanel.transform, hudMultiplayerPausePanel);
            UIStuff.AddDivider(hudScrollContainer.transform, false).name = "Divider Top";
            UIStuff.AddDivider(hudScrollContainer.transform, true).name = "Divider Bottom";
            RectTransform UIScrollContainerTransform = hudScrollContainer.GetComponent<RectTransform>();
            UIScrollContainerTransform.offsetMin = new Vector2(0, 180);
            //componentChildComponentReferences.SetValue(hudMultiplayerPausePanel, new HUDComponent[] { HUDPrimaryLightPanelMainMenu.GetComponentInChildren<HUDTranslucentImageWithCameraResultAsImageSource>(), HostButton, hudScrollContainer.GetComponent<HUDScrollContainer>() });
            //componentChildComponentReferences.SetValue(HostButton.GetComponent<HUDButton>(), new HUDComponent[] { HostButton.GetComponentInChildren<HUDLocalizedText>() });
            return hudMultiplayerPausePanel;
        }
        private static readonly MethodInfo addMenuButtonMethod = AccessTools.Method(typeof(HUDMenuMainState), "AddMenuButton");
        public static readonly IText MultiplayerButtonTranslation = "menu.multiplayer.title".T();
        public static readonly IText MultiplayerRefreshTranslation = "menu.multiplayer.refresh".T();
        public static readonly IText MultiplayerDirectConnectTranslation = "menu.multiplayer.directconnect".T();
        public static readonly IText MultiplayerIpAddressTranslation = "menu.multiplayer.ipaddress".T();
        [HarmonyPatch(typeof(HUDMenuMainState), "AddMenuButton")]
        [HarmonyPostfix]
        public static void AddMenuButtonPostfix(IText text, UnityAction action, HUDMenuMainState __instance, IMainMenuStateControl ___Menu)
        {
            if (text is LazyLocalizedText lazyText && lazyText.Id.Id == "menu.play.title")
            {
                addMenuButtonMethod.Invoke(__instance, new object[] { MultiplayerButtonTranslation, new UnityAction(() => {
                    ___Menu.SwitchToState<HUDMenuMultiplayerState>(null);
                }) });
            }
        }
        [HarmonyPatch(typeof(HUDMainMenuUI), "Construct")]
        [HarmonyPrefix]
        public static void HUDMainMenuUIConstructPrefix(HUDMainMenuUI __instance, ref HUDComponent[] ___ChildComponentReferences)
        {
            var multiplayerUI = BuildMultiplayerUI(__instance.GetComponentInChildren<HUDMenuPlayState>(true));
            ___ChildComponentReferences = ___ChildComponentReferences.AddToArray(multiplayerUI);
            componentAddChildViewInternal.MakeGenericMethod(typeof(HUDMenuMultiplayerState)).Invoke(__instance, new object[] { multiplayerUI });
        }
        [HarmonyPatch(typeof(MainMenuOrchestrator), "Step_0_2_InitStates")]
        [HarmonyPrefix]
        public static void MainMenuOrchestratorInitStatesPrefix(MainMenuOrchestrator __instance, Dictionary<string, MainMenuStateManager.CameraState> ___UICameraStatesDict)
        {
            ___UICameraStatesDict.Add("Multiplayer", ___UICameraStatesDict["Play"]);
        }
        [HarmonyPatch(typeof(MainMenuOrchestrator), "Step_0_2_InitStates")]
        [HarmonyPostfix]
        public static void MainMenuOrchestratorInitStatesPostfix(MainMenuOrchestrator __instance, DependencyContainer ___DependencyContainer)
        {
            //___DependencyContainer.Inject(multiplayerMenuState);
        }
        [HarmonyPatch(typeof(HUDPauseMenu), "Construct")]
        [HarmonyPrefix]
        public static void HUDPauseMenuConstructPrefix(HUDPauseMenu __instance, ref HUDComponent[] ___ChildComponentReferences, HUDMenuButton ___UISaveBtn)
        {
            var multipalyerPauseUI = BuildMultiplayerPauseUI(__instance);
            ___ChildComponentReferences = ___ChildComponentReferences.AddToArray(multipalyerPauseUI);
            componentAddChildViewInternal.MakeGenericMethod(typeof(HUDMultiplayerPausePanel)).Invoke(__instance, new object[] { multipalyerPauseUI });
            if (MultiplayerCore.Client)
            {
                ___UISaveBtn.SetInteractable(false);
            }
        }
        [HarmonyPatch(typeof(GameSessionOrchestrator), "TrySaveCurrent")]
        [HarmonyPrefix]
        public static bool TrySaveCurrentPrefix()
        {
            return !MultiplayerCore.InLobby || MultiplayerCore.Hosting;
        }
        public static List<IConnection> YetToRecieveSavegame = new List<IConnection>();
        //private static readonly MethodInfo SerializeSavegame = AccessTools.Method(typeof(SavegameSerializer), nameof(SavegameSerializer.SerializeSavegame));
        //private static readonly MethodInfo SerializedSavegameInfo = AccessTools.Method(typeof(Shapez2Multiplayer), nameof(SerializedSavegame));
        //[HarmonyPatch(typeof(SavegameSerializer), nameof(SavegameSerializer.SerializeSavegame))]
        //[HarmonyTranspiler]
        //public static IEnumerable<CodeInstruction> SerializeSavegameTranspiler(IEnumerable<CodeInstruction> instructions)
        //{
        //    var codes = new List<CodeInstruction>(instructions);
        //    for (int i = 0; i < codes.Count; i++)
        //    {
        //        if (codes[i].opcode == OpCodes.Stloc_S && codes[i-1].Calls(SerializeSavegame)) {
        //            yield return codes[i];
        //            yield return new CodeInstruction(OpCodes.Ldarg_0);
        //            yield return new CodeInstruction(OpCodes.Ldfld, SavegameOptionsManagerInfo);
        //            yield return new CodeInstruction(OpCodes.Ldloc_S, codes[i].operand);
        //            yield return new CodeInstruction(OpCodes.Call, SerializedSavegameInfo);
        //            continue;
        //        }
        //        yield return codes[i];
        //    }
        //}
        //public static void SerializedSavegame(SavegameOptionsManager savegameOptionsManager, IReadOnlyDictionary<string, byte[]> data)
        //{
        //    if (YetToRecieveSavegame.Count > 0 && MultiplayerCore.Hosting)
        //    {
        //        MultiplayerCore.socketManager.SendTo(new SavegamePacket(data, savegameOptionsManager.Uid), YetToRecieveSavegame);
        //    }
        //    YetToRecieveSavegame.Clear();
        //}
        // potential transpiler version, but would need (net standard 2.1 version of harmonyx and atleast version 25.3.4 of monomod.runtinedetour)
        private static string cachedUid;
        [HarmonyPatch(typeof(SavegameManager), nameof(SavegameManager.BuildSavegameNameFromUid))]
        [HarmonyPostfix]
        public static void BuildSavegameNameFromUidPostfix(string uid)
        {
            cachedUid = uid;
        }
        [HarmonyPatch(typeof(SavegameSerializer), nameof(SavegameSerializer.SerializeSavegame))]
        [HarmonyPostfix]
        public static void SerializeSavegamePostfix(IReadOnlyDictionary<string, byte[]> __result)
        {
            if (YetToRecieveSavegame.Count > 0 && MultiplayerCore.Hosting)
            {
                logger.Info?.Log("Sending save to waiting players");
                MultiplayerCore.socketManager.SendTo(new SavegamePacket(__result, cachedUid), YetToRecieveSavegame);
            }
            YetToRecieveSavegame.Clear();
        }
        //public static bool InExecutePlacementAction = false;
        //public static IPlacementData? CurrentPlacement;
        //[HarmonyPatch(typeof(EntityPlacementRunner), "ExecutePlacementAction")]
        //[HarmonyPrefix]
        //public static void ExecutePlacementActionPrefix()
        //{
        //    InExecutePlacementAction = true;
        //}
        //[HarmonyPatch(typeof(EntityPlacementRunner), "ExecutePlacementAction")]
        //[HarmonyPostfix]
        //public static void ExecutePlacementActionPostfix()
        //{
        //    InExecutePlacementAction = false;
        //}
        //[HarmonyPatch(typeof(EntityPlacementRunner), "CreatePlacementAction")]
        //[HarmonyPostfix]
        //public static void CreatePlacementActionPostfix(IPlacementData placementData, IMapModel map, Player player, bool anyValidPlacement)
        //{
        //    if (anyValidPlacement && InExecutePlacementAction)
        //    {
        //        CurrentPlacement = placementData;
        //    } else
        //    {
        //        CurrentPlacement = null;
        //    }
        //}
        public static bool ActionDetection = true;
#if DEBUG
        public static IPlayerAction DebugLastAction;
#endif
        [HarmonyPatch(typeof(PlayerActionManager), nameof(PlayerActionManager.TryScheduleAction))]
        [HarmonyPostfix]
        public static void TryScheduleActionPostfix(IPlayerAction action, bool __result)
        {
            //if (__result && CurrentPlacement != null && InExecutePlacementAction)
            //{
            //    var packet = new PlacementPacket(CurrentPlacement);
            //    MultiplayerCore.SendToAll(packet);
            //}
            //CurrentPlacement = null;
            if (!__result) return;
            if (!ActionDetection) return;
#if DEBUG
            DebugLastAction = action;
#endif
            if ((action is LevelUpLinearUpgradePlayerAction || action is ResearchUpgradePlayerAction) && MultiplayerCore.Hosting) MultiplayerCore.socketManager.SendToAll(new SyncResearchManagerPacket(Research));
            MultiplayerCore.SendToAll(new PlayerActionPacket(action));
        }
        [HarmonyPatch(typeof(GameOrchestrator), "UnloadCurrentState")]
        [HarmonyPrefix]
        public static void GameOrchestratorUnloadCurrentStatePrefix()
        {
            if (!Shapez2Multiplayer.Game.IsGameInSession(out IGameStartOptions _) || !(Shapez2Multiplayer.CurrentSubOrchestrator is GameSessionOrchestrator)) return;
            MultiplayerCore.Disconnect(canReturnToMenu: false);
        }
        [HarmonyPatch(typeof(HUDPauseMenu), "StartReturnToDesktop")]
        [HarmonyPrefix]
        public static void HUDPauseMenuStartReturnToDesktopPrefix()
        {
            MultiplayerCore.Disconnect(canReturnToMenu: false);
        }
        public static readonly MethodInfo HUDPauseMenuStartReturnToDesktopInfo = AccessTools.Method(typeof(HUDPauseMenu), "StartReturnToDesktop");
        [HarmonyPatch(typeof(HUDPauseMenu), "TryLeaveToDesktop")]
        [HarmonyPrefix]
        public static bool HUDPauseMenuTryLeaveToDesktopPrefix(HUDPauseMenu __instance)
        {
            if (!MultiplayerCore.Client) return true;
            HUDPauseMenuStartReturnToDesktopInfo.Invoke(__instance, new object[] { });
            return false;
        }
        public static readonly MethodInfo HUDPauseMenuStartReturnToMenuInfo = AccessTools.Method(typeof(HUDPauseMenu), "StartReturnToMenu");
        [HarmonyPatch(typeof(HUDPauseMenu), "TryLeaveToMenu")]
        [HarmonyPrefix]
        public static bool HUDPauseMenuTryLeaveToMenuPrefix(HUDPauseMenu __instance)
        {
            if (!MultiplayerCore.Client) return true;
            HUDPauseMenuStartReturnToMenuInfo.Invoke(__instance, new object[] { });
            return false;
        }
        public static readonly List<IPlayerAction> WaitingActions = new List<IPlayerAction>();
        public static readonly MethodInfo PlayerActionManagerExecuteActionImmediately_INTERNALInfo = AccessTools.Method(typeof(PlayerActionManager), "ExecuteActionImmediately_INTERNAL");
        [HarmonyPatch(typeof(PlayerActionManager), nameof(PlayerActionManager.ExecuteActionImmediate))]
        [HarmonyPrefix]
        public static bool PlayerActionManagerExecuteActionImmediatePrefix(PlayerActionManager __instance, IPlayerAction action)
        {
            if (!WaitingActions.Contains(action)) return true;
            PlayerActionManagerExecuteActionImmediately_INTERNALInfo.Invoke(__instance, new object[] { action, null });
            WaitingActions.Remove(action);
            return false;
        }
        //public static IPlayerAction? LastActionOnUndoStack;
        [HarmonyPatch(typeof(PlayerActionManager), "Undo")]
        [HarmonyPrefix]
        public static void PlayerActionManagerUndoPrefix(PlayerActionManager __instance, List<IPlayerAction> ___UndoStack)
        {
            if (!__instance.HasActionsOnUndoStack)
            {
                //LastActionOnUndoStack = null;
                return;
            }
            //LastActionOnUndoStack = ___UndoStack[^1];
            MultiplayerCore.SendToAll(new PlayerActionPacket(___UndoStack[^1]));
        }
        //[HarmonyPatch(typeof(PlayerActionManager), "Undo")]
        //[HarmonyPostfix]
        //public static void PlayerActionManagerUndoPostfix(List<IPlayerAction> ___UndoStack)
        //{
        //    if (LastActionOnUndoStack != null && (___UndoStack.Count == 0 || ___UndoStack[^1] != LastActionOnUndoStack))
        //    {
        //        MultiplayerCore.SendToAll(new PlayerActionPacket(LastActionOnUndoStack));
        //    }
        //    LastActionOnUndoStack = null;
        //}
        //public static IPlayerAction? LastActionOnRedoStack;
        [HarmonyPatch(typeof(PlayerActionManager), "Redo")]
        [HarmonyPrefix]
        public static void PlayerActionManagerRedoPrefix(PlayerActionManager __instance, List<IPlayerAction> ___RedoStack)
        {
            if (!__instance.HasActionsOnRedoStack)
            {
                //LastActionOnRedoStack = null;
                return;
            }
            //LastActionOnRedoStack = ___RedoStack[0];
            MultiplayerCore.SendToAll(new PlayerActionPacket(___RedoStack[0]));
        }
        //[HarmonyPatch(typeof(PlayerActionManager), "Redo")]
        //[HarmonyPostfix]
        //public static void PlayerActionManagerRedoPostfix(List<IPlayerAction> ___RedoStack)
        //{
        //    if (LastActionOnRedoStack != null && (___RedoStack.Count == 0 || ___RedoStack[0] != LastActionOnRedoStack))
        //    {
        //        MultiplayerCore.SendToAll(new PlayerActionPacket(LastActionOnRedoStack));
        //    }
        //    LastActionOnRedoStack = null;
        //}
        public static bool BypassSimulationSpeedCheck = false;
        [HarmonyPatch(typeof(SimulationSpeedManager), "Speed", MethodType.Setter)]
        [HarmonyPrefix]
        public static bool SimulationSpeedManagerSpeedPrefix()
        {
            return !MultiplayerCore.InLobby || BypassSimulationSpeedCheck;
        }
        //[HarmonyPatch(typeof(GameOrchestrator), "LoadSession")]
        //[HarmonyPostfix]
        //public static void GameOrchestratorLoadSessionPostfix()
        //{
        //    if (!MultiplayerCore.Client) return;
        //    MultiplayerCore.connectionManager.Send(new FinishedConnectingPacket());
        //}
        [HarmonyPatch(typeof(SystemButtonsModel), nameof(SystemButtonsModel.OnGameUpdate))]
        [HarmonyPrefix]
        public static void SystemButtonsModelOnGameUpdatePrefix()
        {
            if (MultiplayerCore.connectionManager != null)
            {
                MultiplayerCore.connectionManager.HostDrawer?.Draw(Shapez2Multiplayer.LegacyHudDrawOptions, Shapez2Multiplayer.MapModel);
                foreach (var drawer in MultiplayerCore.connectionManager.PlayersDrawers.Values)
                {
                    if (drawer.HasData) drawer.Draw(Shapez2Multiplayer.LegacyHudDrawOptions, Shapez2Multiplayer.MapModel);
                }
            }
            if (MultiplayerCore.socketManager != null)
            {
                foreach (var drawer in MultiplayerCore.socketManager.PlayersDrawers.Values)
                {
                    if (drawer.HasData) drawer.Draw(Shapez2Multiplayer.LegacyHudDrawOptions, Shapez2Multiplayer.MapModel);
                }
            }
        }
        public static OtherPlayerEntityPlacementDrawer CreateOtherPlayerEntityPlacementDrawer()
        {
            return new OtherPlayerEntityPlacementDrawer(Shapez2Multiplayer.GetDrawers().Where(d => !(d is BuildingPlacementHubSlotDrawer || d is BuildingPlacementNotchSlotDrawer)), Shapez2Multiplayer.GameSessionOrchestratorLogger);
        }
        public static HUDBuildingMassSelection HUDBuildingMassSelection { get; private set; }
        public static HUDIslandMassSelection HUDIslandMassSelection { get; private set; }
        [HarmonyPatch(typeof(HUD), nameof(HUD.Initialize))]
        [HarmonyPostfix]
        public static void HUDInitializePostfix(Transform ___Root, DisposableList<HUDPart> ___Parts, DependencyContainer ___DependencyContainer)
        {
            foreach (var part in ___Parts)
            {
                if (part is HUDBuildingMassSelection hudBuildingMassSelection)
                {
                    HUDBuildingMassSelection = hudBuildingMassSelection;
                } else if (part is HUDIslandMassSelection hudIslandMassSelection)
                {
                    HUDIslandMassSelection = hudIslandMassSelection;
                }
            }
            GameObject hudMultiplayerMassSelectionsHostGameObject = new GameObject("HUDMultiplayerMassSelectionsHost");
            hudMultiplayerMassSelectionsHostGameObject.transform.SetParent(___Root);
            var hudMultiplayerMassSelectionHost = hudMultiplayerMassSelectionsHostGameObject.AddComponent<HUDMultiplayerMassSelectionsHost>();
            ___DependencyContainer.Inject(hudMultiplayerMassSelectionHost);
            ___Parts.Add(hudMultiplayerMassSelectionHost);
        }
        [HarmonyPatch(typeof(HUD), nameof(HUD.Dispose))]
        [HarmonyPrefix]
        public static void HUDDisposePrefix(DisposableList<HUDPart> ___Parts)
        {
            List<HUDPart> toRemove = new List<HUDPart>();
            foreach (HUDPart part in ___Parts)
            {
                if (part is HUDMultiplayerMassSelectionsHost)
                {
                    part.Dispose();
                    toRemove.Add(part);
                }
            }
            foreach (HUDPart part in toRemove) ___Parts.Remove(part);
        }
        [HarmonyPatch(typeof(HUDAutosave), nameof(HUDAutosave.OnGameUpdate))]
        [HarmonyPrefix]
        public static bool HUDAutosaveOnGameUpdatePrefix()
        {
            return !MultiplayerCore.Client;
        }
        public static readonly FieldInfo HUDLocalizedText_TextInfo = AccessTools.Field(typeof(HUDLocalizedText), "_Text");
        [HarmonyPatch(typeof(HUDDialog), "CanCloseWithEscape", MethodType.Getter)]
        [HarmonyPrefix]
        public static bool HUDDialogCanCloseWithEscapePrefix(HUDDialog __instance, ref bool __result, HUDDialogPrefabReferences ___UIReferences)
        {
            if (!(__instance is HUDDialogSimpleInfo hudDialogSimpleInfo)) return true;
            if (!((IText)HUDLocalizedText_TextInfo.GetValue(___UIReferences.UITitleText) is LazyLocalizedText lazyLocalizedText)) return true;
            if (lazyLocalizedText.Id.Id != "mutliplayer.paused-dialog.title") return true;
            __result = false;
            return false;
        }
        [HarmonyPatch(typeof(HUDPinnedShapesManager), "TryPinFollowupShapesAfterResearchComplete")]
        [HarmonyPrefix]
        public static bool HUDPinnedShapesManagerTryPinFollowupShapesAfterResearchCompletePrefix()
        {
            return !MultiplayerCore.Client;
        }
    }
}
