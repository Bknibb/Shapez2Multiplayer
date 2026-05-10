using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Shapez2Multiplayer.Packets
{
    public class UpdateConnectionInfoPacket : IPacket
    {
        public List<InfoConnection> UpdateConnections = new List<InfoConnection>();
        public List<uint> DeleteConnections = new List<uint>();
        public UpdateConnectionInfoPacket() { }
        public UpdateConnectionInfoPacket(List<InfoConnection> updateConnections, List<uint> deleteConnections)
        {
            UpdateConnections = updateConnections;
            DeleteConnections = deleteConnections;
        }
        public void Decode(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream);
            var length = reader.ReadInt32();
            for (int i = 0; i < length; i++)
            {
                UpdateConnections.Add(InfoConnection.Decode(stream));
            }
            length = reader.ReadInt32();
            for (int i = 0; i < length; i++)
            {
                DeleteConnections.Add(reader.ReadUInt32());
            }
        }

        public void Encode(Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(UpdateConnections.Count);
            foreach (var connectionInfo in UpdateConnections)
            {
                connectionInfo.Encode(stream);
            }
            writer.Write(DeleteConnections.Count);
            foreach (var connectionId in DeleteConnections)
            {
                writer.Write(connectionId);
            }
        }

        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            if (MultiplayerCore.Client)
            {
                foreach (var connectionInfo in UpdateConnections)
                {
                    if (MultiplayerCore.connectionManager.ConnectionsDict.TryGetValue(connectionInfo.UniversalId, out InfoConnection c))
                    {
                        c.Update(connectionInfo);
                    } else
                    {
                        MultiplayerCore.connectionManager.ConnectionsDict[connectionInfo.UniversalId] = connectionInfo;
                        MultiplayerCore.connectionManager.Connections.Add(connectionInfo);
                        MultiplayerCore.connectionManager.OnOtherPlayerConnected(connectionInfo);
                    }
                }
                foreach (var connectionId in DeleteConnections)
                {
                    if (MultiplayerCore.connectionManager.ConnectionsDict.TryGetValue(connectionId, out InfoConnection connectionInfo))
                    {
                        MultiplayerCore.connectionManager.ConnectionsDict.Remove(connectionId);
                        MultiplayerCore.connectionManager.Connections.RemoveAll(c => c.UniversalId == connectionId);
                        MultiplayerCore.connectionManager.OnOtherPlayerDisconnected(connectionInfo);
                    }
                }
            }
        }
    }
}
