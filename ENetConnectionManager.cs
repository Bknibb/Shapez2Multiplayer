using ENet;
using Shapez2Multiplayer.Packets;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static Shapez2Multiplayer.MultiplayerCore;

namespace Shapez2Multiplayer
{
    public class ENetConnectionManager : IConnectionManager
    {
        public Host host;
        public Peer peer;
        public bool Connected = false;
        public IConnection Connection { get; }
        bool IConnectionManager.Connected => Connected;

        public event Action<DisconnectReason> DisconnectedEvent;
        public event Action ConnectedEvent;
        public event Action<byte[]> MessageEvent;

        public ENetConnectionManager(Host host, Peer peer)
        {
            this.host = host;
            this.peer = peer;
            Connection = new ENetConnection(peer);
        }

        public void Close()
        {
            peer.Disconnect(0);
            host.Flush();
            host.Dispose();
        }

        public void OnConnected()
        {
            Connected = true;
            if (SteamClient.IsValid) Connection.Send(PacketExtensions.Encode(new PlayerInfoPacket(SteamClient.Name))!, Packets.Packet.PlayerInfo);
            ConnectedEvent();
        }

        public void OnDisconnected(DisconnectReason reason)
        {
            Connected = false;
            DisconnectedEvent(reason);
        }

        public void OnMessage(ENet.Packet packet)
        {
            var buffer = new byte[packet.Length];
            packet.CopyTo(buffer);
            MessageEvent(buffer);
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
                        OnConnected();
                        break;

                    case EventType.Disconnect:
                        OnDisconnected(DisconnectReason.Lostconnection);
                        break;

                    case EventType.Timeout:
                        OnDisconnected(DisconnectReason.Timedout);
                        break;

                    case EventType.Receive:
                        OnMessage(netEvent.Packet);
                        netEvent.Packet.Dispose();
                        break;
                }
            }
        }
    }
}
