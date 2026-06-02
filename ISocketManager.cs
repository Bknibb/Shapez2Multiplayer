using System;
using System.Collections.Generic;

namespace Shapez2Multiplayer
{
    public interface ISocketManager
    {
        IReadOnlyCollection<IConnection> Connected { get; }
        bool Valid { get; }
        event Action<IConnection> ConnectedEvent;
        event Action<IConnection> DisconnectedEvent;
        event Action<IConnection, byte[]> MessageEvent;
        void Close();
        void Update();
    }
}
