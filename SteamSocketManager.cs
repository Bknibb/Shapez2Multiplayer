using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shapez2Multiplayer
{
    public class SteamSocketManager : SocketManager, ISocketManager
    {
        public IReadOnlyCollection<IConnection> Connected => _SteamConnections.Values;
        public bool Valid { get; private set; } = true;
        private Dictionary<uint, SteamConnection> _SteamConnections = new Dictionary<uint, SteamConnection>();

        public event Action<IConnection> DisconnectedEvent;
        public event Action<IConnection> ConnectedEvent;
        public event Action<IConnection, byte[]> MessageEvent;

        public override void OnConnected(Connection connection, ConnectionInfo info)
        {
            base.OnConnected(connection, info);
            new Friend(info.Identity.SteamId).RequestInfoAsync().ContinueWith(_ => SteamConnection.NameCache[connection.Id] = new Friend(info.Identity.SteamId).Name);
            var steamConnection = new SteamConnection(connection);
            _SteamConnections.Add(connection.Id, steamConnection);
            ConnectedEvent(steamConnection);
        }

        public override void OnDisconnected(Connection connection, ConnectionInfo info)
        {
            base.OnDisconnected(connection, info);
            DisconnectedEvent(_SteamConnections[connection.Id]);
            _SteamConnections.Remove(connection.Id);
        }

        public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            var buffer = new byte[size];
            Marshal.Copy(data, buffer, 0, size);
            MessageEvent(_SteamConnections[connection.Id], buffer);
        }

        public void Update()
        {
            SteamClient.RunCallbacks();
            Receive();
        }

        void ISocketManager.Close()
        {
            Valid = false;
            base.Close();
        }
    }
}
