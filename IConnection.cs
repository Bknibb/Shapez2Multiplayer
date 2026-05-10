using System;
using System.Collections.Generic;
using System.Text;

namespace Shapez2Multiplayer
{
    public interface IConnection : IEquatable<IConnection>
    {
        public uint Id { get; }
        public uint UniversalId { get; }
        public int Ping { get; }
        public string Name => $"Player {UniversalId}";

        public void Close();
        public bool Send(byte[] data);
    }
}
