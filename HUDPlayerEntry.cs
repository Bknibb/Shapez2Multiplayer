using Core.Dependency;
using Core.Localization;
using Game.Core.Modding;
using Shapez2Multiplayer.Packets;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine.Events;

namespace Shapez2Multiplayer
{
    public class HUDPlayerEntry : HUDComponent
    {
        public IConnection Connection { get; set; }
        public TextMeshProUGUI NameText { get; set; }
        public TextMeshProUGUI PingText { get; set; }
        public HUDButton KickButton { get; set; }
        [Construct]
        private void Construct()
        {
            KickButton.Interactable = !(Connection is InfoConnection);
            KickButton.Text = "multiplayer.kick".T();
            KickButton.OnClick.AddListener(new UnityAction(() =>
            {
                MultiplayerCore.socketManager?.Disconnect(Connection, MultiplayerCore.DisconnectReason.Kicked);
                Connection.Close();
            }));
        }
        private void OnEnable()
        {
            InvokeRepeating(nameof(EntryUpdate), 0f, 1f);
        }
        private void OnDisable()
        {
            CancelInvoke();
        }
        public void EntryUpdate()
        {
            NameText.text = Connection.Name;
            PingText.text = Connection.Ping.ToString();
        }

        protected override void OnDispose()
        {
            
        }
    }
}
