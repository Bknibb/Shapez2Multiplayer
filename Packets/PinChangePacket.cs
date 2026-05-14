using Game.HUD.QuestArea.PinnedShapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Shapez2Multiplayer.Packets
{
    public class PinChangePacket : IPacket
    {
        public IPin Pin;
        public bool Remove;
        public PinChangePacket() { }
        public PinChangePacket(IPin pin, bool remove)
        {
            Pin = pin;
            Remove = remove;
        }

        public void Decode(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream);
            Remove = reader.ReadBoolean();
            Pin = PinFactory.Deserialize(reader.ReadString());
        }

        public bool Encode(Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(Remove);
            writer.Write(Pin.Serialize());
            return true;
        }

        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            if (Remove)
            {
                if (!Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer.HUDData.Pins.TryUnpin(Pin)) Shapez2Multiplayer.logger.Warning.Log("Failed To Unpin, Likely Desync");
            } else
            {
                if (!Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer.HUDData.Pins.TryPin(Pin)) Shapez2Multiplayer.logger.Warning.Log("Failed To Pin, Likely Desync");
            }
        }
    }
}
