using Core.Localization;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Shapez2Multiplayer.Packets
{
    public class PlayerInfoPacket : IPacket
    {
        public string Name { get; set; }
        public PlayerInfoPacket() { }
        public PlayerInfoPacket(string name) {
            Name = name;
        }
        public void Decode(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream);
            Name = reader.ReadString();
        }

        public void Encode(Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(Name);
        }

        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            if (connection is ENetConnection eNetConnection)
            {
                ENetConnection.NameCache[eNetConnection.Id] = Name;
                if (MultiplayerCore.socketManager != null)
                {
                    MultiplayerCore.socketManager.SendToAll(new UpdateConnectionInfoPacket(new List<InfoConnection>() { new InfoConnection(eNetConnection) }, new List<uint>()));
                    if (MultiplayerCore.socketManager.Connecting.Contains(eNetConnection))
                    {
                        MultiplayerCore.socketManager.SendToAllExcept(new PausePacket(true, new CombinedText("multiplayer.paused-dialog.description-waitingforplayer".T(), new RawText("\n" + string.Join(", ", MultiplayerCore.socketManager.Connecting.Select(c => c.Name))))), MultiplayerCore.socketManager.Connecting);
                        new PausePacket(true, new CombinedText("multiplayer.paused-dialog.description-waitingforplayer".T(), new RawText("\n" + string.Join(", ", MultiplayerCore.socketManager.Connecting.Select(c => c.Name))))).Handle(null);
                    }
                }
            }
        }
    }
}