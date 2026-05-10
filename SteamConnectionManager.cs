using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static Shapez2Multiplayer.MultiplayerCore;

namespace Shapez2Multiplayer
{
    public class SteamConnectionManager : ConnectionManager, IConnectionManager
    {
        public IConnection Connection { get; private set; }

        public bool Connected => base.Connected;

        public event Action<DisconnectReason> DisconnectedEvent;
        public event Action ConnectedEvent;
        public event Action<byte[]> MessageEvent;

        public SteamConnectionManager()
        {

        }

        public override void OnConnected(ConnectionInfo info)
        {
            base.OnConnected(info);
            Connection = new SteamConnection(base.Connection);
            new Friend(info.Identity.SteamId).RequestInfoAsync().ContinueWith(_ => SteamConnection.NameCache[base.Connection.Id] = new Friend(info.Identity.SteamId).Name);
            ConnectedEvent();
        }

        public override void OnDisconnected(ConnectionInfo info)
        {
            base.OnDisconnected(info);
            DisconnectedEvent(info.EndReason == NetConnectionEnd.Misc_Timeout || info.EndReason == NetConnectionEnd.Remote_Timeout ? DisconnectReason.Timedout : DisconnectReason.Lostconnection);
        }

        public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            var buffer = new byte[size];
            Marshal.Copy(data, buffer, 0, size);
            MessageEvent(buffer);
        }

        public void Close()
        {
            base.Close();
        }

        public void Update()
        {
            SteamClient.RunCallbacks();
            Receive();
        }
    }
}
