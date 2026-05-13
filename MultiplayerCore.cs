using Core.Localization;
using Cysharp.Threading.Tasks;
using ENet;
using Game.Core.Modding;
using Shapez2Multiplayer.Packets;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static Shapez2Multiplayer.MultiplayerCore;

namespace Shapez2Multiplayer
{
    public static class MultiplayerCore
    {
        public static ShapezSocketManager? socketManager;
        public static ShapezConnectionManager? connectionManager;
        public static Lobby? Lobby;
        public static bool InLobby => Lobby != null || socketManager != null || connectionManager != null;
        public static bool Hosting => socketManager != null;
        public static bool Client => connectionManager != null;
        public static uint CurrentUniversalId = 1;
        public static HUDDialogSimpleInfo? ConnectingDialog;
        public static void Initialize()
        {
            SteamFriends.OnGameLobbyJoinRequested += async (lobby, friend) =>
            {
                Shapez2Multiplayer.logger.Info?.Log("Received lobby join request from friend: " + friend + " for lobby: " + lobby.Id);
                JoinLobby(lobby);
            };
            ENet.Library.Initialize();
            SteamNetworkingUtils.SendBufferSize = 1024 * 1024;
        }
        public static async Task JoinLobby(Lobby lobby)
        {
            if (!CanJoin())
            {
                Shapez2Multiplayer.logger.Info?.Log("Cannot join lobby in this state");
                return;
            }
            ConnectingDialog = Shapez2Multiplayer.MainMenuOrchestratorDialogStack.Show<HUDDialogSimpleInfo>(Globals.Resources.UIDialogSimpleInfoPrefab);
            ConnectingDialog.Init("multiplayer.connecting-dialog.title".T(), "multiplayer.connecting-dialog.description".T(), "multiplayer.connecting-dialog.cancel".T());
            ConnectingDialog.OnClosed.Register(DialogClosed);
            var enter = await lobby.Join();
            if (enter == RoomEnter.Success)
            {
                Shapez2Multiplayer.logger.Info?.Log("Successfully joined lobby: " + lobby.Id);
                try
                {
                    if (ConnectingDialog == null)
                    {
                        lobby.Leave();
                        return;
                    }
                    Lobby = lobby;
                    connectionManager = new ShapezConnectionManager(SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(lobby.Owner.Id, 0));
                    connectionManager.Disconnected += reason => Disconnect(reason);
                }
                catch (Exception ex)
                {
                    ConnectingDialog.OnClosed.Unregister(DialogClosed);
                    ConnectingDialog.Close();
                    ConnectingDialog = null;
                    Shapez2Multiplayer.MainMenuOrchestratorDialogStack.Show<HUDDialogSimpleInfo>(Globals.Resources.UIDialogSimpleInfoPrefab).Init("multiplayer.failedtoconnect-dialog.title".T(), "multiplayer.failedtoconnect-dialog.description".T());
                    Shapez2Multiplayer.MainMenuStateManagerUISoundPlayer.PlayError();
                    Shapez2Multiplayer.logger.Error.Log("Failed to connect to server");
                    Shapez2Multiplayer.logger.Exception.LogException(ex);
                    lobby.Leave();
                    Lobby = null;
                }
            }
            else
            {
                ConnectingDialog.OnClosed.Unregister(DialogClosed);
                ConnectingDialog.Close();
                ConnectingDialog = null;
                Shapez2Multiplayer.logger.Error?.Log("Failed to join lobby: " + lobby.Id + " with error: " + enter);
                Shapez2Multiplayer.MainMenuOrchestratorDialogStack.Show<HUDDialogSimpleInfo>(Globals.Resources.UIDialogSimpleInfoPrefab).Init("multiplayer.failedtoconnect-dialog.title".T(), "multiplayer.failedtoconnect-dialog.description".T());
                Shapez2Multiplayer.MainMenuStateManagerUISoundPlayer.PlayError();
                Lobby = null;
            }
        }
        public static void DialogClosed()
        {
            ConnectingDialog = null;
            Disconnect(canReturnToMenu: false);
        }
        public static bool CanJoin()
        {
            return !InLobby && Shapez2Multiplayer.Game.IsGameInMainMenu();
        }
        public static async Task CreateLobby(bool steam = true, bool ENet = true)
        {
            if (!Shapez2Multiplayer.Game.IsGameInSession(out IGameStartOptions _) || !(Shapez2Multiplayer.CurrentSubOrchestrator is GameSessionOrchestrator GameSessionOrchestrator) || (InLobby && !Hosting))
            {
                return;
            }
            if (steam && !(Hosting && socketManager.SocketManagers.Any(s => s is SteamSocketManager)))
            {
                if (Hosting)
                {
                    socketManager.AddSocketManager(SteamNetworkingSockets.CreateRelaySocket<SteamSocketManager>(0));
                } else
                {
                    Shapez2Multiplayer.SimulationSpeed.Speed = 1f;
                    socketManager = new ShapezSocketManager(SteamNetworkingSockets.CreateRelaySocket<SteamSocketManager>(0));
                }
                var lobby = await SteamMatchmaking.CreateLobbyAsync();
                if (!lobby.HasValue)
                {
                    Shapez2Multiplayer.logger.Error?.Log("Failed to create lobby");
                    return;
                }
                Lobby = lobby;
                lobby.Value.SetFriendsOnly();
                lobby.Value.SetJoinable(true);
                try
                {
                    RefreshLobbyData();
                }
                catch (Exception ex)
                {
                    Shapez2Multiplayer.logger.Warning?.Log("Failed to set lobby data");
                    Shapez2Multiplayer.logger.Warning?.LogException(ex);
                }
                Shapez2Multiplayer.logger.Info?.Log("Lobby created with id: " + lobby.Value.Id);
                
            }
            if (ENet && !(Hosting && socketManager.SocketManagers.Any(s => s is ENetSocketManager)))
            {
                var server = new Host();
                server.Create(new Address() { Port = 7777 }, 100);
                if (Hosting)
                {
                    socketManager.AddSocketManager(new ENetSocketManager(server));
                }
                else
                {
                    Shapez2Multiplayer.SimulationSpeed.Speed = 1f;
                    socketManager = new ShapezSocketManager(new ENetSocketManager(server));
                }
                Shapez2Multiplayer.logger.Info?.Log("Started ENet server on port 7777");
            }
            if (Hosting)
            {
                Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer.HUDData.Pins.OnPinAdded.Register(MultiplayerEvents.OnPinAdded);
                Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer.HUDData.Pins.OnPinRemoved.Register(MultiplayerEvents.OnPinRemoved);
                ((IEntityPlacementStateController)Shapez2Multiplayer.EntityPlacementRunner).OnPlacementDataChanged.Register(MultiplayerEvents.OnPlacementDataChanged);
                Shapez2Multiplayer.Research.LinearUpgradeManager.OnChanged.Register(MultiplayerEvents.OnResearchLinearUpgradeManagerChanged);
                Shapez2Multiplayer.Research.PlayerLevel.OnLevelChanged.Register(MultiplayerEvents.OnResearchPlayerLevelManagerChanged);
                Shapez2Multiplayer.Research.PlayerLevelGoals.OnChanged.Register(MultiplayerEvents.OnResearchPlayerLevelGoalManagerChanged);
                Shapez2Multiplayer.Research.Progress.OnChanged.Register(MultiplayerEvents.OnResearchUnlockProgressManagerChanged);
                Shapez2Multiplayer.Research.UnlockManager.OnResearchManuallyUnlockedByPlayer.Register(MultiplayerEvents.OnResearchUnlockManagerResearchManuallyUnlockedByPlayer);
            }
        }
        public static void RefreshLobbyData()
        {
            if (!Lobby.HasValue) return;
            Lobby.Value.SetData("name", Shapez2Multiplayer.Savegame.Name);
            Lobby.Value.SetData("gamever", ((int)Savegame.CurrentVersion).ToString());
            Lobby.Value.SetData("appsourceversion", GameEnvironmentManager.Version);
            Lobby.Value.SetData("appsourceenvironment", GameEnvironment.BuildConfiguration.ToString());
            Lobby.Value.SetData("appsourcestore", GameEnvironment.Store.ToString());
            IModdingFramework moddingFramework = Shapez2Multiplayer.GameSessionOrchestratorDependencyContainer.Resolve<IModdingFramework>();
            Lobby.Value.SetData("modver", moddingFramework.Context.ExecutableMods.FirstOrDefault(mod => mod.EntryPoint is Shapez2Multiplayer).Metadata.Version.ToString());
            Lobby.Value.SetData("mode", Shapez2Multiplayer.Mode.BaseId.Id);
            Lobby.Value.SetData("scenario", Shapez2Multiplayer.Mode.Scenario.UniqueId.Id);
            Lobby.Value.SetData("difficultyresearchshapecost", Shapez2Multiplayer.Mode.Parameters.DifficultyParameters.ResearchShapeCostMultiplier.ToString());
            Lobby.Value.SetData("difficultychunklimit", Shapez2Multiplayer.Mode.Parameters.DifficultyParameters.ChunkLimitMultiplier.ToString());
            Lobby.Value.SetData("difficultyblueprintcost", Shapez2Multiplayer.Mode.Parameters.DifficultyParameters.BlueprintCostMultiplier.ToString());
            Lobby.Value.SetData("playtime", Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer.TotalPlaytime.ToString());
            Lobby.Value.SetData("research", Shapez2Multiplayer.Research.Progress.ComputeProgress().ToString());
            Lobby.Value.SetData("structurecount", Shapez2Multiplayer.MapModel.BuildingCount.ToString());
            Lobby.Value.SetData("gamerules", Shapez2Multiplayer.ActiveRules.Count.ToString());
            Lobby.Value.SetData("mods", JsonUtility.ToJson(Savegame.SerializeModSignature(Shapez2Multiplayer.CreateModSignature(moddingFramework.Context.ResolvedMods), moddingFramework)));
            Lobby.Value.SetData("cheatsused", Shapez2Multiplayer.Savegame.CheatsEnabled.ToString());
            Lobby.Value.SetData("completed", Shapez2Multiplayer.Research.Layout.Levels.All(l => Shapez2Multiplayer.Research.Progress.IsUnlocked(l)).ToString());
            Lobby.Value.SetData("uid", Shapez2Multiplayer.SavegameOptionsManager.Uid);
        }
        public static bool SendToAll(IPacket packet)
        {
            return (socketManager?.SendToAll(packet) ?? false) || ((connectionManager?.SendToAll(packet)) ?? false);
        }
        public static async Task DirectConnect(string addressStr)
        {
            if (!CanJoin()) return;
            ConnectingDialog = Shapez2Multiplayer.MainMenuOrchestratorDialogStack.Show<HUDDialogSimpleInfo>(Globals.Resources.UIDialogSimpleInfoPrefab);
            ConnectingDialog.Init("multiplayer.connecting-dialog.title".T(), "multiplayer.connecting-dialog.description".T(), "multiplayer.connecting-dialog.cancel".T());
            ConnectingDialog.OnClosed.Register(DialogClosed);
            var client = new Host();
            var address = new Address() { Port = 7777 };
            address.SetIP(addressStr);
            client.Create();
            Peer peer = client.Connect(address);
            connectionManager = new ShapezConnectionManager(new ENetConnectionManager(client, peer));
            connectionManager.Disconnected += reason => Disconnect(reason);
        }
        public static void Disconnect(DisconnectReason disconnectReason = DisconnectReason.None, bool canReturnToMenu = true)
        {
            try
            {
                if (socketManager != null)
                {
                    Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer.HUDData.Pins.OnPinAdded.Unregister(MultiplayerEvents.OnPinAdded);
                    Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer.HUDData.Pins.OnPinRemoved.Unregister(MultiplayerEvents.OnPinRemoved);
                    ((IEntityPlacementStateController)Shapez2Multiplayer.EntityPlacementRunner).OnPlacementDataChanged.Unregister(MultiplayerEvents.OnPlacementDataChanged);
                    Shapez2Multiplayer.Research.LinearUpgradeManager.OnChanged.Unregister(MultiplayerEvents.OnResearchLinearUpgradeManagerChanged);
                    Shapez2Multiplayer.Research.PlayerLevel.OnLevelChanged.Unregister(MultiplayerEvents.OnResearchPlayerLevelManagerChanged);
                    Shapez2Multiplayer.Research.PlayerLevelGoals.OnChanged.Unregister(MultiplayerEvents.OnResearchPlayerLevelGoalManagerChanged);
                    Shapez2Multiplayer.Research.Progress.OnChanged.Unregister(MultiplayerEvents.OnResearchUnlockProgressManagerChanged);
                    Shapez2Multiplayer.Research.UnlockManager.OnResearchManuallyUnlockedByPlayer.Unregister(MultiplayerEvents.OnResearchUnlockManagerResearchManuallyUnlockedByPlayer);
                    MultiplayerCore.socketManager.SendToAll(new DisconnectReasonPacket(MultiplayerCore.DisconnectReason.SessionClosed));
                    foreach (var sm in socketManager.SocketManagers)
                    {
                        sm.Update();
                        sm.Close();
                    }
                    HUDMultiplayerPausePanel.instance.ClearPlayers();
                    socketManager = null;
                }
                bool wasClient = Client;
                if (connectionManager != null)
                {
                    Shapez2Multiplayer.GameSessionOrchestrator?.LocalPlayer.HUDData.Pins.OnPinAdded.Unregister(MultiplayerEvents.OnPinAdded);
                    Shapez2Multiplayer.GameSessionOrchestrator?.LocalPlayer.HUDData.Pins.OnPinRemoved.Unregister(MultiplayerEvents.OnPinRemoved);
                    ((IEntityPlacementStateController?)Shapez2Multiplayer.EntityPlacementRunner)?.OnPlacementDataChanged.Unregister(MultiplayerEvents.OnPlacementDataChanged);
                    connectionManager.ConnectionManager.Close();
                    connectionManager = null;
                }
                ConnectingDialog?.OnClosed.Unregister(DialogClosed);
                ConnectingDialog?.Close();
                ConnectingDialog = null;
                ChunkedPacket.ChunkedPacketCache.Clear();
                ChunkedPacket.HostChunkedPacketCache.Clear();
                Lobby?.Leave();
                Lobby = null;
                if (wasClient && canReturnToMenu && Shapez2Multiplayer.Game.IsGameInSession(out IGameStartOptions _))
                {
                    Shapez2Multiplayer.GameSessionOrchestrator.AudioManager.FadeOutMusic();
                    Shapez2Multiplayer.GameFlowNavigator.LoadMainMenu().ContinueWith(async _ =>
                    {
                        if (disconnectReason == DisconnectReason.None) return;
                        await UniTask.DelayFrame(1);
                        Shapez2Multiplayer.MainMenuOrchestratorDialogStack.Show<HUDDialogSimpleInfo>(Globals.Resources.UIDialogSimpleInfoPrefab).Init("multiplayer.disconnected-dialog.title".T(), GetReasonTranslation(disconnectReason));
                        Shapez2Multiplayer.MainMenuStateManagerUISoundPlayer.PlayError();
                    });
                }
                else if (wasClient && Shapez2Multiplayer.Game.IsGameInMainMenu())
                {
                    if (disconnectReason == DisconnectReason.None) return;
                    Shapez2Multiplayer.MainMenuOrchestratorDialogStack.Show<HUDDialogSimpleInfo>(Globals.Resources.UIDialogSimpleInfoPrefab).Init("multiplayer.disconnected-dialog.title".T(), GetReasonTranslation(disconnectReason));
                    Shapez2Multiplayer.MainMenuStateManagerUISoundPlayer.PlayError();
                }
            } catch (Exception ex)
            {
                Shapez2Multiplayer.logger.Error.Log("Error while Disconnecting");
                Shapez2Multiplayer.logger.Exception.LogException(ex);
            }
        }
        public static IText GetReasonTranslation(DisconnectReason reason)
        {
            return reason switch
            {
                DisconnectReason.Timedout => "multiplayer.disconnected-dialog.description-timedout".T(),
                DisconnectReason.Kicked => "multiplayer.disconnected-dialog.description-kicked".T(),
                DisconnectReason.SessionClosed => "multiplayer.disconnected-dialog.description-sessionclosed".T(),
                _ => "multiplayer.disconnected-dialog.description-lostconnection".T(),
            };
        }
        public enum DisconnectReason : byte
        {
            None,
            Timedout,
            Lostconnection,
            Kicked,
            SessionClosed
        }
        public static async Task<Lobby[]?> FindFriendLobbies()
        {
            try
            {
                //return await SteamMatchmaking.LobbyList.RequestAsync() ?? new Steamworks.Data.Lobby[0];
                return SteamFriends.GetFriends().Where(friend => friend.IsPlayingThisGame).Where(friend => friend.GameInfo?.Lobby.HasValue ?? false).Select(friend => friend.GameInfo.Value.Lobby.Value).ToArray();
            } catch (Exception ex)
            {
                Shapez2Multiplayer.logger.Error.Log("Failed to load friend lobbies.");
                Shapez2Multiplayer.logger.Exception.LogException(ex);
                return null;
            }
        }
        public static void Update()
        {
            ChunkedPacket.Update();
            socketManager?.Update();
            connectionManager?.Update();
        }
    }
}
