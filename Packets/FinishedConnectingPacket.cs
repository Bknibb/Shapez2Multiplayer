using Core.Localization;
using System.IO;
using System.Linq;

namespace Shapez2Multiplayer.Packets
{
    public class FinishedConnectingPacket : IPacket
    {
        public void Decode(Stream stream)
        {
            
        }

        public bool Encode(Stream stream)
        {
            return true;
        }

        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            if (connection == null)
            {
                Shapez2Multiplayer.logger.Warning.Log("FinishedConnectingPacket Recieved From Host");
                return;
            }
            MultiplayerCore.socketManager.Connecting.Remove(connection);
            if (MultiplayerCore.socketManager.Connecting.Count == 0)
            {
                MultiplayerCore.socketManager.SendToAll(new PausePacket(false));
                new PausePacket(false).Handle(null);
                PlacementIndicatorDataPacket.SentToAllConnections = false;
                MultiplayerCore.socketManager.ForceUpdateCursor();
                MultiplayerCore.socketManager.PingUpdateTimer = float.MaxValue;
            }
            else
            {
                MultiplayerCore.socketManager.SendToAllExcept(new PausePacket(true, new CombinedText("multiplayer.paused-dialog.description-waitingforplayer".T(), new RawText("\n" + string.Join(", ", MultiplayerCore.socketManager.Connecting.Select(c => c.Name))))), MultiplayerCore.socketManager.Connecting);
                new PausePacket(true, new CombinedText("multiplayer.paused-dialog.description-waitingforplayer".T(), new RawText("\n" + string.Join(", ", MultiplayerCore.socketManager.Connecting.Select(c => c.Name))))).Handle(null);
            }
        }
    }
}
