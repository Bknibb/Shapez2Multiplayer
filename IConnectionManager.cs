using System;
using static Shapez2Multiplayer.MultiplayerCore;

namespace Shapez2Multiplayer
{
    public interface IConnectionManager
    {
        IConnection Connection { get; }
        bool Connected { get; }
        event Action ConnectedEvent;
        event Action<DisconnectReason> DisconnectedEvent;
        event Action<byte[]> MessageEvent;
        void Close();
        void Update();
    }
}
