using Core.Localization;
using K4os.Compression.LZ4;
using Shapez2Multiplayer.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Mathematics;
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
                PlayersBuildingMassSelections.Add(connection.UniversalId, HUDMultiplayerMassSelectionsHost.Instance.CreateOtherPlayerHUDBuildingMassSelection(connection));
                PlayersIslandMassSelections.Add(connection.UniversalId, HUDMultiplayerMassSelectionsHost.Instance.CreateOtherPlayerHUDIslandMassSelection(connection));
                PlacementIndicatorDataPacket.SentToAllConnections = false;
                ForceUpdateCursor();
            }
        }

        public void OnOtherPlayerDisconnected(InfoConnection connection)
        {
            OtherPlayerDisconnected?.Invoke(connection);
            HUDMultiplayerPausePanel.instance?.RemovePlayer(connection);
            PlayersDrawers.Remove(connection.UniversalId);
            PlayersBuildingMassSelections.Remove(connection.UniversalId);
            PlayersIslandMassSelections.Remove(connection.UniversalId);
            HUDMultiplayerCursors.Instance.RemoveCursor(connection);
            Shapez2Multiplayer.HUD.Events.ShowNotification.Invoke(new HUDNotificationData(HUDNotificationType.Info, "multiplayer.player-lost-connection".T().Bind("player-name", new RawText(connection.Name))));
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
            var encoded = PacketExtensions.Encode(new SendToAllPacket(packet));
            if (encoded == null) return true;
            var type = PacketExtensions.GetFromType(packet.GetType());
            var ret = ConnectionManager.Connection.Send(encoded, type);
            if (!ret) Shapez2Multiplayer.logger.Warning.Log($"Dropped packet {packet.GetType().Name} because send failed");
            return ret;
        }
        public bool Send(IPacket packet)
        {
            var encoded = PacketExtensions.Encode(packet);
            if (encoded == null) return true;
            var type = PacketExtensions.GetFromType(packet.GetType());
            if (packet is SendToAllPacket sendToAllPacket) type = PacketExtensions.GetFromType(sendToAllPacket.Packet.GetType());
            var ret = ConnectionManager.Connection.Send(encoded, type);
            if (!ret) Shapez2Multiplayer.logger.Warning.Log($"Dropped packet {packet.GetType().Name} because send failed");
            return ret;
        }
        public void ForceUpdateCursor()
        {
            if (Shapez2Multiplayer.GameSessionOrchestrator == null) return;
            SyncCursorTimer = 0.0f;
            var cursorState = Shapez2Multiplayer.GameCursorManager._State;
            if (ScreenUtils.TryGetWorldCoordinate(Shapez2Multiplayer.GameSessionOrchestrator.Viewport, Shapez2Multiplayer.GameSessionOrchestrator.Viewport.CursorScreenPosition, out var cursorWorldPosition))
            {
                LastCursorState = cursorState;
                LastCursorWorldPosition = (float3)cursorWorldPosition;
                SendToAll(new CursorPacket((float3)cursorWorldPosition, cursorState));
            }
            var viewportIslandLayer = Shapez2Multiplayer.GameSessionOrchestrator.Viewport.IslandLayer;
            var viewportBuildingLayer = Shapez2Multiplayer.GameSessionOrchestrator.Viewport.BuildingLayer;
            var viewportShowAllBuildingLayers = Shapez2Multiplayer.GameSessionOrchestrator.Viewport.ShowAllBuildingLayers;
            var viewportShowAllIslandLayers = Shapez2Multiplayer.GameSessionOrchestrator.Viewport.ShowAllIslandLayers;
            LastViewportIslandLayer = viewportIslandLayer;
            LastViewportBuildingLayer = viewportBuildingLayer;
            LastViewportShowAllBuildingLayers = viewportShowAllBuildingLayers;
            LastViewportShowAllIslandLayers = viewportShowAllIslandLayers;
            SendToAll(new ViewportPropertyChangedPacket(viewportIslandLayer, viewportBuildingLayer, viewportShowAllBuildingLayers, viewportShowAllIslandLayers));
            SendToAll(new PlayerInteractionStateChangedPacket(Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer.InteractionState.State));
        }
        float MassSelectionsTimer = 0.0f;
        float SyncCursorTimer = 0.0f;
        const float SYNC_MASS_SELECTIONS_TIME = 1.0f;
        const float SYNC_CURSOR_TIME = 0.1f;
        CursorHoverState? LastCursorState;
        float3? LastCursorWorldPosition;
        short? LastViewportIslandLayer;
        short? LastViewportBuildingLayer;
        bool? LastViewportShowAllBuildingLayers;
        bool? LastViewportShowAllIslandLayers;
        public void Update()
        {
            MassSelectionsTimer += Time.deltaTime;
            if (MassSelectionsTimer >= SYNC_MASS_SELECTIONS_TIME && Shapez2Multiplayer.GameSessionOrchestrator != null)
            {
                MassSelectionsTimer = 0.0f;
                SendToAll(new UpdateBuildingMassSelectionPacket(Shapez2Multiplayer.HUDBuildingMassSelection, Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer.InteractionState.BuildingSelection.ToList()));
                SendToAll(new UpdateIslandMassSelectionPacket(Shapez2Multiplayer.HUDIslandMassSelection, Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer.InteractionState.IslandSelection.ToList()));
            }
            SyncCursorTimer += Time.deltaTime;
            if (SyncCursorTimer >= SYNC_CURSOR_TIME && Shapez2Multiplayer.GameSessionOrchestrator != null)
            {
                SyncCursorTimer = 0.0f;
                var cursorState = Shapez2Multiplayer.GameCursorManager._State;
                if (ScreenUtils.TryGetWorldCoordinate(Shapez2Multiplayer.GameSessionOrchestrator.Viewport, Shapez2Multiplayer.GameSessionOrchestrator.Viewport.CursorScreenPosition, out var cursorWorldPosition) && (cursorState != LastCursorState || !((float3)cursorWorldPosition).Equals(LastCursorWorldPosition)))
                {
                    LastCursorState = cursorState;
                    LastCursorWorldPosition = (float3)cursorWorldPosition;
                    SendToAll(new CursorPacket((float3)cursorWorldPosition, cursorState));
                }
                var viewportIslandLayer = Shapez2Multiplayer.GameSessionOrchestrator.Viewport.IslandLayer;
                var viewportBuildingLayer = Shapez2Multiplayer.GameSessionOrchestrator.Viewport.BuildingLayer;
                var viewportShowAllBuildingLayers = Shapez2Multiplayer.GameSessionOrchestrator.Viewport.ShowAllBuildingLayers;
                var viewportShowAllIslandLayers = Shapez2Multiplayer.GameSessionOrchestrator.Viewport.ShowAllIslandLayers;
                if (viewportIslandLayer != LastViewportIslandLayer || viewportBuildingLayer != LastViewportBuildingLayer || viewportShowAllBuildingLayers != LastViewportShowAllBuildingLayers || viewportShowAllIslandLayers != LastViewportShowAllIslandLayers)
                {
                    LastViewportIslandLayer = viewportIslandLayer;
                    LastViewportBuildingLayer = viewportBuildingLayer;
                    LastViewportShowAllBuildingLayers = viewportShowAllBuildingLayers;
                    LastViewportShowAllIslandLayers = viewportShowAllIslandLayers;
                    SendToAll(new ViewportPropertyChangedPacket(viewportIslandLayer, viewportBuildingLayer, viewportShowAllBuildingLayers, viewportShowAllIslandLayers));
                }
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
