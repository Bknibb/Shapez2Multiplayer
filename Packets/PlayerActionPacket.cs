using Core.Collections;
using Core.Collections.Scoped;
using Game.Core.Coordinates;
using Game.Core.Serialization;
using Game.Core.Trains;
using Game.Placement.Data;
using Game.Placement.Processing;
using HarmonyLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Shapez2Multiplayer.Packets
{
    public class PlayerActionPacket : IPacket
    {
        public IPlayerAction PlayerAction { get; set; }
        public PlayerActionPacket() { }
        public PlayerActionPacket(IPlayerAction playerAction)
        {
            PlayerAction = playerAction;
        }
        public void Decode(Stream stream)
        {
            Encoding.serializationVisitor = new BinarySerializationVisitor(false, false, Savegame.CurrentVersion, stream, Shapez2Multiplayer.GameSessionOrchestrator.DataSerializers, Shapez2Multiplayer.logger);
            if (stream.Position >= stream.Length)
            {
                Shapez2Multiplayer.logger.Error.Log("Recieved empty player action packet");
                return;
            }
            PlayerAction = Encoding.DecodePlayerAction(stream);
        }

        public void Encode(Stream stream)
        {
            Encoding.serializationVisitor = new BinarySerializationVisitor(true, false, Savegame.CurrentVersion, stream, Shapez2Multiplayer.GameSessionOrchestrator.DataSerializers, Shapez2Multiplayer.logger);
            Encoding.Encode(PlayerAction, stream);
        }

        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            if (PlayerAction == null) return;
#if DEBUG
            Shapez2Multiplayer.DebugLastAction = PlayerAction;
#endif
            Shapez2Multiplayer.WaitingActions.Add(PlayerAction);
            if (!Shapez2Multiplayer.PlayerActions.TryScheduleActionNoDetection(PlayerAction))
            {
                Shapez2Multiplayer.logger.Warning.Log("Action Failed, Likely Desync");
                Shapez2Multiplayer.WaitingActions.Remove(PlayerAction);
            }
        }
    }
}
