using Core.Collections.Disposables;
using Core.Dependency;
using Core.Events;
using Core.Localization;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Core.Coordinates;
using Game.Core.Modding;
using Game.HUD.QuestArea.PinnedShapes;
using Game.Orchestration;
using Game.Placement.Data;
using HarmonyLib;
using Menu.MainMenu;
using Shapez2Multiplayer.Packets;
using Shapez2UILib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.Core.View;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
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
        public static ITickableOrchestrator CurrentSubOrchestrator => GameOrchestrator.CurrentSubOrchestrator;
        public static GameSessionOrchestrator? GameSessionOrchestrator => CurrentSubOrchestrator is GameSessionOrchestrator gameSessionOrchestrator ? gameSessionOrchestrator : null;
        public static MainMenuOrchestrator? MainMenuOrchestrator => CurrentSubOrchestrator is MainMenuOrchestrator mainMenuOrchestrator ? mainMenuOrchestrator : null;
        public static GameSessionOrchestrator? MainMenuOrchestratorBackgroundGameOrchestrator => CurrentSubOrchestrator is MainMenuOrchestrator mainMenuOrchestrator ? mainMenuOrchestrator.BackgroundGameOrchestrator : null;
        public static MainMenuStateManager? MainMenuOrchestratorStateManager => CurrentSubOrchestrator is MainMenuOrchestrator mainMenuOrchestrator ? mainMenuOrchestrator.StateManager : null;
        public static IHUDDialogStack? MainMenuOrchestratorDialogStack => MainMenuOrchestratorDependencyContainer?.Resolve<IHUDDialogStack>();
        public static IUISoundPlayer? MainMenuStateManagerUISoundPlayer => MainMenuOrchestratorDependencyContainer?.Resolve<IUISoundPlayer>();
        public static ILogger? MainMenuOrchestratorLogger => MainMenuOrchestratorDependencyContainer?.Resolve<ILogger>();
        public static IGameFlowNavigator? MainMenuOrchestratorFlowNavigator => CurrentSubOrchestrator is MainMenuOrchestrator mainMenuOrchestrator ? mainMenuOrchestrator.FlowNavigator : null;
        public static IAnalyticsTracker? MainMenuOrchestratorAnalyticsTracker => MainMenuOrchestratorDependencyContainer?.Resolve<IAnalyticsTracker>();
        public static Savegame? Savegame => GameSessionOrchestratorDependencyContainer?.Resolve<Savegame>();
        public static SavegameOptionsManager? SavegameOptionsManager => GameSessionOrchestratorDependencyContainer?.Resolve<SavegameOptionsManager>();
        public static GameMode? Mode => GameSessionOrchestratorDependencyContainer?.Resolve<GameMode>();
        public static ResearchManager? Research => GameSessionOrchestratorDependencyContainer?.Resolve<ResearchManager>();
        public static IMapModel? MapModel => GameSessionOrchestratorDependencyContainer?.Resolve<IMapModel>();
        public static SaveFileAccessor? FileAccessor => GameSessionOrchestrator?.FileAccessor;
        public static IEntityPlacementRunner? EntityPlacementRunner => GameSessionOrchestratorDependencyContainer?.Resolve<IEntityPlacementRunner>();
        public static PlayerActionManager? PlayerActions => GameSessionOrchestratorDependencyContainer?.Resolve<PlayerActionManager>();
        public static List<GameRule>? ActiveRules => Mode?.GameRules.ActiveRules;
        public static DependencyContainer? GameSessionOrchestratorDependencyContainer => GameSessionOrchestrator?.DependencyContainer;
        public static DependencyContainer? MainMenuOrchestratorDependencyContainer => MainMenuOrchestrator?.DependencyContainer;
        public static IGameFlowNavigator? GameFlowNavigator => GameSessionOrchestratorDependencyContainer?.Resolve<IGameFlowNavigator>();
        public static SimulationSpeedManager? SimulationSpeed => GameSessionOrchestratorDependencyContainer?.Resolve<SimulationSpeedManager>();
        public static IHUDDialogStack? DialogStack => GameSessionOrchestratorDependencyContainer?.Resolve<IHUDDialogStack>();
        public static EntityPlacementDrawer? EntityPlacementDrawer => GameSessionOrchestrator?.EntityPlacementDrawer;
        public static FrameDrawOptions? LegacyHudDrawOptions => GameSessionOrchestrator?.LegacyHudDrawOptions;
        public static IHubObserver? HubObserver => GameSessionOrchestratorDependencyContainer?.Resolve<IHubObserver>();
        public static VisualTheme? Theme => GameSessionOrchestratorDependencyContainer?.Resolve<VisualTheme>();
        public static IBuildingPlacementIndicatorAccessor? BuildingPlacementIndicators => GameSessionOrchestratorDependencyContainer?.Resolve<IBuildingPlacementIndicatorAccessor>();
        public static ITutorialHighlightProvider? TutorialHighlighProvider => GameSessionOrchestratorDependencyContainer?.Resolve<ITutorialHighlightProvider>();
        public static ILogger? GameSessionOrchestratorLogger => GameSessionOrchestratorDependencyContainer?.Resolve<ILogger>();
        public static HUD? HUD => GameSessionOrchestrator?.HUD;
        public static Transform? HUDRoot => HUD?.Root;
        public static GameInputManager? GameInputManager => GameSessionOrchestratorDependencyContainer?.Resolve<GameInputManager>();
        public static GameCursorManager? GameCursorManager => GameInputManager?.CursorManager;
        public static IInteractionMode? InteractionMode => GameSessionOrchestratorDependencyContainer?.Resolve<IInteractionMode>();
        public static IEventSender? PassiveEventBus => GameSessionOrchestratorDependencyContainer?.Resolve<IEventSender>();
        public static HUDEvents? HudEvents => GameSessionOrchestratorDependencyContainer?.Resolve<HUDEvents>();
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
            GameOrchestrator = GameBootstrapper.GameOrchestrator;
            Game = GameOrchestrator.Game;
            Application.quitting += Quitting;
            MainMenuUIRegistrar.RegisterUI<HUDMenuMultiplayerState>(BuildMultiplayerUI, "Multiplayer", MultiplayerButtonTranslation, MultiplayerButtonTranslationId, "menu.play.title", addMainPanel: false);
            UIHook.ElementHookUI<HUDMultiplayerPausePanel, HUDPauseMenu>(BuildMultiplayerPauseUI, "Multiplayer Pause Panel");
        }

        private void Quitting()
        {
            MultiplayerCore.Disconnect(canReturnToMenu: false);
        }
        public static float3 ComputeCursorWorldPosition()
        {
            if (GameSessionOrchestrator == null) return float3.zero;
            if (ScreenUtils.TryGetWorldCoordinate(GameSessionOrchestrator.Viewport, GameSessionOrchestrator.Viewport.CursorScreenPosition, out var worldPosition)) return (float3)worldPosition;
            return float3.zero;
        }
        public static float2 WorldToScreenPosition(float3 worldCoordinate)
        {
            if (GameSessionOrchestrator == null) return float2.zero;
            return (float2)ExtraScreenUtils.WorldToScreenPointDouble(GameSessionOrchestrator.Viewport, worldCoordinate);
        }
        public static SavegameModsContext? CreateModSignature(IEnumerable<ResolvedMod> mods)
        {
            return Savegame?.CreateModSignature(mods);
        }
        public static void WriteToStream(Stream outputStream, IReadOnlyDictionary<string, byte[]> serializedFileContents)
        {
            FileAccessor?.WriteToStream(outputStream, serializedFileContents);
        }
        public static Sequence? MainMenuOrchestratorFadeOut(bool useQuitVarient = false)
        {
            return MainMenuOrchestrator?.FadeOut(useQuitVarient);
        }
        public static IPlayerAction? CreatePlacementAction(IPlacementData placementData, IMapModel map, Player player, out bool anyValidPlacement)
        {
            anyValidPlacement = false;
            return ((EntityPlacementRunner?)EntityPlacementRunner)?.CreatePlacementAction(placementData, map, player, out anyValidPlacement);
        }
        public static IEnumerable<IPlacementDrawer> GetDrawers()
        {
            return BuiltinPlacementDrawers.GetDrawers(MapModel, Mode, HubObserver, Theme, BuildingPlacementIndicators, GameSessionOrchestratorDependencyContainer.Resolve<IIslandPreviewDrawer>(), TutorialHighlighProvider, GameSessionOrchestratorLogger);
        }
        public void Dispose()
        {
            MultiplayerCore.Disconnect(canReturnToMenu: false);
            harmony.UnpatchSelf();
        }
        public static void BuildMultiplayerUI(HUDMenuMultiplayerState hudMenuMultiplayerState)
        {
            HUDMenuPlayState hudMenuPlayState = hudMenuMultiplayerState.transform.parent.GetComponentInChildren<HUDMenuPlayState>(true);
            GameObject Play = hudMenuPlayState.gameObject;
            GameObject MainContent = GameObject.Instantiate(Play.transform.Find("MainContent").gameObject, hudMenuMultiplayerState.transform);
            MainContent.transform.localPosition = Play.transform.Find("MainContent").localPosition;
            Transform Content = MainContent.GetComponentInChildren<ScrollRect>().content;
            Content.GetChild(0).gameObject.name = "NoSessions";
            for (int i = 2; i < Content.childCount; i++)
            {
                GameObject.Destroy(Content.GetChild(i).gameObject);
            }
            GameObject Panel = GameObject.Instantiate(Play.transform.Find("Panel").gameObject, hudMenuMultiplayerState.transform);
            Panel.transform.localPosition = Play.transform.Find("Panel").localPosition;
            GameObject RefreshButton = Panel.transform.GetChild(1).gameObject;
            RefreshButton.name = "BtnRefresh";
            GameObject ImportButton = Panel.transform.GetChild(2).gameObject;
            RectTransform importButtonRect = ImportButton.GetComponent<RectTransform>();
            UISavegamePrefab = Play.GetComponent<HUDMenuPlayState>().UISavegamePrefab;
            HUDInputField InputField = UIFactory.AddInputField(Panel.transform, hudMenuMultiplayerState);
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
            hudMenuMultiplayerState.BtnRefresh = RefreshButton.GetComponent<HUDButton>();
            hudMenuMultiplayerState.Content = Content;
            hudMenuMultiplayerState.BtnDirectConnect = DirectConnectButton.GetComponent<HUDButton>();
            hudMenuMultiplayerState.DirectConnectInput = InputField;
            hudMenuMultiplayerState.SetChildComponentReferences(new HUDComponent[] { hudMenuMultiplayerState.transform.GetChild(0).GetComponent<HUDMenuBackButton>(), MainContent.GetComponentInChildren<HUDTranslucentImageWithCameraResultAsImageSource>(), MainContent.GetComponentInChildren<HUDScrollContainer>(), Panel.GetComponentInChildren<HUDTranslucentImageWithCameraResultAsImageSource>() }.Concat(panelComponents).ToArray());
            MainContent.GetComponentInChildren<HUDScrollContainer>().SetChildComponentReferences(new HUDComponent[] { Content.GetChild(0).GetComponent<HUDLocalizedText>(), Content.GetChild(1).GetComponent<HUDLocalizedText>() });
        }
        public static void BuildMultiplayerPauseUI(HUDMultiplayerPausePanel hudMultiplayerPausePanel)
        {
            RectTransform MultiplayerPausePanelRectTransform = hudMultiplayerPausePanel.GetComponent<RectTransform>();
            MultiplayerPausePanelRectTransform.anchorMin = new Vector2(1, 0);
            MultiplayerPausePanelRectTransform.anchorMax = new Vector2(1, 0);
            MultiplayerPausePanelRectTransform.offsetMin = new Vector2(-500, 150);
            MultiplayerPausePanelRectTransform.offsetMax = new Vector2(-20, 550);
            GameObject HUDPrimaryLightPanelMainMenu = UIFactory.AddPanel(hudMultiplayerPausePanel.transform, hudMultiplayerPausePanel);
            HUDButton HostButton = UIFactory.AddButton(hudMultiplayerPausePanel.transform, hudMultiplayerPausePanel);
            HostButton.name = "HostButton";
            RectTransform HostButtonRectTransform = HostButton.GetComponent<RectTransform>();
            HostButtonRectTransform.anchorMin = new Vector2(0, 0);
            HostButtonRectTransform.anchorMax = new Vector2(1, 0);
            HostButtonRectTransform.offsetMin = new Vector2(20, 100);
            HostButtonRectTransform.offsetMax = new Vector2(-20, 160);
            HUDButton InviteButton = UIFactory.AddButton(hudMultiplayerPausePanel.transform, hudMultiplayerPausePanel, secondary: true);
            InviteButton.name = "InviteButton";
            RectTransform InviteButtonRectTransform = InviteButton.GetComponent<RectTransform>();
            InviteButtonRectTransform.anchorMin = new Vector2(0, 0);
            InviteButtonRectTransform.anchorMax = new Vector2(0.5f, 0);
            InviteButtonRectTransform.offsetMin = new Vector2(20, 20);
            InviteButtonRectTransform.offsetMax = new Vector2(-10, 80);
            HUDButton ReportIssueButton = UIFactory.AddButton(hudMultiplayerPausePanel.transform, hudMultiplayerPausePanel, secondary: true);
            ReportIssueButton.name = "ReportIssueButton";
            RectTransform ReportIssueButtonRectTransform = ReportIssueButton.GetComponent<RectTransform>();
            ReportIssueButtonRectTransform.anchorMin = new Vector2(0.5f, 0);
            ReportIssueButtonRectTransform.anchorMax = new Vector2(1, 0);
            ReportIssueButtonRectTransform.offsetMin = new Vector2(10, 20);
            ReportIssueButtonRectTransform.offsetMax = new Vector2(-20, 80);
            HUDScrollContainer hudScrollContainer = UIFactory.AddScrollContainer(hudMultiplayerPausePanel.transform, hudMultiplayerPausePanel);
            UIFactory.AddDivider(hudScrollContainer.transform, false).name = "Divider Top";
            UIFactory.AddDivider(hudScrollContainer.transform, true).name = "Divider Bottom";
            RectTransform UIScrollContainerTransform = hudScrollContainer.GetComponent<RectTransform>();
            UIScrollContainerTransform.offsetMin = new Vector2(0, 180);
        }
        public static readonly string MultiplayerButtonTranslationId = "menu.multiplayer.title";
        public static readonly IText MultiplayerButtonTranslation = MultiplayerButtonTranslationId.T();
        public static readonly IText MultiplayerRefreshTranslation = "menu.multiplayer.refresh".T();
        public static readonly IText MultiplayerDirectConnectTranslation = "menu.multiplayer.directconnect".T();
        public static readonly IText MultiplayerIpAddressTranslation = "menu.multiplayer.ipaddress".T();
        [HarmonyPatch(typeof(HUDPauseMenu), "Construct")]
        [HarmonyPrefix]
        public static void HUDPauseMenuConstructPrefix(HUDMenuButton ___UISaveBtn)
        {
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
                YetToRecieveSavegame.Clear();
            }
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
        public static bool TryScheduleActionPrefix(IPlayerAction action, ref bool __result)
        {
            if (!MultiplayerCore.Client) return true;
            if (!(action is LevelUpLinearUpgradePlayerAction || action is ResearchUpgradePlayerAction)) return true;
            MultiplayerCore.connectionManager.Send(new PlayerActionPacket(action));
            __result = false;
            return false;
        }
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
        [HarmonyPatch(typeof(HUDPauseMenu), "TryLeaveToDesktop")]
        [HarmonyPrefix]
        public static bool HUDPauseMenuTryLeaveToDesktopPrefix(HUDPauseMenu __instance)
        {
            if (!MultiplayerCore.Client) return true;
            __instance.StartReturnToDesktop();
            return false;
        }
        [HarmonyPatch(typeof(HUDPauseMenu), "TryLeaveToMenu")]
        [HarmonyPrefix]
        public static bool HUDPauseMenuTryLeaveToMenuPrefix(HUDPauseMenu __instance)
        {
            if (!MultiplayerCore.Client) return true;
            __instance.StartReturnToMenu();
            return false;
        }
        public static readonly List<IPlayerAction> WaitingActions = new List<IPlayerAction>();
        [HarmonyPatch(typeof(PlayerActionManager), nameof(PlayerActionManager.ExecuteActionImmediate))]
        [HarmonyPrefix]
        public static bool PlayerActionManagerExecuteActionImmediatePrefix(PlayerActionManager __instance, IPlayerAction action)
        {
            if (!WaitingActions.Contains(action)) return true;
            __instance.ExecuteActionImmediately_INTERNAL(action, out var _);
            WaitingActions.Remove(action);
            return false;
        }
        public static IPlayerAction? LastActionOnUndoStack;
        [HarmonyPatch(typeof(PlayerActionManager), "Undo")]
        [HarmonyPrefix]
        public static void PlayerActionManagerUndoPrefix(PlayerActionManager __instance, List<IPlayerAction> ___UndoStack)
        {
            if (!__instance.HasActionsOnUndoStack)
            {
                LastActionOnUndoStack = null;
                return;
            }
            LastActionOnUndoStack = ___UndoStack[^1];
            if (___UndoStack[^1] is ActionModifyIsland actionModifyIsland)
            {
                MultiplayerCore.SendToAll(new PlayerActionPacket(new ActionModifyIsland(actionModifyIsland.Map, actionModifyIsland.Executor, new ActionModifyIsland.Payload(actionModifyIsland.Data.Delete, actionModifyIsland.Data.IgnorePlacementBlueprintCost, actionModifyIsland.Data.RefundDeletionBlueprintCost))));
            }
            else if (___UndoStack[^1] is ActionModifyBuildings actionModifyBuildings)
            {
                MultiplayerCore.SendToAll(new PlayerActionPacket(new ActionModifyBuildings(actionModifyBuildings.Map, actionModifyBuildings.Executor, new ModifyBuildingsPayload(Array.Empty<PlaceBuildingPayload>(), actionModifyBuildings.Data.Delete, actionModifyBuildings.Data.BlueprintCurrencyModification), actionModifyBuildings.UseBunchEditMode)));
            }
            else if (___UndoStack[^1] is CombinedUndoablePlayerAction combinedUndoablePlayerAction)
            {
                MultiplayerCore.SendToAll(new PlayerActionPacket(new CombinedUndoablePlayerAction(combinedUndoablePlayerAction.Actions.Select(action =>
                {
                    if (action is ActionModifyIsland actionModifyIsland1)
                    {
                        return new ActionModifyIsland(actionModifyIsland1.Map, actionModifyIsland1.Executor, new ActionModifyIsland.Payload(actionModifyIsland1.Data.Delete, actionModifyIsland1.Data.IgnorePlacementBlueprintCost, actionModifyIsland1.Data.RefundDeletionBlueprintCost));
                    }
                    else if (action is ActionModifyBuildings actionModifyBuildings1)
                    {
                        return new ActionModifyBuildings(actionModifyBuildings1.Map, actionModifyBuildings1.Executor, new ModifyBuildingsPayload(Array.Empty<PlaceBuildingPayload>(), actionModifyBuildings1.Data.Delete, actionModifyBuildings1.Data.BlueprintCurrencyModification), actionModifyBuildings1.UseBunchEditMode);
                    }
                    else
                    {
                        return action;
                    }
                }))));
            }
            else
            {
                MultiplayerCore.SendToAll(new PlayerActionPacket(___UndoStack[^1]));
            }
        }
        [HarmonyPatch(typeof(PlayerActionManager), "Undo")]
        [HarmonyPostfix]
        public static void PlayerActionManagerUndoPostfix(/*List<IPlayerAction> ___UndoStack*/)
        {
            //if (LastActionOnUndoStack != null && (___UndoStack.Count == 0 || ___UndoStack[^1] != LastActionOnUndoStack))
            //{
            //    MultiplayerCore.SendToAll(new PlayerActionPacket(LastActionOnUndoStack));
            //}
            if (LastActionOnUndoStack != null)
            {
                if (LastActionOnUndoStack is ActionModifyIsland actionModifyIsland)
                {
                    MultiplayerCore.SendToAll(new PlayerActionPacket(new ActionModifyIsland(actionModifyIsland.Map, actionModifyIsland.Executor, new ActionModifyIsland.Payload(actionModifyIsland.Data.Place, actionModifyIsland.Data.IgnorePlacementBlueprintCost, actionModifyIsland.Data.RefundDeletionBlueprintCost))));
                }
                else if (LastActionOnUndoStack is ActionModifyBuildings actionModifyBuildings)
                {
                    MultiplayerCore.SendToAll(new PlayerActionPacket(new ActionModifyBuildings(actionModifyBuildings.Map, actionModifyBuildings.Executor, new ModifyBuildingsPayload(actionModifyBuildings.Data.Place), actionModifyBuildings.UseBunchEditMode)));
                }
                else if (LastActionOnUndoStack is CombinedUndoablePlayerAction combinedUndoablePlayerAction)
                {
                    MultiplayerCore.SendToAll(new PlayerActionPacket(new CombinedUndoablePlayerAction(combinedUndoablePlayerAction.Actions.Select(action =>
                    {
                        if (action is ActionModifyIsland actionModifyIsland1)
                        {
                            return new ActionModifyIsland(actionModifyIsland1.Map, actionModifyIsland1.Executor, new ActionModifyIsland.Payload(actionModifyIsland1.Data.Place, actionModifyIsland1.Data.IgnorePlacementBlueprintCost, actionModifyIsland1.Data.RefundDeletionBlueprintCost));
                        }
                        else if (action is ActionModifyBuildings actionModifyBuildings1)
                        {
                            return new ActionModifyBuildings(actionModifyBuildings1.Map, actionModifyBuildings1.Executor, new ModifyBuildingsPayload(actionModifyBuildings1.Data.Place), actionModifyBuildings1.UseBunchEditMode);
                        }
                        else
                        {
                            return action;
                        }
                    }))));
                }
                else
                {
                    MultiplayerCore.SendToAll(new PlayerActionPacket(LastActionOnUndoStack));
                }
            }
            LastActionOnUndoStack = null;

        }
        public static IPlayerAction? LastActionOnRedoStack;
        [HarmonyPatch(typeof(PlayerActionManager), "Redo")]
        [HarmonyPrefix]
        public static void PlayerActionManagerRedoPrefix(PlayerActionManager __instance, List<IPlayerAction> ___RedoStack)
        {
            if (!__instance.HasActionsOnRedoStack)
            {
                LastActionOnRedoStack = null;
                return;
            }
            LastActionOnRedoStack = ___RedoStack[0];
            if (___RedoStack[0] is ActionModifyIsland actionModifyIsland)
            {
                MultiplayerCore.SendToAll(new PlayerActionPacket(new ActionModifyIsland(actionModifyIsland.Map, actionModifyIsland.Executor, new ActionModifyIsland.Payload(actionModifyIsland.Data.Delete, actionModifyIsland.Data.IgnorePlacementBlueprintCost, actionModifyIsland.Data.RefundDeletionBlueprintCost))));
            }
            else if (___RedoStack[0] is ActionModifyBuildings actionModifyBuildings)
            {
                MultiplayerCore.SendToAll(new PlayerActionPacket(new ActionModifyBuildings(actionModifyBuildings.Map, actionModifyBuildings.Executor, new ModifyBuildingsPayload(Array.Empty<PlaceBuildingPayload>(), actionModifyBuildings.Data.Delete, actionModifyBuildings.Data.BlueprintCurrencyModification), actionModifyBuildings.UseBunchEditMode)));
            }
            else if (___RedoStack[0] is CombinedUndoablePlayerAction combinedUndoablePlayerAction)
            {
                MultiplayerCore.SendToAll(new PlayerActionPacket(new CombinedUndoablePlayerAction(combinedUndoablePlayerAction.Actions.Select(action =>
                {
                    if (action is ActionModifyIsland actionModifyIsland1)
                    {
                        return new ActionModifyIsland(actionModifyIsland1.Map, actionModifyIsland1.Executor, new ActionModifyIsland.Payload(actionModifyIsland1.Data.Delete, actionModifyIsland1.Data.IgnorePlacementBlueprintCost, actionModifyIsland1.Data.RefundDeletionBlueprintCost));
                    }
                    else if (action is ActionModifyBuildings actionModifyBuildings1)
                    {
                        return new ActionModifyBuildings(actionModifyBuildings1.Map, actionModifyBuildings1.Executor, new ModifyBuildingsPayload(Array.Empty<PlaceBuildingPayload>(), actionModifyBuildings1.Data.Delete, actionModifyBuildings1.Data.BlueprintCurrencyModification), actionModifyBuildings1.UseBunchEditMode);
                    }
                    else
                    {
                        return action;
                    }
                }))));
            }
            else
            {
                MultiplayerCore.SendToAll(new PlayerActionPacket(___RedoStack[0]));
            }
        }
        [HarmonyPatch(typeof(PlayerActionManager), "Redo")]
        [HarmonyPostfix]
        public static void PlayerActionManagerRedoPostfix(List<IPlayerAction> ___RedoStack)
        {
            //if (LastActionOnRedoStack != null && (___RedoStack.Count == 0 || ___RedoStack[0] != LastActionOnRedoStack))
            //{
            //    MultiplayerCore.SendToAll(new PlayerActionPacket(LastActionOnRedoStack));
            //}
            if (LastActionOnRedoStack != null)
            {
                if (LastActionOnRedoStack is ActionModifyIsland actionModifyIsland)
                {
                    MultiplayerCore.SendToAll(new PlayerActionPacket(new ActionModifyIsland(actionModifyIsland.Map, actionModifyIsland.Executor, new ActionModifyIsland.Payload(actionModifyIsland.Data.Place, actionModifyIsland.Data.IgnorePlacementBlueprintCost, actionModifyIsland.Data.RefundDeletionBlueprintCost))));
                }
                else if (LastActionOnRedoStack is ActionModifyBuildings actionModifyBuildings)
                {
                    MultiplayerCore.SendToAll(new PlayerActionPacket(new ActionModifyBuildings(actionModifyBuildings.Map, actionModifyBuildings.Executor, new ModifyBuildingsPayload(actionModifyBuildings.Data.Place), actionModifyBuildings.UseBunchEditMode)));
                }
                else if (LastActionOnRedoStack is CombinedUndoablePlayerAction combinedUndoablePlayerAction)
                {
                    MultiplayerCore.SendToAll(new PlayerActionPacket(new CombinedUndoablePlayerAction(combinedUndoablePlayerAction.Actions.Select(action =>
                    {
                        if (action is ActionModifyIsland actionModifyIsland1)
                        {
                            return new ActionModifyIsland(actionModifyIsland1.Map, actionModifyIsland1.Executor, new ActionModifyIsland.Payload(actionModifyIsland1.Data.Place, actionModifyIsland1.Data.IgnorePlacementBlueprintCost, actionModifyIsland1.Data.RefundDeletionBlueprintCost));
                        }
                        else if (action is ActionModifyBuildings actionModifyBuildings1)
                        {
                            return new ActionModifyBuildings(actionModifyBuildings1.Map, actionModifyBuildings1.Executor, new ModifyBuildingsPayload(actionModifyBuildings1.Data.Place), actionModifyBuildings1.UseBunchEditMode);
                        }
                        else
                        {
                            return action;
                        }
                    }))));
                }
                else
                {
                    MultiplayerCore.SendToAll(new PlayerActionPacket(LastActionOnRedoStack));
                }
            }
            LastActionOnRedoStack = null;
        }
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
                }
                else if (part is HUDIslandMassSelection hudIslandMassSelection)
                {
                    HUDIslandMassSelection = hudIslandMassSelection;
                }
            }
            GameObject hudMultiplayerMassSelectionsHostGameObject = new GameObject("HUDMultiplayerMassSelectionsHost");
            hudMultiplayerMassSelectionsHostGameObject.transform.SetParent(___Root);
            var hudMultiplayerMassSelectionHost = hudMultiplayerMassSelectionsHostGameObject.AddComponent<HUDMultiplayerMassSelectionsHost>();
            ___DependencyContainer.Inject(hudMultiplayerMassSelectionHost);
            ___Parts.Add(hudMultiplayerMassSelectionHost);
            GameObject hudMultiplayerCursorsGameObject = new GameObject("HUDMultiplayerCursors");
            hudMultiplayerCursorsGameObject.transform.SetParent(___Root);
            hudMultiplayerCursorsGameObject.transform.SetSiblingIndex(0);
            hudMultiplayerCursorsGameObject.transform.localScale = Vector3.one;
            hudMultiplayerCursorsGameObject.layer = LayerMask.NameToLayer("UI");
            var hudMultiplayerCursors = hudMultiplayerCursorsGameObject.AddComponent<HUDMultiplayerCursors>();
            ___DependencyContainer.Inject(hudMultiplayerCursors);
            ___Parts.Add(hudMultiplayerCursors);
        }
        [HarmonyPatch(typeof(HUD), nameof(HUD.Dispose))]
        [HarmonyPrefix]
        public static void HUDDisposePrefix(DisposableList<HUDPart> ___Parts)
        {
            List<HUDPart> toRemove = new List<HUDPart>();
            foreach (HUDPart part in ___Parts)
            {
                if (part is HUDMultiplayerMassSelectionsHost || part is HUDMultiplayerCursors)
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
        [HarmonyPatch(typeof(HUDDialog), "CanCloseWithEscape", MethodType.Getter)]
        [HarmonyPrefix]
        public static bool HUDDialogCanCloseWithEscapePrefix(HUDDialog __instance, ref bool __result, HUDDialogPrefabReferences ___UIReferences)
        {
            if (!(__instance is HUDDialogSimpleInfo hudDialogSimpleInfo)) return true;
            if (!(___UIReferences.UITitleText._Text is LazyLocalizedText lazyLocalizedText)) return true;
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
        [HarmonyPatch(typeof(HUDPinnedShapesManager), "UnpinCompletedNodes")]
        [HarmonyPrefix]
        public static bool HUDPinnedShapesManagerUnpinCompletedNodesPrefix()
        {
            return !MultiplayerCore.Client;
        }
        public static bool IgnorePinEvents = false;
        [HarmonyPatch(typeof(HUDIslandGridVisualization), "Draw")]
        [HarmonyPrefix]
        public static void HUDIslandGridVisualizationDrawPrefix(HUDIslandGridVisualization __instance, FrameDrawOptionsNoLOD options, float ___Alpha)
        {
            if (Shapez2Multiplayer.GameSessionOrchestrator == null) return;
            if (!__instance.AreIslandsUnlocked) return;
            InstancedMeshManager ui = options.Renderers.UI;
            IMeshReference planeMesh = GeometryHelpers.PlaneMesh;
            MaterialReference islandGridHelperMaterialCursor = options.Theme.BaseResources.IslandGridHelperMaterialCursor;
            foreach (var cursor in HUDMultiplayerCursors.Instance.Cursors)
            {
                //var screenPosition = (float2)ExtraScreenUtils.WorldToScreenPointDouble(Shapez2Multiplayer.GameSessionOrchestrator.Viewport, cursor.WorldPosition);
                //if (screenPosition.x < 0 || screenPosition.y < 0 || screenPosition.x > Screen.width || screenPosition.y > Screen.height) continue;
                //if (RaycastHelpers.TryGetCursorPointOnVirtualPlane(screenPosition, (double)options.Viewport.Height, options.Viewport.MainCamera, out var planePosition, out var enter))
                //{

                //}
                float3 offset = cursor.WorldPosition + new float3(0f, -4.01f, 0f);
                float3 target = new float3(190f * ___Alpha);
                ui.Add(planeMesh, islandGridHelperMaterialCursor, FastMatrix.TranslateScale(in offset, in target), ShadowToken.Off, ShadowToken.Off);
            }
        }
        [HarmonyPatch]
        public class ConstantSignalBuildingModuleDataProviderShowDialogDelegatePatch {
            public static MethodBase TargetMethod()
            {
                var type = AccessTools.FirstInner(typeof(ConstantSignalBuildingModuleDataProvider), t => t.GetDeclaredFields().Any(f => f.Name == "config" && f.FieldType == typeof(ConstantSignalConfiguration)));
                return type.FirstMethod(m => m.Name.StartsWith("<HUD_ShowConfigureDialog>"));
            }
            public static void Postfix(ConstantSignalConfiguration ___config)
            {
                if (!MultiplayerCore.InLobby) return;
                var building = Shapez2Multiplayer.MapModel.Buildings.Cast<BuildingModel?>().FirstOrDefault(b => b.HasValue && b.Value.Configuration is ConstantSignalConfiguration constantSignalConfiguration && constantSignalConfiguration == ___config);
                if (building.HasValue)
                {
                    MultiplayerCore.SendToAll(new UpdateBuildingConfigurationPacket(building.Value.Tile_G, building.Value.Configuration));
                }
            }
        }
    }
}
