using ENet;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Shapez2Multiplayer
{
    public class ENetSocketManager : ISocketManager
    {
        public Host host;
        public IReadOnlyCollection<Peer> Connections => _Connections;
        public bool Valid { get; private set; } = true;
        private List<Peer> _Connections = new List<Peer>();
        public IReadOnlyCollection<IConnection> Connected => _ENetConnections.Values;
        private Dictionary<uint, ENetConnection> _ENetConnections = new Dictionary<uint, ENetConnection>();

        public event Action<IConnection> DisconnectedEvent;
        public event Action<IConnection> ConnectedEvent;
        public event Action<IConnection, byte[]> MessageEvent;

        public ENetSocketManager(Host host)
        {
            this.host = host;
        }

        public void Close()
        {
            Valid = false;
            foreach (var peer in _Connections) peer.Disconnect(0);
            host.Flush();
            host.Dispose();
        }

        public void OnConnected(Peer peer)
        {
            var connection = new ENetConnection(peer);
            ConnectedEvent(connection);
            _ENetConnections.Add(peer.ID, connection);
            _Connections.Add(peer);
        }

        public void OnDisconnected(Peer peer)
        {
            DisconnectedEvent(_ENetConnections[peer.ID]);
            _ENetConnections.Remove(peer.ID);
            _Connections.Remove(peer);
        }

        public void OnMessage(Peer peer, Packet packet)
        {
            var buffer = new byte[packet.Length];
            packet.CopyTo(buffer);
            MessageEvent(_ENetConnections[peer.ID], buffer);
        }

        public void Update()
        {
            bool polled = false;

            while (!polled)
            {
                if (host.CheckEvents(out Event netEvent) <= 0)
                {
                    if (host.Service(0, out netEvent) <= 0)
                        break;

                    polled = true;
                }

                switch (netEvent.Type)
                {
                    case EventType.None:
                        break;

                    case EventType.Connect:
                        OnConnected(netEvent.Peer);
                        break;

                    case EventType.Disconnect:
                        OnDisconnected(netEvent.Peer);
                        break;

                    case EventType.Timeout:
                        OnDisconnected(netEvent.Peer);
                        break;

                    case EventType.Receive:
                        OnMessage(netEvent.Peer, netEvent.Packet);
                        netEvent.Packet.Dispose();
                        break;
                }
            }
        }
    }
}
