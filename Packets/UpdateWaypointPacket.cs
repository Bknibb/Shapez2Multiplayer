using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Shapez2Multiplayer.Packets
{
    public class UpdateWaypointPacket : IPacket
    {
        public PlayerWaypoint Waypoint { get; set; }
        public UpdateWaypointPacket() { }
        public UpdateWaypointPacket(IPlayerWaypoint waypoint)
        {
            Waypoint = (PlayerWaypoint)waypoint;
        }

        public bool Encode(Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(Waypoint.Name);
            writer.Write(Waypoint.ShapeIconKey);
            writer.Write(Waypoint.UID);
            writer.Write(Waypoint.PositionX);
            writer.Write(Waypoint.PositionY);
            writer.Write(Waypoint.Zoom);
            writer.Write(Waypoint.Angle);
            writer.Write(Waypoint.BuildingLayer);
            writer.Write(Waypoint.IslandLayer);
            writer.Write(Waypoint.RotationDegrees);
            return true;
        }

        public void Decode(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream);
            Waypoint = new PlayerWaypoint()
            {
                Name = reader.ReadString(),
                ShapeIconKey = reader.ReadString(),
                UID = reader.ReadString(),
                PositionX = reader.ReadDouble(),
                PositionY = reader.ReadDouble(),
                Zoom = reader.ReadSingle(),
                Angle = reader.ReadSingle(),
                BuildingLayer = reader.ReadInt16(),
                IslandLayer = reader.ReadInt16(),
                RotationDegrees = reader.ReadSingle(),
            };
        }

        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            PlayerWaypoint? modifyWaypoint = (PlayerWaypoint?)Shapez2Multiplayer.PlayerWaypoints.Waypoints.Cast<IPlayerWaypoint?>().FirstOrDefault(w => w.UID == Waypoint.UID);
            if (modifyWaypoint == null)
            {
                Shapez2Multiplayer.IgnoreWaypointEvents = true;
                Shapez2Multiplayer.PlayerWaypoints.Add(Waypoint);
                Shapez2Multiplayer.IgnoreWaypointEvents = false;
            } else
            {
                Shapez2Multiplayer.IgnoreWaypointEvents = true;
                Shapez2Multiplayer.PlayerWaypoints.ChangeWaypoint(modifyWaypoint, Waypoint.Name, Waypoint.ShapeIconKey, Waypoint);
                Shapez2Multiplayer.IgnoreWaypointEvents = false;
            }
        }
    }
}
