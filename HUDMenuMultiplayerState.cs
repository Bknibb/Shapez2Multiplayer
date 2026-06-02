using Core.Dependency;
using Core.Localization;
using Shapez2UILib;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Shapez2Multiplayer
{
    public class HUDMenuMultiplayerState : HUDMainMenuState
    {
        public Transform Content;
        [Construct]
        private void Construct()
        {
            BtnRefresh.Text = Shapez2Multiplayer.MultiplayerRefreshTranslation;
            BtnRefresh.OnClick.AddListener(new UnityAction(() => this.RefreshSessionsList()));
            UISessionsContainer = Content.GetComponent<RectTransform>();
            UIEmptyText = Content.GetChild(0).GetComponent<HUDLocalizedText>();
            UIEmptyText.Text = "menu.multiplayer.no-sessions".T();
            UIErrorText = Content.GetChild(1).GetComponent<HUDLocalizedText>();
            BtnDirectConnect.Text = Shapez2Multiplayer.MultiplayerDirectConnectTranslation;
            BtnDirectConnect.OnClick.AddListener(new UnityAction(DirectConnect));
            DirectConnectInput.Placeholder = Shapez2Multiplayer.MultiplayerIpAddressTranslation;
        }
        public override void OnMenuEnterState(object payload)
        {
            RefreshSessionsList();
        }
        private async Task RefreshSessionsList()
        {
            ClearSessionsList();
            UIEmptyText.gameObject.SetActiveSelfExt(false);
            UIErrorText.gameObject.SetActiveSelfExt(false);
            //try
            //{
            //    serviceDiscovery?.Dispose();
            //    serviceDiscovery = new ServiceDiscovery();
            //    serviceDiscovery.ServiceInstanceDiscovered += async (_, e) =>
            //    {
            //        var server = new DiscoveredServer();
            //        Shapez2Multiplayer.logger.Info.Log($"Discovered server");
            //        foreach (var answer in e.Message.Answers)
            //        {
            //            if (answer is ARecord aRecord)
            //            {
            //                server.Address = aRecord.Address.ToString();
            //            }
            //            else if (answer is TXTRecord tXTRecord)
            //            {
            //                server.Properties = tXTRecord.Strings.ToDictionary(str => str.Split('=')[0], str => str.Split('=')[1]);
            //            }
            //        }
            //        Shapez2Multiplayer.logger.Info.Log($"address: {server.Address}");
            //        UIErrorText.gameObject.SetActiveSelfExt(false);
            //        UIEmptyText.gameObject.SetActiveSelfExt(false);
            //        try
            //        {
            //            var entry = Sessions.Cast<HUDSessionEntry?>().FirstOrDefault(entry => entry.Entry.Equals(server));
            //            if (entry != null)
            //            {
            //                entry.Entry = server;
            //                return;
            //            }
            //            HUDSavegameEntryPrefab hudsavegameEntryPrefab = RequestChildView<HUDSavegameEntryPrefab>(Shapez2Multiplayer.UISavegamePrefab).PlaceAt(UISessionsContainer, false);
            //            GameObject hudsavegameEntryGameObject = hudsavegameEntryPrefab.gameObject;
            //            var children = this.GetChildren();
            //            children.Remove(hudsavegameEntryPrefab);
            //            var loadedChildren = this.GetLoadedChildren();
            //            loadedChildren.Remove(hudsavegameEntryPrefab);
            //            DestroyImmediate(hudsavegameEntryPrefab); // this destroys the HUDSavegameEntryPrefab not the GameObject
            //            HUDSessionEntry hudSessionEntry = hudsavegameEntryGameObject.AddComponent<HUDSessionEntry>();
            //            this.AddChildViewInternal<HUDSessionEntry>(hudSessionEntry);
            //            hudSessionEntry.FromSavegameEntry(hudsavegameEntryPrefab);
            //            hudSessionEntry.Entry = server;
            //            Sessions.Add(hudSessionEntry);
            //        }
            //        catch (Exception ex)
            //        {
            //            Shapez2Multiplayer.logger.Error.Log("Error while adding session to list.");
            //            Shapez2Multiplayer.logger.Exception.LogException(ex);
            //        }
            //    };
            //    serviceDiscovery.QueryServiceInstances("_Shapez2Multiplayer._udp");
            //} catch (Exception ex)
            //{
            //    Shapez2Multiplayer.logger.Error.Log("Error while starting service discovery.");
            //    Shapez2Multiplayer.logger.Exception.LogException(ex);
            //}
            var lobbies = await MultiplayerCore.FindFriendLobbies();
            if (lobbies == null)
            {
                UIErrorText.Text = "menu.multiplayer.error-load-sessions".T();
                UIErrorText.gameObject.SetActiveSelfExt(true);
                return;
            }
            if (lobbies.Length == 0)
            {
                UIEmptyText.gameObject.SetActiveSelfExt(true);
                return;
            }
            try
            {
                foreach (var lobby in lobbies)
                {
                    await lobby.RefreshAsync();
                    HUDSavegameEntryPrefab hudsavegameEntryPrefab = RequestChildView<HUDSavegameEntryPrefab>(Shapez2Multiplayer.UISavegamePrefab).PlaceAt(UISessionsContainer, false);
                    GameObject hudsavegameEntryGameObject = hudsavegameEntryPrefab.gameObject;
                    var children = this.GetChildren();
                    children.Remove(hudsavegameEntryPrefab);
                    var loadedChildren = this.GetLoadedChildren();
                    loadedChildren.Remove(hudsavegameEntryPrefab);
                    DestroyImmediate(hudsavegameEntryPrefab); // this destroys the HUDSavegameEntryPrefab not the GameObject
                    HUDSessionEntry hudSessionEntry = hudsavegameEntryGameObject.AddComponent<HUDSessionEntry>();
                    this.AddChildViewInternal<HUDSessionEntry>(hudSessionEntry);
                    hudSessionEntry.FromSavegameEntry(hudsavegameEntryPrefab);
                    hudSessionEntry.Entry = new SteamLobby(lobby);
                    Sessions.Add(hudSessionEntry);
                }
            } catch (Exception ex)
            {
                UIErrorText.Text = "menu.multiplayer.error-load-sessions".T();
                UIErrorText.gameObject.SetActiveSelfExt(true);
                Shapez2Multiplayer.logger.Error.Log("Error while refreshing sessions list.");
                Shapez2Multiplayer.logger.Exception.LogException(ex);
            }
        }
        private void DirectConnect()
        {
            MultiplayerCore.DirectConnect(DirectConnectInput.Value).ContinueWith(t =>
            {
                if (!t.IsFaulted) { return; }
                Shapez2Multiplayer.MainMenuOrchestratorDialogStack.Show<HUDDialogSimpleInfo>(Globals.Resources.UIDialogSimpleInfoPrefab).Init("multiplayer.failedtoconnect-dialog.title".T(), "multiplayer.failedtoconnect-dialog.description".T());
                Shapez2Multiplayer.MainMenuStateManagerUISoundPlayer.PlayError();
            });
        }
        private void ClearSessionsList()
        {
            var children = this.GetChildren();
            foreach (HUDSessionEntry hudSessionEntry in Sessions)
            {
                children.Remove(hudSessionEntry);
                hudSessionEntry.Dispose();
                Destroy(hudSessionEntry.gameObject);
            }
            Sessions.Clear();
        }
        protected override void OnDispose()
        {
            
        }
        public override void GoBack()
        {
            this.Menu.SwitchToState<HUDMenuMainState>(null);
        }
        public class DiscoveredServer : ILobbyData, IEquatable<DiscoveredServer>
        {
            public string Address;
            public Dictionary<string, string> Properties;
            public string AdditionalTitle => Address;
            public bool Equals(ILobbyData other)
            {
                return other is DiscoveredServer discoveredServer && Equals(discoveredServer);
            }
            public bool Equals(DiscoveredServer other)
            {
                return Address == other.Address;
            }
            public string GetData(string key)
            {
                return Properties[key];
            }
        }
        public class SteamLobby : ILobbyData, IEquatable<SteamLobby>
        {
            public Lobby Lobby;
            public string AdditionalTitle => Lobby.Owner.Name;
            public SteamLobby(Lobby lobby)
            {
                Lobby = lobby;
            }

            public bool Equals(ILobbyData other)
            {
                return other is SteamLobby steamLobby && Equals(steamLobby);
            }
            public bool Equals(SteamLobby other)
            {
                return Lobby.Id == other.Lobby.Id;
            }

            public string GetData(string key)
            {
                return Lobby.GetData(key);
            }
        }
        public interface ILobbyData : IEquatable<ILobbyData>
        {
            string GetData(string key);
            string AdditionalTitle { get; }
        }
        public HUDButton BtnRefresh;
        private HUDLocalizedText UIEmptyText;
        private HUDLocalizedText UIErrorText;
        private RectTransform UISessionsContainer;
        private List<HUDSessionEntry> Sessions = new List<HUDSessionEntry>();
        //private ServiceDiscovery? serviceDiscovery;
        public HUDInputField DirectConnectInput;
        public HUDButton BtnDirectConnect;
    }
}
