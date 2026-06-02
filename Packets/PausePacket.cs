using Core.Localization;
using System.IO;

namespace Shapez2Multiplayer.Packets
{
    public class PausePacket : IPacket
    {
        public bool Pause { get; set; }
        public IText? PauseReason { get; set; }
        public PausePacket() { }
        public PausePacket(bool pause)
        {
            Pause = pause;
        }
        public PausePacket(bool pause, IText? reason)
        {
            Pause = pause;
            PauseReason = reason;
        }
        public void Decode(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream);
            Pause = reader.ReadBoolean();
            if (reader.ReadBoolean()) PauseReason = Encoding.DecodeText(stream);
        }

        public bool Encode(Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(Pause);
            writer.Write(PauseReason != null);
            if (PauseReason != null) Encoding.Encode(PauseReason, stream);
            return true;
        }
        public static HUDDialogSimpleInfo? Dialog;
        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            Shapez2Multiplayer.BypassSimulationSpeedCheck = true;
            Shapez2Multiplayer.SimulationSpeed.IsPaused = Pause;
            Shapez2Multiplayer.BypassSimulationSpeedCheck = false;
            if (Pause)
            {
                if (PauseReason != null)
                {
                    if (Dialog == null)
                    {
                        Dialog = Shapez2Multiplayer.DialogStack.Show<HUDDialogSimpleInfo>(Globals.Resources.UIDialogSimpleInfoPrefab);
                        Dialog.GetComponentInChildren<HUDButton>(true).gameObject.SetActiveSelfExt(false);
                        Dialog.Init("mutliplayer.paused-dialog.title".T(), PauseReason);
                    } else
                    {
                        Dialog.Init("mutliplayer.paused-dialog.title".T(), PauseReason);
                    }
                }
            } else
            {
                Dialog?.GetComponentInChildren<HUDButton>(true).gameObject.SetActiveSelfExt(true);
                Dialog?.Close();
                Dialog = null;
            }
        }
    }
}
