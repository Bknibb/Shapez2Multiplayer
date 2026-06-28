using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Shapez2Multiplayer.Packets
{
    public class DeleteWaypointPacket : IPacket
    {
        public string UID { get; set; }
        public DeleteWaypointPacket() { }
        public DeleteWaypointPacket(string UID)
        {
            this.UID = UID;
        }
        public DeleteWaypointPacket(IPlayerWaypoint waypoint)
        {
            UID = waypoint.UID;
        }

        public bool Encode(Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(UID);
            return true;
        }

        public void Decode(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream);
            UID = reader.ReadString();
        }

        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            IPlayerWaypoint? waypoint = Shapez2Multiplayer.PlayerWaypoints.Cast<IPlayerWaypoint?>().FirstOrDefault(w => w.UID == UID);
            if (waypoint == null) return;
            Shapez2Multiplayer.IgnoreWaypointEvents = true;
            Shapez2Multiplayer.PlayerWaypoints.DeleteWaypoint(waypoint);
            Shapez2Multiplayer.IgnoreWaypointEvents = false;
        }
    }
}
