using Core.Localization;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ENet;
using Game.Core.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Shapez2Multiplayer.MultiplayerCore;

namespace Shapez2Multiplayer.Packets
{
    public class SavegamePacket : IPacket
    {
        public IReadOnlyDictionary<string, byte[]> Savegame { set; private get; }
        public string Uid { get; set; }
        public MemoryStream SavegameEncoded { private set; get; }
        public SavegamePacket() { }
        public SavegamePacket(IReadOnlyDictionary<string, byte[]> savegame, string uid)
        {
            Savegame = savegame;
            Uid = uid;
        }
        public void Decode(Stream stream)
        {
            int UidLength = stream.ReadByte();
            byte[] UidBytes = new byte[UidLength];
            stream.Read(UidBytes, 0, UidLength);
            Uid = UTF8Encoding.UTF8.GetString(UidBytes);
            Shapez2Multiplayer.logger.Info?.Log($"Recieved savegame packet with Uid: {Uid}");
            SavegameEncoded = new MemoryStream();
            stream.CopyTo(SavegameEncoded);
        }

        public void Encode(Stream stream)
        {
            var UidBytes = UTF8Encoding.UTF8.GetBytes(Uid);
            stream.WriteByte((byte)UidBytes.Length);
            stream.Write(UidBytes);
            Shapez2Multiplayer.WriteToStream(stream, Savegame);
        }

        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            if (MultiplayerCore.Hosting) return;
            Shapez2Multiplayer.logger.Info?.Log($"Attempting to load save due to SavegamePacket with Uid: {Uid}");
            MultiplayerCore.ConnectingDialog?.OnClosed.Unregister(MultiplayerCore.DialogClosed);
            MultiplayerCore.ConnectingDialog?.Close();
            MultiplayerCore.ConnectingDialog = null;
            Sequence? sequence = Shapez2Multiplayer.MainMenuOrchestratorFadeOut(false);
            if (sequence == null) return;
            sequence.OnComplete(() =>
            {
                IReadOnlyDictionary<Type, IDataSerializer> dataSerializers = Shapez2Multiplayer.MainMenuOrchestratorBackgroundGameOrchestrator.DataSerializers;
                SavegameBlobReader savegameBlobReader = new SaveFileAccessor(Shapez2Multiplayer.MainMenuOrchestratorLogger).ReadFromStream(SavegameEncoded, dataSerializers);
                Shapez2Multiplayer.MainMenuOrchestratorFlowNavigator.LoadSession(new GameStartOptionsContinueExisting(savegameBlobReader, false, Uid, false)).ContinueWith(_ =>
                {
                    if (!MultiplayerCore.Client)
                    {
                        Shapez2Multiplayer.GameSessionOrchestrator.AudioManager.FadeOutMusic();
                        Shapez2Multiplayer.GameFlowNavigator.LoadMainMenu().ContinueWith(async _ =>
                        {
                            await UniTask.DelayFrame(1);
                            Shapez2Multiplayer.MainMenuOrchestratorDialogStack.Show<HUDDialogSimpleInfo>(Globals.Resources.UIDialogSimpleInfoPrefab).Init("multiplayer.disconnected-dialog.title".T(), GetReasonTranslation(DisconnectReason.Lostconnection));
                            Shapez2Multiplayer.MainMenuStateManagerUISoundPlayer.PlayError();
                        });
                        return;
                    }
                    MultiplayerCore.connectionManager.HostDrawer = Shapez2Multiplayer.CreateOtherPlayerEntityPlacementDrawer();
                    MultiplayerCore.connectionManager.HostBuildingMassSelection = HUDMultiplayerMassSelectionsHost.Instance.CreateOtherPlayerHUDBuildingMassSelection();
                    MultiplayerCore.connectionManager.HostIslandMassSelection = HUDMultiplayerMassSelectionsHost.Instance.CreateOtherPlayerHUDIslandMassSelection();
                    Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer.HUDData.Pins.OnPinAdded.Register(MultiplayerEvents.OnPinAdded);
                    Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer.HUDData.Pins.OnPinRemoved.Register(MultiplayerEvents.OnPinRemoved);
                    ((IEntityPlacementStateController)Shapez2Multiplayer.EntityPlacementRunner).OnPlacementDataChanged.Register(MultiplayerEvents.OnPlacementDataChanged);
                    MultiplayerCore.connectionManager.FinishedConnecting = true;
                    foreach (var connection in MultiplayerCore.connectionManager.Connections)
                    {
                        if (connection.UniversalId != MultiplayerCore.connectionManager.UniversalId)
                        {
                            MultiplayerCore.connectionManager.PlayersDrawers.Add(connection.UniversalId, Shapez2Multiplayer.CreateOtherPlayerEntityPlacementDrawer());
                            MultiplayerCore.connectionManager.PlayersBuildingMassSelections.Add(connection.UniversalId, HUDMultiplayerMassSelectionsHost.Instance.CreateOtherPlayerHUDBuildingMassSelection());
                            MultiplayerCore.connectionManager.PlayersIslandMassSelections.Add(connection.UniversalId, HUDMultiplayerMassSelectionsHost.Instance.CreateOtherPlayerHUDIslandMassSelection());
                        }
                    }
                    MultiplayerCore.connectionManager.Send(new FinishedConnectingPacket());
                });
                Shapez2Multiplayer.MainMenuOrchestratorAnalyticsTracker.UIMenuContinueExistingGame();
            });
        }
    }
}
