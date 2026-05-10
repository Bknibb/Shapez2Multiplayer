using Core.Dependency;
using Core.Localization;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Core.View;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Shapez2Multiplayer.MultiplayerCore;

namespace Shapez2Multiplayer
{
    public class HUDMenuMultiplayerState : HUDMainMenuState
    {
        public Transform Content;
        [Construct]
        private void Construct()
        {
            UIBtnBack.GetComponentInChildren<HUDLocalizedText>().Text = Shapez2Multiplayer.MultiplayerButtonTranslation;
            UIBtnBack.OnClick.AddListener(new UnityAction(this.GoBack));
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
                    var children = (List<IView>)Shapez2Multiplayer.HUDComponentChildren.GetValue(this);
                    children.Remove(hudsavegameEntryPrefab);
                    var loadedChildren = (HashSet<IView>)Shapez2Multiplayer.HUDComponentLoadedChildren.GetValue(this);
                    loadedChildren.Remove(hudsavegameEntryPrefab);
                    DestroyImmediate(hudsavegameEntryPrefab); // this destroys the HUDSavegameEntryPrefab not the GameObject
                    HUDSessionEntry hudSessionEntry = hudsavegameEntryGameObject.AddComponent<HUDSessionEntry>();
                    Shapez2Multiplayer.componentAddChildViewInternal.MakeGenericMethod(typeof(HUDSessionEntry)).Invoke(this, new object[] { hudSessionEntry });
                    hudSessionEntry.FromSavegameEntry(hudsavegameEntryPrefab);
                    hudSessionEntry.Entry = lobby;
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
            var children = (List<IView>)Shapez2Multiplayer.HUDComponentChildren.GetValue(this);
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
            UIBtnBack.OnClick.RemoveListener(new UnityAction(this.GoBack));

        }
        public override void GoBack()
        {
            this.Menu.SwitchToState<HUDMenuMainState>(null);
        }
        public HUDMenuBackButton UIBtnBack;
        public HUDButton BtnRefresh;
        private HUDLocalizedText UIEmptyText;
        private HUDLocalizedText UIErrorText;
        private RectTransform UISessionsContainer;
        private List<HUDSessionEntry> Sessions = new List<HUDSessionEntry>();
        public HUDInputField DirectConnectInput;
        public HUDButton BtnDirectConnect;
    }
}
