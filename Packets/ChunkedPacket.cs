using Core.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Shapez2Multiplayer.Packets
{
    public class ChunkedPacket : IPacket
    {
        public static readonly Dictionary<uint, Dictionary<uint, ChunkCacheData>> ChunkedPacketCache = new Dictionary<uint, Dictionary<uint, ChunkCacheData>>();
        public static readonly Dictionary<uint, ChunkCacheData> HostChunkedPacketCache = new Dictionary<uint, ChunkCacheData>();
        public static readonly Queue<Tuple<ChunkedPacket, IConnection>> ToSend = new Queue<Tuple<ChunkedPacket, IConnection>>();
        public static uint CurrentId = 0;

        public byte[] Data;
        public uint Id;
        public bool first = false;
        public bool finished = false;
        public uint Index;
        public uint TotalChunks;
        public const int ChunkSize = 1024 * 128;
        public const int ChunkThreshold = 1024 * 256;
        public const float SendDelay = 1f;
        static float SendTimer = 0.0f;
        public ChunkedPacket() { }
        public static void Send(byte[] data, IConnection sender)
        {
            var id = CurrentId++;
            uint index = 1;
            uint total = (uint)MathF.Ceiling((float)data.Length / ChunkSize);
            for (int i = 0; i < data.Length; i += ChunkSize)
            {
                var chunk = new ChunkedPacket(id, i == 0, data.Skip(i).Take(ChunkSize).ToArray(), i + ChunkSize >= data.Length, index, total);
                ToSend.Enqueue(new Tuple<ChunkedPacket, IConnection>(chunk, sender));
                index++;
            }
        }
        public ChunkedPacket(uint id, bool first, byte[] data, bool finished, uint index, uint totalChunks)
        {
            Id = id;
            this.first = first;
            Data = data;
            this.finished = finished;
            Index = index;
            TotalChunks = totalChunks;
        }

        public void Decode(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream);
            first = reader.ReadBoolean();
            Id = reader.ReadUInt32();
            Index = reader.ReadUInt32();
            TotalChunks = reader.ReadUInt32();
            var length = reader.ReadInt32();
            Data = reader.ReadBytes(length);
            finished = reader.ReadBoolean();
        }

        public void Encode(Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(first);
            writer.Write(Id);
            writer.Write(Index);
            writer.Write(TotalChunks);
            writer.Write(Data.Length);
            writer.Write(Data);
            writer.Write(finished);
        }

        public void Handle(IConnection? connection, InfoConnection? routedFrom = null)
        {
            var from = connection?.UniversalId;
            var cache = from == null ? HostChunkedPacketCache : ChunkedPacketCache[from.Value];
            if (first)
            {
                cache[Id] = new ChunkCacheData(TotalChunks);
            }
            if (!cache.ContainsKey(Id))
            {
                Shapez2Multiplayer.logger.Error.Log("Chunked packet received with id " + Id + " but no cache entry was found.");
                return;
            }
            var cacheData = cache[Id];
            if (Index != cacheData.Index+1)
            {
                Shapez2Multiplayer.logger.Error.Log("Chunked packet received out of order with id " + Id + " but index " + Index + " was received instead of " + (cacheData.Index+1) + ". Discarding Entire Chunked Packet.");
                cacheData.Stream.Dispose();
                cache.Remove(Id);
                return;
            }
            cacheData.Index = Index;
            cacheData.Stream.Write(Data);
            Data = null;
            if (MultiplayerCore.Client && !MultiplayerCore.connectionManager.FinishedConnecting && MultiplayerCore.ConnectingDialog != null)
            {
                MultiplayerCore.ConnectingDialog.Init("multiplayer.connecting-dialog.title".T(), new CombinedText("multiplayer.connecting-dialog.description".T(), new RawText("\n"), "multiplayer.connecting-dialog.recieving-data".T(), new RawText($" {cacheData.Index}/{cacheData.TotalChunks}")), "multiplayer.connecting-dialog.cancel".T());
            }
            if (!finished) return;
            cacheData.Stream.Seek(0, SeekOrigin.Begin);
            var data = cacheData.Stream.ToArray();
            cacheData.Stream.Dispose();
            cache.Remove(Id);
            MultiplayerCore.socketManager?.OnMessage(connection, data);
            MultiplayerCore.connectionManager?.OnMessage(data);
        }

        public static void Update()
        {
            SendTimer += Time.deltaTime;
            if (SendTimer >= SendDelay)
            {
                SendTimer = 0.0f;
                if (ToSend.TryDequeue(out var data))
                {
                    MultiplayerCore.socketManager?.SendTo(data.Item1, data.Item2);
                    MultiplayerCore.connectionManager?.Send(data.Item1);
                }
            }
        }
        public class ChunkCacheData
        {
            public MemoryStream Stream = new MemoryStream();
            public uint Index = 0;
            public uint TotalChunks;
            public ChunkCacheData(uint totalChunks)
            {
                TotalChunks = totalChunks;
            }
        }
    }
}
