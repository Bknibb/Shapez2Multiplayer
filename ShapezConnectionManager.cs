using HarmonyLib;
using K4os.Compression.LZ4;
using Shapez2Multiplayer.Packets;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Shapez2Multiplayer.MultiplayerCore;

namespace Shapez2Multiplayer
{
    public class ShapezConnectionManager
    {
        public IConnectionManager ConnectionManager;
        public MultiplayerCore.DisconnectReason? RecievedDisconnectReason;
        public event Action<DisconnectReason> Disconnected;
        public List<InfoConnection> Connections = new List<InfoConnection>();
        public Dictionary<uint, InfoConnection> ConnectionsDict = new Dictionary<uint, InfoConnection>();
        public event Action<InfoConnection> OtherPlayerConnected;
        public event Action<InfoConnection> OtherPlayerDisconnected;
        public uint UniversalId;
        public OtherPlayerEntityPlacementDrawer HostDrawer;
        public Dictionary<uint, OtherPlayerEntityPlacementDrawer> PlayersDrawers = new Dictionary<uint, OtherPlayerEntityPlacementDrawer>();
        public OtherPlayerHUDBuildingMassSelection HostBuildingMassSelection;
        public OtherPlayerHUDIslandMassSelection HostIslandMassSelection;
        public Dictionary<uint, OtherPlayerHUDBuildingMassSelection> PlayersBuildingMassSelections = new Dictionary<uint, OtherPlayerHUDBuildingMassSelection>();
        public Dictionary<uint, OtherPlayerHUDIslandMassSelection> PlayersIslandMassSelections = new Dictionary<uint, OtherPlayerHUDIslandMassSelection>();
        public bool FinishedConnecting = false;
        public bool InSeperateThread = false;
        public ShapezConnectionManager(IConnectionManager connectionManager)
        {
            ConnectionManager = connectionManager;
            ConnectionManager.ConnectedEvent += OnConnected;
            ConnectionManager.DisconnectedEvent += OnDisconnected;
            ConnectionManager.MessageEvent += OnMessage;
        }
        public void OnConnected()
        {
            Shapez2Multiplayer.logger.Info?.Log("Connected to host!");
        }

        public void OnDisconnected(DisconnectReason reason)
        {
            Shapez2Multiplayer.logger.Info?.Log("Disconnected from host");
            Disconnected(RecievedDisconnectReason ?? reason);
        }

        public void OnOtherPlayerConnected(InfoConnection connection)
        {
            OtherPlayerConnected?.Invoke(connection);
            HUDMultiplayerPausePanel.instance?.AddPlayer(connection);
            if (connection.UniversalId != UniversalId && FinishedConnecting)
            {
                PlayersDrawers.Add(connection.UniversalId, Shapez2Multiplayer.CreateOtherPlayerEntityPlacementDrawer());
                PlayersBuildingMassSelections.Add(connection.UniversalId, HUDMultiplayerMassSelectionsHost.Instance.CreateOtherPlayerHUDBuildingMassSelection());
                PlayersIslandMassSelections.Add(connection.UniversalId, HUDMultiplayerMassSelectionsHost.Instance.CreateOtherPlayerHUDIslandMassSelection());
            }
        }

        public void OnOtherPlayerDisconnected(InfoConnection connection)
        {
            OtherPlayerDisconnected?.Invoke(connection);
            HUDMultiplayerPausePanel.instance?.RemovePlayer(connection);
            PlayersDrawers.Remove(connection.UniversalId);
            PlayersBuildingMassSelections.Remove(connection.UniversalId);
            PlayersIslandMassSelections.Remove(connection.UniversalId);
        }

        public void OnMessage(byte[] data)
        {
            var compressedLength = data.Length;
            data = LZ4Pickler.Unpickle(data);
#if DEBUG
            Shapez2Multiplayer.logger.Info?.Log($"Recieved Data Of Length: {data.Length}, Compressed {compressedLength}");
#endif
            var packet = PacketExtensions.Decode(data, out uint? from);

            InfoConnection? fromInfo = null;
            if (from.HasValue) if (ConnectionsDict.TryGetValue(from.Value, out InfoConnection c)) fromInfo = c;
            packet.Handle(null, fromInfo);
        }
        public bool SendToAll(IPacket packet)
        {
            byte[] encoded = PacketExtensions.Encode(new SendToAllPacket(packet));
            var ret = ConnectionManager.Connection.Send(encoded);
            if (!ret) Shapez2Multiplayer.logger.Warning.Log($"Dropped packet {packet.GetType().Name} because send failed");
            return ret;
        }
        public bool Send(IPacket packet)
        {
            byte[] encoded = PacketExtensions.Encode(packet);
            var ret = ConnectionManager.Connection.Send(encoded);
            if (!ret) Shapez2Multiplayer.logger.Warning.Log($"Dropped packet {packet.GetType().Name} because send failed");
            return ret;
        }
        float MassSelectionsTimer = 0.0f;
        const float SYNC_MASS_SELECTIONS_TIME = 1.0f;
        public void Update()
        {
            MassSelectionsTimer += Time.deltaTime;
            if (MassSelectionsTimer >= SYNC_MASS_SELECTIONS_TIME && Shapez2Multiplayer.GameSessionOrchestrator != null)
            {
                MassSelectionsTimer = 0.0f;
                SendToAll(new UpdateBuildingMassSelectionPacket(Shapez2Multiplayer.HUDBuildingMassSelection, Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer.InteractionState.BuildingSelection.ToList()));
                SendToAll(new UpdateIslandMassSelectionPacket(Shapez2Multiplayer.HUDIslandMassSelection, Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer.InteractionState.IslandSelection.ToList()));
            }
            if (!InSeperateThread) ConnectionManager.Update();
        }
        public async Task SeperateThread()
        {
            InSeperateThread = true;
            while (InSeperateThread)
            {
                ConnectionManager.Update();
                await Task.Delay(1000);
            }
        }
        public void StartSeperateThread()
        {
            Task.Run(SeperateThread);
        }
        public void StopSeperateThread()
        {
            InSeperateThread = false;
        }
    }
}
