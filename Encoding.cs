using Core.Collections;
using Core.Collections.Scoped;
using Core.Localization;
using Game.Core.Coordinates;
using Game.Core.Research;
using Game.Core.Serialization;
using Game.Core.Simulation;
using Game.Core.Trains;
using Game.Placement.Data;
using Game.Placement.Processing;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine.UI;

namespace Shapez2Multiplayer
{
    public static class Encoding
    {
        //public static readonly Dictionary<Type, Action<object, Stream>> Encoders = new Dictionary<Type, Action<object, Stream>>()
        //{
        //    { typeof(IPlacementData), (obj, stream) => Encode((IPlacementData)obj, stream) },
        //    { typeof(ConcurrentPlacementData), (obj, stream) => Encode((ConcurrentPlacementData)obj, stream) },
        //    { typeof(FlatPlacementData), (obj, stream) => Encode((FlatPlacementData)obj, stream) },
        //    { typeof(OverlappingPlacementData), (obj, stream) => Encode((OverlappingPlacementData)obj, stream) },
        //    { typeof(BuildingPlacement), (obj, stream) => Encode((BuildingPlacement)obj, stream) },
        //    { typeof(IslandPlacement), (obj, stream) => Encode((IslandPlacement)obj, stream) },
        //    { typeof(IslandDescriptor), (obj, stream) => Encode((IslandDescriptor)obj, stream) },
        //    { typeof(BuildingDescriptor), (obj, stream) => Encode((BuildingDescriptor)obj, stream) },
        //    { typeof(GlobalTileTransform), (obj, stream) => Encode((GlobalTileTransform)obj, stream) },
        //    { typeof(IIslandConfiguration), (obj, stream) => Encode((IIslandConfiguration)obj, stream) },
        //    { typeof(GlobalChunkTransform), (obj, stream) => Encode((GlobalChunkTransform)obj, stream) },
        //    { typeof(IslandDefinition), (obj, stream) => Encode((IslandDefinition)obj, stream) },
        //    { typeof(BuildingDefinition), (obj, stream) => Encode((BuildingDefinition)obj, stream) },
        //    //{ typeof(BuildingConnectorData), (obj, stream) => Encode((BuildingConnectorData)obj, stream) },
        //    //{ typeof(LocalTilePivot), (obj, stream) => Encode((LocalTilePivot)obj, stream) },
        //    //{ typeof(TileDirection), (obj, stream) => Encode((TileDirection)obj, stream) },
        //    //{ typeof(TileDimensions), (obj, stream) => Encode((TileDimensions)obj, stream) },
        //    //{ typeof(LocalTileBounds), (obj, stream) => Encode((LocalTileBounds)obj, stream) },
        //    { typeof(IslandDefinitionId), (obj, stream) => Encode((IslandDefinitionId)obj, stream) },
        //    { typeof(BuildingDefinitionId), (obj, stream) => Encode((BuildingDefinitionId)obj, stream) },
        //    { typeof(IslandChunkData), (obj, stream) => Encode((IslandChunkData)obj, stream) },
        //    { typeof(ChunkDirection), (obj, stream) => Encode((ChunkDirection)obj, stream) },
        //    { typeof(NotchDefinition), (obj, stream) => Encode((NotchDefinition)obj, stream) },
        //    { typeof(GridRotation), (obj, stream) => Encode((GridRotation)obj, stream) }
        //};
        //public static readonly Dictionary<Type, Func<Stream, object>> Decoders = new Dictionary<Type, Func<Stream, object>>()
        //{
        //    { typeof(IPlacementData), stream => DecodePlacementData(stream) },
        //    { typeof(ConcurrentPlacementData), stream => DecodeConcurrentPlacementData(stream) },
        //    { typeof(FlatPlacementData), stream => DecodeFlatPlacementData(stream) },
        //    { typeof(OverlappingPlacementData), stream => DecodeOverlappingPlacementData(stream) },
        //    { typeof(BuildingPlacement), stream => DecodeBuildingPlacement(stream) },
        //    { typeof(IslandPlacement), stream => DecodeIslandPlacement(stream) },
        //    { typeof(IslandDescriptor), stream => DecodeIslandDescriptor(stream) },
        //    { typeof(BuildingDescriptor), stream => DecodeBuildingDescriptor(stream) },
        //    { typeof(GlobalTileTransform), stream => DecodeGlobalTileTransform(stream) },
        //    { typeof(IIslandConfiguration), stream => DecodeIIslandConfiguration(stream) },
        //    { typeof(GlobalChunkTransform), stream => DecodeGlobalChunkTransform(stream) },
        //    { typeof(IslandDefinition), stream => DecodeIslandDefinition(stream) },
        //    { typeof(BuildingDefinition), stream => DecodeBuildingDefinition(stream) },
        //    //{ typeof(BuildingConnectorData), stream => DecodeBuildingConnectorData(stream) },
        //    //{ typeof(LocalTilePivot), stream => DecodeLocalTilePivot(stream) },
        //    //{ typeof(TileDirection), stream => DecodeTileDirection(stream) },
        //    //{ typeof(TileDimensions), stream => DecodeTileDimensions(stream) },
        //    //{ typeof(LocalTileBounds), stream => DecodeLocalTileBounds(stream) },
        //    { typeof(IslandDefinitionId), stream => DecodeIslandDefinitionId(stream) },
        //    { typeof(BuildingDefinitionId), stream => DecodeBuildingDefinitionId(stream) },
        //    { typeof(IslandChunkData), stream => DecodeIslandChunkData(stream) },
        //    { typeof(ChunkDirection), stream => DecodeChunkDirection(stream) },
        //    { typeof(NotchDefinition), stream => DecodeNotchDefinition(stream) },
        //    { typeof(GridRotation), stream => DecodeGridRotation(stream) }
        //};
        public static readonly Dictionary<GlobalTileCoordinate, BuildingId> DeletedBuildingIds = new Dictionary<GlobalTileCoordinate, BuildingId>();
        public static readonly Dictionary<GlobalChunkCoordinate, IslandId> DeletedIslandIds = new Dictionary<GlobalChunkCoordinate, IslandId>();
        public static readonly FieldInfo ConcurrentPlacementData_ExtraBuildingsToRemovePositionsInfo = AccessTools.Field(typeof(ConcurrentPlacementData), "_ExtraBuildingsToRemovePositions");
        public static readonly FieldInfo ConcurrentPlacementData_ExtraIslandsToRemovePositionsInfo = AccessTools.Field(typeof(ConcurrentPlacementData), "_ExtraIslandsToRemovePositions");
        public static readonly FieldInfo ConcurrentPlacementData_IslandsMapInfo = AccessTools.Field(typeof(ConcurrentPlacementData), "_IslandsMap");
        public static readonly FieldInfo ConcurrentPlacementData_BuildingsMapInfo = AccessTools.Field(typeof(ConcurrentPlacementData), "_BuildingsMap");
        public static readonly FieldInfo FlatPlacementData_ExtraBuildingsToRemovePositionsInfo = AccessTools.Field(typeof(FlatPlacementData), "_ExtraBuildingsToRemovePositions");
        public static readonly FieldInfo FlatPlacementData_ExtraIslandsToRemovePositionsInfo = AccessTools.Field(typeof(FlatPlacementData), "_ExtraIslandsToRemovePositions");
        public static readonly FieldInfo FlatPlacementData_Buildings = AccessTools.Field(typeof(FlatPlacementData), "_Buildings");
        public static readonly FieldInfo FlatPlacementData_Islands = AccessTools.Field(typeof(FlatPlacementData), "_Islands");
        public static readonly FieldInfo FlatPlacementDataBuildingIndexMap = AccessTools.Field(typeof(FlatPlacementData), "BuildingIndexMap");
        public static readonly FieldInfo FlatPlacementDataIslandIndexMap = AccessTools.Field(typeof(FlatPlacementData), "IslandIndexMap");
        public static readonly FieldInfo OverlappingPlacementData_ExtraBuildingsToRemovePositionsInfo = AccessTools.Field(typeof(OverlappingPlacementData), "_ExtraBuildingsToRemovePositions");
        public static readonly FieldInfo OverlappingPlacementData_ExtraIslandsToRemovePositionsInfo = AccessTools.Field(typeof(OverlappingPlacementData), "_ExtraIslandsToRemovePositions");
        public static readonly FieldInfo OverlappingPlacementData_IslandsMapInfo = AccessTools.Field(typeof(OverlappingPlacementData), "_IslandsMap");
        public static readonly FieldInfo OverlappingPlacementData_BuildingsMapInfo = AccessTools.Field(typeof(OverlappingPlacementData), "_BuildingsMap");
        //public static readonly BinaryFormatter bf = new BinaryFormatter(); really inefficient in packet size
        public static ISerializationVisitor serializationVisitor;
        public static readonly FieldInfo PlacementInputHolderInputInfo = AccessTools.Field(typeof(PlacementInputHolder), "Input");
        public static void Encode(ChunkVector chunkVector, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(chunkVector.x);
            writer.Write(chunkVector.y);
            writer.Write(chunkVector.z);
        }
        public static ChunkVector DecodeChunkVector(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            return new ChunkVector(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt16());
        }
        public static void Encode(ChunkLimitCurrency chunkLimitCurrency, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(chunkLimitCurrency.Amount);
        }
        public static ChunkLimitCurrency DecodeChunkLimitCurrency(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            return new ChunkLimitCurrency(reader.ReadInt32());
        }
        public static void Encode(ResearchUpgradeId researchUpgradeId, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(researchUpgradeId.Id);
        }
        public static ResearchUpgradeId DecodeResearchUpgradeId(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            return new ResearchUpgradeId(reader.ReadString());
        }
        public static void Encode(ResearchLinearUpgradeId researchLinearUpgradeId, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(researchLinearUpgradeId.Id);
        }
        public static ResearchLinearUpgradeId DecodeResearchLinearUpgradeId(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            return new ResearchLinearUpgradeId(reader.ReadString());
        }
        public static void Encode(BlueprintCurrency blueprintCurrency, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(blueprintCurrency.TotalSub);
        }
        public static readonly ConstructorInfo BlueprintCurrencyConstructorInfo = AccessTools.Constructor(typeof(BlueprintCurrency), new Type[] { typeof(long) });
        public static BlueprintCurrency DecodeBlueprintCurrency(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            return (BlueprintCurrency)BlueprintCurrencyConstructorInfo.Invoke(new object[] { reader.ReadInt64() });
        }
        public static void Encode(GlobalTileCoordinate globalTileCoordinate, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(globalTileCoordinate.x);
            writer.Write(globalTileCoordinate.y);
            writer.Write(globalTileCoordinate.z);
        }
        public static GlobalTileCoordinate DecodeGlobalTileCoordinate(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            return new GlobalTileCoordinate(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt16());
        }
        public static void Encode(IslandTileCoordinate islandTileCoordinate, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(islandTileCoordinate.x);
            writer.Write(islandTileCoordinate.y);
            writer.Write(islandTileCoordinate.z);
        }
        public static IslandTileCoordinate DecodeIslandTileCoordinate(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            return new IslandTileCoordinate(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
        }
        public static void Encode(GlobalChunkCoordinate globalChunkCoordinate, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(globalChunkCoordinate.x);
            writer.Write(globalChunkCoordinate.y);
            writer.Write(globalChunkCoordinate.z);
        }
        public static GlobalChunkCoordinate DecodeGlobalChunkCoordinate(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            return new GlobalChunkCoordinate(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt16());
        }
        public static readonly FieldInfo HUDBuildingMassSelectionAreaSelectionEnd_GInfo = AccessTools.Field(typeof(HUDBuildingMassSelection), "AreaSelectionEnd_G");
        public static readonly FieldInfo HUDBuildingMassSelectionAreaSelectionStart_GInfo = AccessTools.Field(typeof(HUDBuildingMassSelection), "AreaSelectionStart_G");
        public static readonly FieldInfo HUDBuildingMassSelectionAreaCurrentModeInfo = AccessTools.Field(typeof(HUDBuildingMassSelection), "CurrentMode");
        public static readonly FieldInfo HUDBuildingMassSelectionAreaPendingSelectionInfo = AccessTools.Field(typeof(HUDBuildingMassSelection), "PendingSelection");
        public static readonly FieldInfo HUDBuildingMassSelectionAreaHoverAnimationsInfo = AccessTools.Field(typeof(HUDBuildingMassSelection), "HoverAnimations");
        public static void Encode(HUDBuildingMassSelection hudBuildingMassSelection, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var areaSelectionEnd_G = (GlobalTileCoordinate?)HUDBuildingMassSelectionAreaSelectionEnd_GInfo.GetValue(hudBuildingMassSelection);
            writer.Write(areaSelectionEnd_G.HasValue);
            if (areaSelectionEnd_G.HasValue) Encode(areaSelectionEnd_G.Value, stream);
            var areaSelectionStart_G = (GlobalTileCoordinate?)HUDBuildingMassSelectionAreaSelectionStart_GInfo.GetValue(hudBuildingMassSelection);
            writer.Write(areaSelectionStart_G.HasValue);
            if (areaSelectionStart_G.HasValue) Encode(areaSelectionStart_G.Value, stream);
            Encode((HUDMassSelectionMode)HUDBuildingMassSelectionAreaCurrentModeInfo.GetValue(hudBuildingMassSelection), stream);
            var pendingSelection = (HashSet<BuildingModel>)HUDBuildingMassSelectionAreaPendingSelectionInfo.GetValue(hudBuildingMassSelection);
            writer.Write(pendingSelection.Count);
            foreach (var selectable in pendingSelection)
            {
                Encode(selectable.Tile_G, stream);
            }
            //var hoverAnimations = (IList)HUDBuildingMassSelectionAreaHoverAnimationsInfo.GetValue(hudBuildingMassSelection);
            //writer.Write(hoverAnimations.Count);
            //FieldInfo hoverAnimTarget = null;
            //FieldInfo hoverAnimLastHoverTime = null;
            //FieldInfo hoverAnimInitialHoverTime = null;
            //foreach (var anim in hoverAnimations)
            //{
            //    if (hoverAnimTarget == null)
            //    {
            //        hoverAnimTarget = AccessTools.Field(anim.GetType(), "Target");
            //        hoverAnimLastHoverTime = AccessTools.Field(anim.GetType(), "LastHoverTime");
            //        hoverAnimInitialHoverTime = AccessTools.Field(anim.GetType(), "InitialHoverTime");
            //    }
            //    Encode(((BuildingModel)hoverAnimTarget.GetValue(anim)).Tile_G);
            //    writer.Write((float)hoverAnimLastHoverTime.GetValue(anim));
            //    writer.Write((float)hoverAnimInitialHoverTime.GetValue(anim));
            //}
        }
        public static HUDBuildingMassSelection DecodeHUDBuildingMassSelection(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var hudBuildingMassSelection = new HUDBuildingMassSelection();
            if (reader.ReadBoolean()) HUDBuildingMassSelectionAreaSelectionEnd_GInfo.SetValue(hudBuildingMassSelection, DecodeGlobalTileCoordinate(stream));
            if (reader.ReadBoolean()) HUDBuildingMassSelectionAreaSelectionStart_GInfo.SetValue(hudBuildingMassSelection, DecodeGlobalTileCoordinate(stream));
            HUDBuildingMassSelectionAreaCurrentModeInfo.SetValue(hudBuildingMassSelection, DecodeHUDMassSelectionMode(stream));
            var pendingSelection = (HashSet<BuildingModel>)HUDBuildingMassSelectionAreaPendingSelectionInfo.GetValue(hudBuildingMassSelection);
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                if (Shapez2Multiplayer.MapModel.TryGetBuilding(DecodeGlobalTileCoordinate(stream), out BuildingModel building))
                {
                    pendingSelection.Add(building);
                }
            }
            //var hoverAnimations = (IList)HUDBuildingMassSelectionAreaHoverAnimationsInfo.GetValue(hudBuildingMassSelection);
            //count = reader.ReadInt32();
            //var hoverAnimType = typeof(HUDMassSelectionBase<BuildingModel, GlobalTileCoordinate>).GetNestedType("HoverAnimation", BindingFlags.NonPublic).MakeGenericType(new Type[] { typeof(BuildingModel), typeof(GlobalTileCoordinate) });
            //FieldInfo hoverAnimTarget = null;
            //FieldInfo hoverAnimLastHoverTime = null;
            //FieldInfo hoverAnimInitialHoverTime = null;
            //for (int i = 0; i < count; i++)
            //{
            //    if (Shapez2Multiplayer.MapModel.TryGetBuilding((GlobalTileCoordinate)DecodeGlobalTileCoordinate(stream), out BuildingModel building))
            //    {
            //        object anim = Activator.CreateInstance(hoverAnimType);
            //        if (hoverAnimTarget == null)
            //        {
            //            hoverAnimTarget = AccessTools.Field(anim.GetType(), "Target");
            //            hoverAnimLastHoverTime = AccessTools.Field(anim.GetType(), "LastHoverTime");
            //            hoverAnimInitialHoverTime = AccessTools.Field(anim.GetType(), "InitialHoverTime");
            //        }
            //        hoverAnimTarget.SetValue(anim, building);
            //        hoverAnimLastHoverTime.SetValue(anim, reader.ReadSingle());
            //        hoverAnimInitialHoverTime.SetValue(anim, reader.ReadSingle());
            //    }
            //}
            return hudBuildingMassSelection;
        }
        public static readonly FieldInfo HUDIslandMassSelectionAreaSelectionEnd_GInfo = AccessTools.Field(typeof(HUDIslandMassSelection), "AreaSelectionEnd_G");
        public static readonly FieldInfo HUDIslandMassSelectionAreaSelectionStart_GInfo = AccessTools.Field(typeof(HUDIslandMassSelection), "AreaSelectionStart_G");
        public static readonly FieldInfo HUDIslandMassSelectionAreaCurrentModeInfo = AccessTools.Field(typeof(HUDIslandMassSelection), "CurrentMode");
        public static readonly FieldInfo HUDIslandMassSelectionAreaPendingSelectionInfo = AccessTools.Field(typeof(HUDIslandMassSelection), "PendingSelection");
        public static readonly FieldInfo HUDIslandMassSelectionAreaHoverAnimationsInfo = AccessTools.Field(typeof(HUDIslandMassSelection), "HoverAnimations");
        public static void Encode(HUDIslandMassSelection hudIslandMassSelection, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var areaSelectionEnd_G = (GlobalChunkCoordinate?)HUDIslandMassSelectionAreaSelectionEnd_GInfo.GetValue(hudIslandMassSelection);
            writer.Write(areaSelectionEnd_G.HasValue);
            if (areaSelectionEnd_G.HasValue) Encode(areaSelectionEnd_G.Value, stream);
            var areaSelectionStart_G = (GlobalChunkCoordinate?)HUDIslandMassSelectionAreaSelectionStart_GInfo.GetValue(hudIslandMassSelection);
            writer.Write(areaSelectionStart_G.HasValue);
            if (areaSelectionStart_G.HasValue) Encode(areaSelectionStart_G.Value, stream);
            Encode((HUDMassSelectionMode)HUDIslandMassSelectionAreaCurrentModeInfo.GetValue(hudIslandMassSelection), stream);
            var pendingSelection = (HashSet<IslandModel>)HUDIslandMassSelectionAreaPendingSelectionInfo.GetValue(hudIslandMassSelection);
            writer.Write(pendingSelection.Count);
            foreach (var selectable in pendingSelection)
            {
                Encode(selectable.Position, stream);
            }
            //var hoverAnimations = (IList)HUDIslandMassSelectionAreaHoverAnimationsInfo.GetValue(hudIslandMassSelection);
            //writer.Write(hoverAnimations.Count);
            //FieldInfo hoverAnimTarget = null;
            //FieldInfo hoverAnimLastHoverTime = null;
            //FieldInfo hoverAnimInitialHoverTime = null;
            //foreach (var anim in hoverAnimations)
            //{
            //    if (hoverAnimTarget == null)
            //    {
            //        hoverAnimTarget = AccessTools.Field(anim.GetType(), "Target");
            //        hoverAnimLastHoverTime = AccessTools.Field(anim.GetType(), "LastHoverTime");
            //        hoverAnimInitialHoverTime = AccessTools.Field(anim.GetType(), "InitialHoverTime");
            //    }
            //    Encode(((IslandModel)hoverAnimTarget.GetValue(anim)).Position);
            //    writer.Write((float)hoverAnimLastHoverTime.GetValue(anim));
            //    writer.Write((float)hoverAnimInitialHoverTime.GetValue(anim));
            //}
        }
        public static HUDIslandMassSelection DecodeHUDIslandMassSelection(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var hudIslandMassSelection = new HUDIslandMassSelection();
            if (reader.ReadBoolean()) HUDIslandMassSelectionAreaSelectionEnd_GInfo.SetValue(hudIslandMassSelection, DecodeGlobalChunkCoordinate(stream));
            if (reader.ReadBoolean()) HUDIslandMassSelectionAreaSelectionStart_GInfo.SetValue(hudIslandMassSelection, DecodeGlobalChunkCoordinate(stream));
            HUDIslandMassSelectionAreaCurrentModeInfo.SetValue(hudIslandMassSelection, DecodeHUDMassSelectionMode(stream));
            var pendingSelection = (HashSet<IslandModel>)HUDIslandMassSelectionAreaPendingSelectionInfo.GetValue(hudIslandMassSelection);
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                if (Shapez2Multiplayer.MapModel.TryGetIsland(DecodeGlobalChunkCoordinate(stream), out IslandModel island))
                {
                    pendingSelection.Add(island);
                }
            }
            //var hoverAnimations = (IList)HUDIslandMassSelectionAreaHoverAnimationsInfo.GetValue(hudIslandMassSelection);
            //count = reader.ReadInt32();
            //var hoverAnimType = typeof(HUDMassSelectionBase<IslandModel, GlobalChunkCoordinate>).GetNestedType("HoverAnimation", BindingFlags.NonPublic).MakeGenericType(new Type[] { typeof(IslandModel), typeof(GlobalChunkCoordinate) });
            //FieldInfo hoverAnimTarget = null;
            //FieldInfo hoverAnimLastHoverTime = null;
            //FieldInfo hoverAnimInitialHoverTime = null;
            //for (int i = 0; i < count; i++)
            //{
            //    if (Shapez2Multiplayer.MapModel.TryGetIsland((GlobalChunkCoordinate)DecodeGlobalChunkCoordinate(stream), out IslandModel island))
            //    {
            //        object anim = Activator.CreateInstance(hoverAnimType);
            //        if (hoverAnimTarget == null)
            //        {
            //            hoverAnimTarget = AccessTools.Field(anim.GetType(), "Target");
            //            hoverAnimLastHoverTime = AccessTools.Field(anim.GetType(), "LastHoverTime");
            //            hoverAnimInitialHoverTime = AccessTools.Field(anim.GetType(), "InitialHoverTime");
            //        }
            //        hoverAnimTarget.SetValue(anim, island);
            //        hoverAnimLastHoverTime.SetValue(anim, reader.ReadSingle());
            //        hoverAnimInitialHoverTime.SetValue(anim, reader.ReadSingle());
            //    }
            //}
            return hudIslandMassSelection;
        }
        public static void Encode(HUDMassSelectionMode hudMassSelectionMode, Stream stream)
        {
            stream.WriteByte((byte)hudMassSelectionMode);
        }
        public static HUDMassSelectionMode DecodeHUDMassSelectionMode(Stream stream)
        {
            return (HUDMassSelectionMode)stream.ReadByte();
        }
        public static void Encode(PlacementInputHolder placementInputHolder, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var placementInput = (IPlacementInput)PlacementInputHolderInputInfo.GetValue(placementInputHolder);
            writer.Write(placementInput != null);
            if (placementInput != null) Encode(placementInput, stream);
        }
        public static PlacementInputHolder DecodePlacementInputHolder(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            IPlacementInput? placementInput = null;
            if (reader.ReadBoolean()) placementInput = DecodePlacementInput(stream);
            var placementInputHolder = new PlacementInputHolder(); // don't use constructor to allow input to be null
            object boxed = placementInputHolder;
            PlacementInputHolderInputInfo.SetValue(boxed, placementInput);
            return (PlacementInputHolder)boxed;
        }
        public static void Encode(IPlacementInput placementInput, Stream stream)
        {
            if (placementInput is AreaPlacementInput<GlobalTileTransform, GlobalTileCoordinate> areaPlacementInputTile)
            {
                stream.WriteByte((byte)PlacementInputType.AreaPlacementInputTile);
                Encode(areaPlacementInputTile, stream);
            }
            else if (placementInput is AreaPlacementInput<GlobalChunkTransform, GlobalChunkCoordinate> areaPlacementInputChunk)
            {
                stream.WriteByte((byte)PlacementInputType.AreaPlacementInputChunk);
                Encode(areaPlacementInputChunk, stream);
            }
            else if (placementInput is BlueprintPlacementInput<GlobalTileCoordinate> blueprintPlacementInputTile)
            {
                stream.WriteByte((byte)PlacementInputType.BlueprintPlacementInputTile);
                Encode(blueprintPlacementInputTile, stream);
            }
            else if (placementInput is BlueprintPlacementInput<GlobalChunkCoordinate> blueprintPlacementInputChunk)
            {
                stream.WriteByte((byte)PlacementInputType.BlueprintPlacementInputChunk);
                Encode(blueprintPlacementInputChunk, stream);
            }
            else if (placementInput is LinePlacementInput<GlobalTileCoordinate, TileDirection, GlobalTilePivot> linePlacementInputTile)
            {
                stream.WriteByte((byte)PlacementInputType.LinePlacementInputTile);
                Encode(linePlacementInputTile, stream);
            }
            else if (placementInput is LinePlacementInput<GlobalChunkCoordinate, ChunkDirection, GlobalChunkPivot> linePlacementInputChunk)
            {
                stream.WriteByte((byte)PlacementInputType.LinePlacementInputChunk);
                Encode(linePlacementInputChunk, stream);
            }
            else if (placementInput is PathPlacementInput<GlobalTileCoordinate, TileDirection, GlobalTilePivot> pathPlacementInputTile)
            {
                stream.WriteByte((byte)PlacementInputType.PathPlacementInputTile);
                Encode(pathPlacementInputTile, stream);
            }
            else if (placementInput is PathPlacementInput<GlobalChunkCoordinate, ChunkDirection, GlobalChunkPivot> pathPlacementInputChunk)
            {
                stream.WriteByte((byte)PlacementInputType.PathPlacementInputChunk);
                Encode(pathPlacementInputChunk, stream);
            }
            else if (placementInput is SinglePlacementInput<GlobalTileCoordinate, TileDirection, GlobalTilePivot> singlePlacementInputTile)
            {
                stream.WriteByte((byte)PlacementInputType.SinglePlacementInputTile);
                Encode(singlePlacementInputTile, stream);
            }
            else if (placementInput is SinglePlacementInput<GlobalChunkCoordinate, ChunkDirection, GlobalChunkPivot> singlePlacementInputChunk)
            {
                stream.WriteByte((byte)PlacementInputType.SinglePlacementInputChunk);
                Encode(singlePlacementInputChunk, stream);
            }
            else if (placementInput is VariablePairPlacementInput<GlobalTileCoordinate, TileDirection, GlobalTilePivot> variablePairPlacementInputTile)
            {
                stream.WriteByte((byte)PlacementInputType.VariablePairPlacementInputTile);
                Encode(variablePairPlacementInputTile, stream);
            }
            else if (placementInput is VariablePairPlacementInput<GlobalChunkCoordinate, ChunkDirection, GlobalChunkPivot> variablePairPlacementInputChunk)
            {
                stream.WriteByte((byte)PlacementInputType.VariablePairPlacementInputChunk);
                Encode(variablePairPlacementInputChunk, stream);
            }
            else
            {
                Shapez2Multiplayer.logger.Error.Log($"Unknown Placement Input Type: {placementInput.GetType().GetGenericFriendlyName()}");
            }
        }
        public static IPlacementInput? DecodePlacementInput(Stream stream)
        {
            switch ((PlacementInputType)stream.ReadByte())
            {
                case PlacementInputType.AreaPlacementInputTile:
                    return DecodeAreaPlacementInputTile(stream);
                case PlacementInputType.AreaPlacementInputChunk:
                    return DecodeAreaPlacementInputChunk(stream);
                case PlacementInputType.BlueprintPlacementInputTile:
                    return DecodeBlueprintPlacementInputTile(stream);
                case PlacementInputType.BlueprintPlacementInputChunk:
                    return DecodeBlueprintPlacementInputChunk(stream);
                case PlacementInputType.LinePlacementInputTile:
                    return DecodeLinePlacementInputTile(stream);
                case PlacementInputType.LinePlacementInputChunk:
                    return DecodeLinePlacementInputChunk(stream);
                case PlacementInputType.PathPlacementInputTile:
                    return DecodePathPlacementInputTile(stream);
                case PlacementInputType.PathPlacementInputChunk:
                    return DecodePathPlacementInputChunk(stream);
                case PlacementInputType.SinglePlacementInputTile:
                    return DecodeSinglePlacementInputTile(stream);
                case PlacementInputType.SinglePlacementInputChunk:
                    return DecodeSinglePlacementInputChunk(stream);
                case PlacementInputType.VariablePairPlacementInputTile:
                    return DecodeVariablePairPlacementInputTile(stream);
                case PlacementInputType.VariablePairPlacementInputChunk:
                    return DecodeVariablePairPlacementInputChunk(stream);
                default:
                    Shapez2Multiplayer.logger.Error.Log("Tried to decode unknown player action");
                    return null;
            }
        }
        public static void Encode(AreaPlacementInput<GlobalTileTransform, GlobalTileCoordinate> areaPlacementInput, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(areaPlacementInput.Confirmed);
            writer.Write(areaPlacementInput.Cancelled);
            Encode(areaPlacementInput.StartTransform, stream);
            Encode(areaPlacementInput.EndTransform, stream);
            writer.Write(areaPlacementInput.StartedPlacement);
            Encode(areaPlacementInput.PreferredRotation, stream);
            writer.Write(areaPlacementInput.StartedDragging);
            writer.Write(areaPlacementInput.IsFlipped);
            writer.Write(areaPlacementInput.UseForce);
        }
        public static AreaPlacementInput<GlobalTileTransform, GlobalTileCoordinate> DecodeAreaPlacementInputTile(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var confirmed = reader.ReadBoolean();
            var cancelled = reader.ReadBoolean();
            var startTransform = DecodeGlobalTileTransform(stream);
            var endTransform = DecodeGlobalTileTransform(stream);
            var startedPlacement = reader.ReadBoolean();
            var preferredRotation = DecodeGridRotation(stream);
            var startedDragging = reader.ReadBoolean();
            var isFlipped = reader.ReadBoolean();
            var useForce = reader.ReadBoolean();
            var areaPlacementInput = new AreaPlacementInput<GlobalTileTransform, GlobalTileCoordinate>(preferredRotation, isFlipped);
            areaPlacementInput.Confirmed = confirmed;
            areaPlacementInput.Cancelled = cancelled;
            areaPlacementInput.StartTransform = startTransform;
            areaPlacementInput.EndTransform = endTransform;
            areaPlacementInput.StartedPlacement = startedPlacement;
            areaPlacementInput.StartedDragging = startedDragging;
            areaPlacementInput.UseForce = useForce;
            return areaPlacementInput;
        }
        public static void Encode(AreaPlacementInput<GlobalChunkTransform, GlobalChunkCoordinate> areaPlacementInput, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(areaPlacementInput.Confirmed);
            writer.Write(areaPlacementInput.Cancelled);
            Encode(areaPlacementInput.StartTransform, stream);
            Encode(areaPlacementInput.EndTransform, stream);
            writer.Write(areaPlacementInput.StartedPlacement);
            Encode(areaPlacementInput.PreferredRotation, stream);
            writer.Write(areaPlacementInput.StartedDragging);
            writer.Write(areaPlacementInput.IsFlipped);
            writer.Write(areaPlacementInput.UseForce);
        }
        public static AreaPlacementInput<GlobalChunkTransform, GlobalChunkCoordinate> DecodeAreaPlacementInputChunk(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var confirmed = reader.ReadBoolean();
            var cancelled = reader.ReadBoolean();
            var startTransform = DecodeGlobalChunkTransform(stream);
            var endTransform = DecodeGlobalChunkTransform(stream);
            var startedPlacement = reader.ReadBoolean();
            var preferredRotation = DecodeGridRotation(stream);
            var startedDragging = reader.ReadBoolean();
            var isFlipped = reader.ReadBoolean();
            var useForce = reader.ReadBoolean();
            var areaPlacementInput = new AreaPlacementInput<GlobalChunkTransform, GlobalChunkCoordinate>(preferredRotation, isFlipped);
            areaPlacementInput.Confirmed = confirmed;
            areaPlacementInput.Cancelled = cancelled;
            areaPlacementInput.StartTransform = startTransform;
            areaPlacementInput.EndTransform = endTransform;
            areaPlacementInput.StartedPlacement = startedPlacement;
            areaPlacementInput.StartedDragging = startedDragging;
            areaPlacementInput.UseForce = useForce;
            return areaPlacementInput;
        }
        public static void Encode(BlueprintPlacementInput<GlobalTileCoordinate> blueprintPlacementInput, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(blueprintPlacementInput.IsFlipped);
            writer.Write(blueprintPlacementInput.StartedPlacement);
            writer.Write(blueprintPlacementInput.Confirmed);
            writer.Write(blueprintPlacementInput.Cancelled);
            Encode(blueprintPlacementInput.StartPosition, stream);
            Encode(blueprintPlacementInput.EndPosition, stream);
            Encode(blueprintPlacementInput.Rotation, stream);
            writer.Write(blueprintPlacementInput.MirroredAcrossPrimaryAxis);
            writer.Write(blueprintPlacementInput.MirroredAcrossSecondaryAxis);
            writer.Write(blueprintPlacementInput.StartedDragging);
            writer.Write(blueprintPlacementInput.UseForce);
        }
        public static BlueprintPlacementInput<GlobalTileCoordinate> DecodeBlueprintPlacementInputTile(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var isFlipped = reader.ReadBoolean();
            var startedPlacement = reader.ReadBoolean();
            var confirmed = reader.ReadBoolean();
            var cancelled = reader.ReadBoolean();
            var startPosition = DecodeGlobalTileCoordinate(stream);
            var endPosition = DecodeGlobalTileCoordinate(stream);
            var rotation = DecodeGridRotation(stream);
            var mirroredAcrossPrimaryAxis = reader.ReadBoolean();
            var mirroredAcrossSecondaryAxis = reader.ReadBoolean();
            var startedDragging = reader.ReadBoolean();
            var useForce = reader.ReadBoolean();
            var blueprintPlacementInput = new BlueprintPlacementInput<GlobalTileCoordinate>(rotation, isFlipped);
            blueprintPlacementInput.StartedPlacement = startedPlacement;
            blueprintPlacementInput.Confirmed = confirmed;
            blueprintPlacementInput.Cancelled = cancelled;
            blueprintPlacementInput.StartPosition = startPosition;
            blueprintPlacementInput.EndPosition = endPosition;
            blueprintPlacementInput.MirroredAcrossPrimaryAxis = mirroredAcrossPrimaryAxis;
            blueprintPlacementInput.MirroredAcrossSecondaryAxis = mirroredAcrossSecondaryAxis;
            blueprintPlacementInput.StartedDragging = startedDragging;
            blueprintPlacementInput.UseForce = useForce;
            return blueprintPlacementInput;
        }
        public static void Encode(BlueprintPlacementInput<GlobalChunkCoordinate> blueprintPlacementInput, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(blueprintPlacementInput.IsFlipped);
            writer.Write(blueprintPlacementInput.StartedPlacement);
            writer.Write(blueprintPlacementInput.Confirmed);
            writer.Write(blueprintPlacementInput.Cancelled);
            Encode(blueprintPlacementInput.StartPosition, stream);
            Encode(blueprintPlacementInput.EndPosition, stream);
            Encode(blueprintPlacementInput.Rotation, stream);
            writer.Write(blueprintPlacementInput.MirroredAcrossPrimaryAxis);
            writer.Write(blueprintPlacementInput.MirroredAcrossSecondaryAxis);
            writer.Write(blueprintPlacementInput.StartedDragging);
            writer.Write(blueprintPlacementInput.UseForce);
        }
        public static BlueprintPlacementInput<GlobalChunkCoordinate> DecodeBlueprintPlacementInputChunk(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var isFlipped = reader.ReadBoolean();
            var startedPlacement = reader.ReadBoolean();
            var confirmed = reader.ReadBoolean();
            var cancelled = reader.ReadBoolean();
            var startPosition = DecodeGlobalChunkCoordinate(stream);
            var endPosition = DecodeGlobalChunkCoordinate(stream);
            var rotation = DecodeGridRotation(stream);
            var mirroredAcrossPrimaryAxis = reader.ReadBoolean();
            var mirroredAcrossSecondaryAxis = reader.ReadBoolean();
            var startedDragging = reader.ReadBoolean();
            var useForce = reader.ReadBoolean();
            var blueprintPlacementInput = new BlueprintPlacementInput<GlobalChunkCoordinate>(rotation, isFlipped);
            blueprintPlacementInput.StartedPlacement = startedPlacement;
            blueprintPlacementInput.Confirmed = confirmed;
            blueprintPlacementInput.Cancelled = cancelled;
            blueprintPlacementInput.StartPosition = startPosition;
            blueprintPlacementInput.EndPosition = endPosition;
            blueprintPlacementInput.MirroredAcrossPrimaryAxis = mirroredAcrossPrimaryAxis;
            blueprintPlacementInput.MirroredAcrossSecondaryAxis = mirroredAcrossSecondaryAxis;
            blueprintPlacementInput.StartedDragging = startedDragging;
            blueprintPlacementInput.UseForce = useForce;
            return blueprintPlacementInput;
        }
        public static void Encode(LinePlacementInput<GlobalTileCoordinate, TileDirection, GlobalTilePivot> linePlacementInput, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(linePlacementInput.Confirmed);
            writer.Write(linePlacementInput.Cancelled);
            writer.Write(linePlacementInput.StartedDragging);
            writer.Write(linePlacementInput.IsFlipped);
            writer.Write(linePlacementInput.UseForce);
            Encode(linePlacementInput.StartPivot, stream);
            Encode(linePlacementInput.EndPivot, stream);
            writer.Write(linePlacementInput.StartedPlacement);
            Encode(linePlacementInput.PreferredRotation, stream);
            writer.Write(linePlacementInput.AllowParallel);
            writer.Write(linePlacementInput.AllowPerpendicular);
        }
        public static LinePlacementInput<GlobalTileCoordinate, TileDirection, GlobalTilePivot> DecodeLinePlacementInputTile(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var confirmed = reader.ReadBoolean();
            var cancelled = reader.ReadBoolean();
            var startedDragging = reader.ReadBoolean();
            var isFlipped = reader.ReadBoolean();
            var useForce = reader.ReadBoolean();
            var startPivot = DecodeGlobalTilePivot(stream);
            var endPivot = DecodeGlobalTilePivot(stream);
            var startedPlacement = reader.ReadBoolean();
            var preferredRotation = DecodeGridRotation(stream);
            var allowParallel = reader.ReadBoolean();
            var allowPerpendicular = reader.ReadBoolean();
            var linePlacementInput = new LinePlacementInput<GlobalTileCoordinate, TileDirection, GlobalTilePivot>(preferredRotation, isFlipped, allowParallel, allowPerpendicular);
            linePlacementInput.Confirmed = confirmed;
            linePlacementInput.Cancelled = cancelled;
            linePlacementInput.StartedDragging = startedDragging;
            linePlacementInput.UseForce = useForce;
            linePlacementInput.StartPivot = startPivot;
            linePlacementInput.EndPivot = endPivot;
            linePlacementInput.StartedPlacement = startedPlacement;
            return linePlacementInput;
        }
        public static void Encode(LinePlacementInput<GlobalChunkCoordinate, ChunkDirection, GlobalChunkPivot> linePlacementInput, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(linePlacementInput.Confirmed);
            writer.Write(linePlacementInput.Cancelled);
            writer.Write(linePlacementInput.StartedDragging);
            writer.Write(linePlacementInput.IsFlipped);
            writer.Write(linePlacementInput.UseForce);
            Encode(linePlacementInput.StartPivot, stream);
            Encode(linePlacementInput.EndPivot, stream);
            writer.Write(linePlacementInput.StartedPlacement);
            Encode(linePlacementInput.PreferredRotation, stream);
            writer.Write(linePlacementInput.AllowParallel);
            writer.Write(linePlacementInput.AllowPerpendicular);
        }
        public static LinePlacementInput<GlobalChunkCoordinate, ChunkDirection, GlobalChunkPivot> DecodeLinePlacementInputChunk(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var confirmed = reader.ReadBoolean();
            var cancelled = reader.ReadBoolean();
            var startedDragging = reader.ReadBoolean();
            var isFlipped = reader.ReadBoolean();
            var useForce = reader.ReadBoolean();
            var startPivot = DecodeGlobalChunkPivot(stream);
            var endPivot = DecodeGlobalChunkPivot(stream);
            var startedPlacement = reader.ReadBoolean();
            var preferredRotation = DecodeGridRotation(stream);
            var allowParallel = reader.ReadBoolean();
            var allowPerpendicular = reader.ReadBoolean();
            var linePlacementInput = new LinePlacementInput<GlobalChunkCoordinate, ChunkDirection, GlobalChunkPivot>(preferredRotation, isFlipped, allowParallel, allowPerpendicular);
            linePlacementInput.Confirmed = confirmed;
            linePlacementInput.Cancelled = cancelled;
            linePlacementInput.StartedDragging = startedDragging;
            linePlacementInput.UseForce = useForce;
            linePlacementInput.StartPivot = startPivot;
            linePlacementInput.EndPivot = endPivot;
            linePlacementInput.StartedPlacement = startedPlacement;
            return linePlacementInput;
        }
        public static void Encode(PathPlacementInput<GlobalTileCoordinate, TileDirection, GlobalTilePivot> pathPlacementInput, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(pathPlacementInput.Confirmed);
            writer.Write(pathPlacementInput.Cancelled);
            writer.Write(pathPlacementInput.StartedDragging);
            writer.Write(pathPlacementInput.UseForce);
            Encode(pathPlacementInput.InitialDirection, stream);
            writer.Write(pathPlacementInput.SegmentsStack.Count);
            foreach (var segment in pathPlacementInput.SegmentsStack.Reverse())
            {
                Encode(segment, stream);
            }
        }
        public static PathPlacementInput<GlobalTileCoordinate, TileDirection, GlobalTilePivot> DecodePathPlacementInputTile(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var confirmed = reader.ReadBoolean();
            var cancelled = reader.ReadBoolean();
            var startedDragging = reader.ReadBoolean();
            var useForce = reader.ReadBoolean();
            var initialDirection = DecodeTileDirection(stream);
            var pathPlacementInput = new PathPlacementInput<GlobalTileCoordinate, TileDirection, GlobalTilePivot>(initialDirection);
            pathPlacementInput.Confirmed = confirmed;
            pathPlacementInput.Cancelled = cancelled;
            pathPlacementInput.StartedDragging = startedDragging;
            pathPlacementInput.UseForce = useForce;
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                pathPlacementInput.SegmentsStack.Push(DecodePathPlacementInputSegmentTile(stream));
            }
            return pathPlacementInput;
        }
        public static void Encode(PathPlacementInput<GlobalChunkCoordinate, ChunkDirection, GlobalChunkPivot> pathPlacementInput, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(pathPlacementInput.Confirmed);
            writer.Write(pathPlacementInput.Cancelled);
            writer.Write(pathPlacementInput.StartedDragging);
            writer.Write(pathPlacementInput.UseForce);
            Encode(pathPlacementInput.InitialDirection, stream);
            writer.Write(pathPlacementInput.SegmentsStack.Count);
            foreach (var segment in pathPlacementInput.SegmentsStack.Reverse())
            {
                Encode(segment, stream);
            }
        }
        public static PathPlacementInput<GlobalChunkCoordinate, ChunkDirection, GlobalChunkPivot> DecodePathPlacementInputChunk(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var confirmed = reader.ReadBoolean();
            var cancelled = reader.ReadBoolean();
            var startedDragging = reader.ReadBoolean();
            var useForce = reader.ReadBoolean();
            var initialDirection = DecodeChunkDirection(stream);
            var pathPlacementInput = new PathPlacementInput<GlobalChunkCoordinate, ChunkDirection, GlobalChunkPivot>(initialDirection);
            pathPlacementInput.Confirmed = confirmed;
            pathPlacementInput.Cancelled = cancelled;
            pathPlacementInput.StartedDragging = startedDragging;
            pathPlacementInput.UseForce = useForce;
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                pathPlacementInput.SegmentsStack.Push(DecodePathPlacementInputSegmentChunk(stream));
            }
            return pathPlacementInput;
        }
        public static void Encode(SinglePlacementInput<GlobalTileCoordinate, TileDirection, GlobalTilePivot> singlePlacementInput, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            Encode(singlePlacementInput.EndPivot, stream);
            writer.Write(singlePlacementInput.Confirmed);
            writer.Write(singlePlacementInput.Cancelled);
            writer.Write(singlePlacementInput.StartedDragging);
            writer.Write(singlePlacementInput.IsFlipped);
            writer.Write(singlePlacementInput.UseForce);
            Encode(singlePlacementInput.StartPivot, stream);
            writer.Write(singlePlacementInput.StartedPlacement);
            Encode(singlePlacementInput.PreferredRotation, stream);
        }
        public static SinglePlacementInput<GlobalTileCoordinate, TileDirection, GlobalTilePivot> DecodeSinglePlacementInputTile(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var endPivot = DecodeGlobalTilePivot(stream);
            var confirmed = reader.ReadBoolean();
            var cancelled = reader.ReadBoolean();
            var startedDragging = reader.ReadBoolean();
            var isFlipped = reader.ReadBoolean();
            var useForce = reader.ReadBoolean();
            var startPivot = DecodeGlobalTilePivot(stream);
            var startedPlacement = reader.ReadBoolean();
            var preferredRotation = DecodeGridRotation(stream);
            var singlePlacementInput = new SinglePlacementInput<GlobalTileCoordinate, TileDirection, GlobalTilePivot>(preferredRotation, isFlipped);
            singlePlacementInput.EndPivot = endPivot;
            singlePlacementInput.Confirmed = confirmed;
            singlePlacementInput.Cancelled = cancelled;
            singlePlacementInput.StartedDragging = startedDragging;
            singlePlacementInput.UseForce = useForce;
            singlePlacementInput.StartPivot = startPivot;
            singlePlacementInput.StartedPlacement = startedPlacement;
            return singlePlacementInput;
        }
        public static void Encode(SinglePlacementInput<GlobalChunkCoordinate, ChunkDirection, GlobalChunkPivot> singlePlacementInput, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            Encode(singlePlacementInput.EndPivot, stream);
            writer.Write(singlePlacementInput.Confirmed);
            writer.Write(singlePlacementInput.Cancelled);
            writer.Write(singlePlacementInput.StartedDragging);
            writer.Write(singlePlacementInput.IsFlipped);
            writer.Write(singlePlacementInput.UseForce);
            Encode(singlePlacementInput.StartPivot, stream);
            writer.Write(singlePlacementInput.StartedPlacement);
            Encode(singlePlacementInput.PreferredRotation, stream);
        }
        public static SinglePlacementInput<GlobalChunkCoordinate, ChunkDirection, GlobalChunkPivot> DecodeSinglePlacementInputChunk(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var endPivot = DecodeGlobalChunkPivot(stream);
            var confirmed = reader.ReadBoolean();
            var cancelled = reader.ReadBoolean();
            var startedDragging = reader.ReadBoolean();
            var isFlipped = reader.ReadBoolean();
            var useForce = reader.ReadBoolean();
            var startPivot = DecodeGlobalChunkPivot(stream);
            var startedPlacement = reader.ReadBoolean();
            var preferredRotation = DecodeGridRotation(stream);
            var singlePlacementInput = new SinglePlacementInput<GlobalChunkCoordinate, ChunkDirection, GlobalChunkPivot>(preferredRotation, isFlipped);
            singlePlacementInput.EndPivot = endPivot;
            singlePlacementInput.Confirmed = confirmed;
            singlePlacementInput.Cancelled = cancelled;
            singlePlacementInput.StartedDragging = startedDragging;
            singlePlacementInput.UseForce = useForce;
            singlePlacementInput.StartPivot = startPivot;
            singlePlacementInput.StartedPlacement = startedPlacement;
            return singlePlacementInput;
        }
        public static void Encode(VariablePairPlacementInput<GlobalTileCoordinate, TileDirection, GlobalTilePivot> variablePairPlacementInput, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(variablePairPlacementInput.Confirmed);
            writer.Write(variablePairPlacementInput.Cancelled);
            writer.Write(variablePairPlacementInput.StartedDragging);
            writer.Write(variablePairPlacementInput.UseForce);
            Encode(variablePairPlacementInput.PreferredRotation, stream);
            Encode(variablePairPlacementInput.StartPivot, stream);
            Encode(variablePairPlacementInput.EndPivot, stream);
            writer.Write(variablePairPlacementInput.StartedPlacement);
            writer.Write(variablePairPlacementInput.IsReversed);
            writer.Write(variablePairPlacementInput.OverrideRange != null);
            if (variablePairPlacementInput.OverrideRange != null) writer.Write(variablePairPlacementInput.OverrideRange.Value);
            writer.Write(variablePairPlacementInput.MinDistance);
            writer.Write(variablePairPlacementInput.MaxDistance);
        }
        public static VariablePairPlacementInput<GlobalTileCoordinate, TileDirection, GlobalTilePivot> DecodeVariablePairPlacementInputTile(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var confirmed = reader.ReadBoolean();
            var cancelled = reader.ReadBoolean();
            var startedDragging = reader.ReadBoolean();
            var useForce = reader.ReadBoolean();
            var preferredRotation = DecodeGridRotation(stream);
            var startPivot = DecodeGlobalTilePivot(stream);
            var endPivot = DecodeGlobalTilePivot(stream);
            var startedPlacement = reader.ReadBoolean();
            var isReversed = reader.ReadBoolean();
            int? overrideRange = null;
            if (reader.ReadBoolean()) overrideRange = reader.ReadInt32();
            var minDistance = reader.ReadInt32();
            var maxDistance = reader.ReadInt32();
            var variablePairPlacementInput = new VariablePairPlacementInput<GlobalTileCoordinate, TileDirection, GlobalTilePivot>(preferredRotation, minDistance, maxDistance, isReversed);
            variablePairPlacementInput.Confirmed = confirmed;
            variablePairPlacementInput.Cancelled = cancelled;
            variablePairPlacementInput.StartedDragging = startedDragging;
            variablePairPlacementInput.UseForce = useForce;
            variablePairPlacementInput.StartPivot = startPivot;
            variablePairPlacementInput.EndPivot = endPivot;
            variablePairPlacementInput.StartedPlacement = startedPlacement;
            variablePairPlacementInput.OverrideRange = overrideRange;
            return variablePairPlacementInput;
        }
        public static void Encode(VariablePairPlacementInput<GlobalChunkCoordinate, ChunkDirection, GlobalChunkPivot> variablePairPlacementInput, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(variablePairPlacementInput.Confirmed);
            writer.Write(variablePairPlacementInput.Cancelled);
            writer.Write(variablePairPlacementInput.StartedDragging);
            writer.Write(variablePairPlacementInput.UseForce);
            Encode(variablePairPlacementInput.PreferredRotation, stream);
            Encode(variablePairPlacementInput.StartPivot, stream);
            Encode(variablePairPlacementInput.EndPivot, stream);
            writer.Write(variablePairPlacementInput.StartedPlacement);
            writer.Write(variablePairPlacementInput.IsReversed);
            writer.Write(variablePairPlacementInput.OverrideRange != null);
            if (variablePairPlacementInput.OverrideRange != null) writer.Write(variablePairPlacementInput.OverrideRange.Value);
            writer.Write(variablePairPlacementInput.MinDistance);
            writer.Write(variablePairPlacementInput.MaxDistance);
        }
        public static VariablePairPlacementInput<GlobalChunkCoordinate, ChunkDirection, GlobalChunkPivot> DecodeVariablePairPlacementInputChunk(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var confirmed = reader.ReadBoolean();
            var cancelled = reader.ReadBoolean();
            var startedDragging = reader.ReadBoolean();
            var useForce = reader.ReadBoolean();
            var preferredRotation = DecodeGridRotation(stream);
            var startPivot = DecodeGlobalChunkPivot(stream);
            var endPivot = DecodeGlobalChunkPivot(stream);
            var startedPlacement = reader.ReadBoolean();
            var isReversed = reader.ReadBoolean();
            int? overrideRange = null;
            if (reader.ReadBoolean()) overrideRange = reader.ReadInt32();
            var minDistance = reader.ReadInt32();
            var maxDistance = reader.ReadInt32();
            var variablePairPlacementInput = new VariablePairPlacementInput<GlobalChunkCoordinate, ChunkDirection, GlobalChunkPivot>(preferredRotation, minDistance, maxDistance, isReversed);
            variablePairPlacementInput.Confirmed = confirmed;
            variablePairPlacementInput.Cancelled = cancelled;
            variablePairPlacementInput.StartedDragging = startedDragging;
            variablePairPlacementInput.UseForce = useForce;
            variablePairPlacementInput.StartPivot = startPivot;
            variablePairPlacementInput.EndPivot = endPivot;
            variablePairPlacementInput.StartedPlacement = startedPlacement;
            variablePairPlacementInput.OverrideRange = overrideRange;
            return variablePairPlacementInput;
        }
        public static void Encode(PathPlacementInputSegment<GlobalTilePivot, TileDirection> pathPlacementInputSegment, Stream stream)
        {
            Encode(pathPlacementInputSegment.StartPivot, stream);
            Encode(pathPlacementInputSegment.EndPivot, stream);
            Encode(pathPlacementInputSegment.PreferredDirection, stream);
        }
        public static PathPlacementInputSegment<GlobalTilePivot, TileDirection> DecodePathPlacementInputSegmentTile(Stream stream)
        {
            return new PathPlacementInputSegment<GlobalTilePivot, TileDirection>(DecodeGlobalTilePivot(stream), DecodeGlobalTilePivot(stream), DecodeTileDirection(stream));
        }
        public static void Encode(PathPlacementInputSegment<GlobalChunkPivot, ChunkDirection> pathPlacementInputSegment, Stream stream)
        {
            Encode(pathPlacementInputSegment.StartPivot, stream);
            Encode(pathPlacementInputSegment.EndPivot, stream);
            Encode(pathPlacementInputSegment.PreferredDirection, stream);
        }
        public static PathPlacementInputSegment<GlobalChunkPivot, ChunkDirection> DecodePathPlacementInputSegmentChunk(Stream stream)
        {
            return new PathPlacementInputSegment<GlobalChunkPivot, ChunkDirection>(DecodeGlobalChunkPivot(stream), DecodeGlobalChunkPivot(stream), DecodeChunkDirection(stream));
        }
        enum PlacementInputType : byte
        {
            AreaPlacementInputTile,
            AreaPlacementInputChunk,
            BlueprintPlacementInputTile,
            BlueprintPlacementInputChunk,
            LinePlacementInputTile,
            LinePlacementInputChunk,
            PathPlacementInputTile,
            PathPlacementInputChunk,
            SinglePlacementInputTile,
            SinglePlacementInputChunk,
            VariablePairPlacementInputTile,
            VariablePairPlacementInputChunk
        }
        public static ResearchManager.SerializedData SerializeResearchManager(ResearchManager researchManager)
        {
            return new ResearchManager.SerializedData()
            {
                ResearchProgress = researchManager.Progress.Serialize(),
                Shapes = researchManager.ShapeStorage.Serialize(),
                BlueprintCurrency = researchManager.BlueprintCurrencyManager.Serialize(),
                PointCurrency = researchManager.PointStorage.Serialize(),
                LinearUpgrades = researchManager.LinearUpgradeManager.Serialize(),
                PlayerLevel = researchManager.PlayerLevel.Serialize(),
                PlayerLevelGoals = researchManager.PlayerLevelGoals.Serialize()
            };
        }
        public static void Encode(ResearchManager.SerializedData serializedData, Stream stream)
        {
            Encode(serializedData.ResearchProgress, stream);
            Encode(serializedData.Shapes, stream);
            Encode(serializedData.BlueprintCurrency, stream);
            Encode(serializedData.PointCurrency, stream);
            Encode(serializedData.LinearUpgrades, stream);
            Encode(serializedData.PlayerLevel, stream);
            Encode(serializedData.PlayerLevelGoals, stream);
        }
        public static ResearchManager.SerializedData DecodeResearchManagerSerializedData(Stream stream)
        {
            var serializedData = new ResearchManager.SerializedData();
            serializedData.ResearchProgress = DecodeResearchUnlockProgressManagerSerializedData(stream);
            serializedData.Shapes = DecodeResearchShapeStorageSerializedData(stream);
            serializedData.BlueprintCurrency = DecodeBlueprintCurrencyManagerSerializedData(stream);
            serializedData.PointCurrency = DecodeResearchPointStorageSerializedData(stream);
            serializedData.LinearUpgrades = DecodeResearchLinearUpgradeManagerSerializedData(stream);
            serializedData.PlayerLevel = DecodeResearchPlayerLevelManagerSerializedData(stream);
            serializedData.PlayerLevelGoals = DecodeResearchPlayerLevelGoalManagerSerializedData(stream);
            return serializedData;
        }
        public static void Encode(ResearchUnlockProgressManager.SerializedData serializedData, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(serializedData.UnlockedUpgradeIds.Count);
            foreach (var upgradeId in serializedData.UnlockedUpgradeIds)
            {
                writer.Write(upgradeId);
            }
        }
        public static ResearchUnlockProgressManager.SerializedData DecodeResearchUnlockProgressManagerSerializedData(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var serializedData = new ResearchUnlockProgressManager.SerializedData();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                serializedData.UnlockedUpgradeIds.Add(reader.ReadString());
            }
            return serializedData;
        }
        public static void Encode(ResearchShapeStorage.SerializedData serializedData, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(serializedData.StoredShapes.Count);
            foreach (var kvp in serializedData.StoredShapes)
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value);
            }
        }
        public static ResearchShapeStorage.SerializedData DecodeResearchShapeStorageSerializedData(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var serializedData = new ResearchShapeStorage.SerializedData();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                serializedData.StoredShapes.Add(reader.ReadString(), reader.ReadInt32());
            }
            return serializedData;
        }
        public static void Encode(BlueprintCurrencyManager.SerializedData serializedData, Stream stream)
        {
            Encode(serializedData.BlueprintCurrency, stream);
            Encode(serializedData.TotalAmountSpent, stream);
        }
        public static BlueprintCurrencyManager.SerializedData DecodeBlueprintCurrencyManagerSerializedData(Stream stream)
        {
            var serializedData = new BlueprintCurrencyManager.SerializedData();
            serializedData.BlueprintCurrency = DecodeBlueprintCurrency(stream);
            serializedData.TotalAmountSpent = DecodeBlueprintCurrency(stream);
            return serializedData;
        }
        public static void Encode(ResearchPointStorage.SerializedData serializedData, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(serializedData.Points);
            writer.Write(serializedData.TotalSpent);
        }
        public static ResearchPointStorage.SerializedData DecodeResearchPointStorageSerializedData(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var serializedData = new ResearchPointStorage.SerializedData();
            serializedData.Points = reader.ReadInt32();
            serializedData.TotalSpent = reader.ReadInt32();
            return serializedData;
        }
        public static void Encode(ResearchLinearUpgradeManager.SerializedData serializedData, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(serializedData.UpgradeLevels.Count);
            foreach (var kvp in serializedData.UpgradeLevels)
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value);
            }
        }
        public static ResearchLinearUpgradeManager.SerializedData DecodeResearchLinearUpgradeManagerSerializedData(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var serializedData = new ResearchLinearUpgradeManager.SerializedData();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                serializedData.UpgradeLevels.Add(reader.ReadString(), reader.ReadInt32());
            }
            return serializedData;
        }
        public static void Encode(ResearchPlayerLevelManager.SerializedData serializedData, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(serializedData.Level);
        }
        public static ResearchPlayerLevelManager.SerializedData DecodeResearchPlayerLevelManagerSerializedData(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var serializedData = new ResearchPlayerLevelManager.SerializedData();
            serializedData.Level = reader.ReadInt32();
            return serializedData;
        }
        public static void Encode(ResearchPlayerLevelGoalManager.SerializedData serializedData, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(serializedData.GoalLevels.Count);
            foreach (var kvp in serializedData.GoalLevels)
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value);
            }
        }
        public static ResearchPlayerLevelGoalManager.SerializedData DecodeResearchPlayerLevelGoalManagerSerializedData(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var serializedData = new ResearchPlayerLevelGoalManager.SerializedData();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                serializedData.GoalLevels.Add(reader.ReadString(), reader.ReadInt32());
            }
            return serializedData;
        }
        public static void Encode(IText text, Stream stream)
        {
            if (text is LazyLocalizedText lazyLocalizedText)
            {
                stream.WriteByte((byte)TextType.LazyLocalizedText);
                Encode(lazyLocalizedText, stream);
            } else if (text is RawText rawText)
            {
                stream.WriteByte((byte)TextType.RawText);
                Encode(rawText, stream);
            } else if (text is CombinedText combinedText)
            {
                stream.WriteByte((byte)TextType.CombinedText);
                Encode(combinedText, stream);
            } else
            {
                Shapez2Multiplayer.logger.Error.Log($"Unknown text type: {text.GetType().Name}");
            }
        }
        public static IText? DecodeText(Stream stream)
        {
            switch ((TextType)stream.ReadByte())
            {
                case TextType.LazyLocalizedText:
                    return DecodeLazyLocalizedText(stream);
                case TextType.RawText:
                    return DecodeRawText(stream);
                case TextType.CombinedText:
                    return DecodeCombinedText(stream);
                default:
                    Shapez2Multiplayer.logger.Error.Log("Tried to decode unknown text");
                    return null;
            }
        }
        public static readonly FieldInfo CombinedTextTextsInfo = AccessTools.Field(typeof(CombinedText), "Texts");
        public static void Encode(CombinedText combinedText, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var Texts = (List<IText>)CombinedTextTextsInfo.GetValue(combinedText);
            writer.Write(Texts.Count);
            foreach (var text in Texts)
            {
                Encode(text, stream);
            }
        }
        public static CombinedText DecodeCombinedText(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var contents = new List<IText>();
            var length = reader.ReadInt32();
            for (int i = 0; i < length; i++)
            {
                contents.Add(DecodeText(stream));
            }
            return new CombinedText(contents);
        }
        public static readonly FieldInfo FieldInfoRawTextTextInfo = AccessTools.Field(typeof(RawText), "Text");
        public static void Encode(RawText rawText, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write((string)FieldInfoRawTextTextInfo.GetValue(rawText));
        }
        public static RawText DecodeRawText(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            return new RawText(reader.ReadString());
        }
        public static void Encode(LazyLocalizedText lazyLocalizedText, Stream stream)
        {
            Encode(lazyLocalizedText.Id, stream);
        }
        public static LazyLocalizedText DecodeLazyLocalizedText(Stream stream)
        {
            return new LazyLocalizedText(DecodeTranslationId(stream));
        }
        public static void Encode(TranslationId translationId, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(translationId.Id);
        }
        public static TranslationId DecodeTranslationId(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            return new TranslationId(reader.ReadString());
        }
        enum TextType : byte
        {
            LazyLocalizedText,
            RawText,
            CombinedText,
            //DualKeybindingText, Needs Keybind Serialization
            //GenericFormattedNumberText, Needs INumberFormatter Serialization
            //KeybindingText, Needs Keybind Serialization
            //KeyCodeText,
            //KeySetText,
            //MonospaceText
        }
        public static void Encode(IPlayerAction playerAction, Stream stream)
        {
            if (playerAction is ActionModifyBuildings actionModifyBuildings)
            {
                stream.WriteByte((byte)PlayerActionType.ActionModifyBuildings);
                Encode(actionModifyBuildings, stream);
            }
            else if (playerAction is ActionModifyIsland actionModifyIsland)
            {
                stream.WriteByte((byte)PlayerActionType.ActionModifyIsland);
                Encode(actionModifyIsland, stream);
            }
            else if (playerAction is CombinedUndoablePlayerAction combinedUndoablePlayerAction)
            {
                stream.WriteByte((byte)PlayerActionType.CombinedUndoable);
                Encode(combinedUndoablePlayerAction, stream);
            }
            else if (playerAction is ResearchUpgradePlayerAction researchUpgradePlayerAction)
            {
                stream.WriteByte((byte)PlayerActionType.ResearchUpgrade);
                Encode(researchUpgradePlayerAction, stream);
            } else if (playerAction is LevelUpLinearUpgradePlayerAction levelUpLinearUpgradePlayerAction)
            {
                stream.WriteByte((byte)PlayerActionType.LevelUpLinearUpgrade);
                Encode(levelUpLinearUpgradePlayerAction, stream);
            } else if (playerAction is ClearBuildingContentPlayerAction clearBuildingContentPlayerAction)
            {
                stream.WriteByte((byte)PlayerActionType.ClearBuildingContent);
                Encode(clearBuildingContentPlayerAction, stream);
            } else if (playerAction is ClearIslandContentPlayerAction clearIslandContentPlayerAction)
            {
                stream.WriteByte((byte)PlayerActionType.ClearIslandContent);
                Encode(clearIslandContentPlayerAction, stream);
            }
            else
            {
                Shapez2Multiplayer.logger.Error.Log($"Unknown player action type: {playerAction.GetType().Name}");
            }
        }
        public static IPlayerAction? DecodePlayerAction(Stream stream)
        {
            switch ((PlayerActionType)stream.ReadByte())
            {
                case PlayerActionType.ActionModifyBuildings:
                    return DecodeActionModifyBuildings(stream);
                case PlayerActionType.ActionModifyIsland:
                    return DecodeActionModifyIsland(stream);
                case PlayerActionType.CombinedUndoable:
                    return DecodeCombinedUndoablePlayerAction(stream);
                case PlayerActionType.ResearchUpgrade:
                    return DecodeResearchUpgradePlayerAction(stream);
                case PlayerActionType.LevelUpLinearUpgrade:
                    return DecodeLevelUpLinearUpgradePlayerAction(stream);
                case PlayerActionType.ClearBuildingContent:
                    return DecodeClearBuildingContentPlayerAction(stream);
                case PlayerActionType.ClearIslandContent:
                    return DecodeClearIslandContentPlayerAction(stream);
                default:
                    Shapez2Multiplayer.logger.Error.Log("Tried to decode unknown player action");
                    return null;
            }
        }
        enum PlayerActionType : byte
        {
            ActionModifyBuildings,
            ActionModifyIsland,
            CombinedUndoable,
            ResearchUpgrade,
            LevelUpLinearUpgrade,
            ClearBuildingContent,
            ClearIslandContent
        }
        public static readonly FieldInfo LevelUpLinearUpgradePlayerActionUpgradeId = AccessTools.Field(typeof(LevelUpLinearUpgradePlayerAction), "UpgradeId");
        public static void Encode(LevelUpLinearUpgradePlayerAction levelUpLinearUpgradePlayerAction, Stream stream)
        {
            Encode((ResearchLinearUpgradeId)LevelUpLinearUpgradePlayerActionUpgradeId.GetValue(levelUpLinearUpgradePlayerAction), stream);
        }
        public static LevelUpLinearUpgradePlayerAction DecodeLevelUpLinearUpgradePlayerAction(Stream stream)
        {
            return new LevelUpLinearUpgradePlayerAction(DecodeResearchLinearUpgradeId(stream), Shapez2Multiplayer.Research.LinearUpgradeManager, Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer);
        }
        public static readonly FieldInfo ResearchUpgradePlayerActionUpgrade = AccessTools.Field(typeof(ResearchUpgradePlayerAction), "Upgrade");
        public static void Encode(ResearchUpgradePlayerAction researchUpgradePlayerAction, Stream stream)
        {
            Encode((IResearchUpgrade)ResearchUpgradePlayerActionUpgrade.GetValue(researchUpgradePlayerAction), stream);
        }
        public static ResearchUpgradePlayerAction DecodeResearchUpgradePlayerAction(Stream stream)
        {
            return new ResearchUpgradePlayerAction(DecodeResearchUpgrade(stream), Shapez2Multiplayer.Research.UnlockManager, Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer);
        }
        public static void Encode(IResearchUpgrade researchUpgrade, Stream stream)
        {
            Encode(researchUpgrade.Id, stream);
        }
        public static IResearchUpgrade DecodeResearchUpgrade(Stream stream)
        {
            return Shapez2Multiplayer.Mode.ResearchLayout.GetUpgrade(DecodeResearchUpgradeId(stream));
        }
        public static readonly FieldInfo CombinedUndoablePlayerActionActionsInfo = AccessTools.Field(typeof(CombinedUndoablePlayerAction), "Actions");
        public static void Encode(CombinedUndoablePlayerAction combinedUndoablePlayerAction, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var Actions = (List<IPlayerAction>)CombinedUndoablePlayerActionActionsInfo.GetValue(combinedUndoablePlayerAction);
            writer.Write(Actions.Count);
            foreach (var action in Actions)
            {
                Encode(action, stream);
            }
        }
        public static CombinedUndoablePlayerAction DecodeCombinedUndoablePlayerAction(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var Actions = new List<IPlayerAction>();
            var length = reader.ReadInt32();
            for (int i = 0; i < length; i++)
            {
                var action = DecodePlayerAction(stream);
                if (action != null)
                {
                    Actions.Add(action);
                }
            }
            return new CombinedUndoablePlayerAction(Actions);
        }
        public static readonly FieldInfo ClearIslandContentPlayerActionBuildingsToClearInfo = AccessTools.Field(typeof(ClearIslandContentPlayerAction), "IslandsToClear");
        public static void Encode(ClearIslandContentPlayerAction clearIslandContentPlayerAction, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var IslandsToClear = (IReadOnlyList<IslandId>)ClearIslandContentPlayerActionBuildingsToClearInfo.GetValue(clearIslandContentPlayerAction);
            writer.Write(IslandsToClear.Count);
            foreach (var Island in IslandsToClear)
            {
                Encode(Shapez2Multiplayer.MapModel.GetIsland(Island).Position, stream);
            }
        }
        public static ClearIslandContentPlayerAction DecodeClearIslandContentPlayerAction(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var IslandsToClear = new IslandId[reader.ReadInt32()];
            for (int i = 0; i < IslandsToClear.Length; i++)
            {
                IslandsToClear[i] = Shapez2Multiplayer.MapModel.GetIsland(DecodeGlobalChunkCoordinate(stream)).Id;
            }
            return new ClearIslandContentPlayerAction(Shapez2Multiplayer.MapModel, Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer, IslandsToClear);
        }
        public static readonly FieldInfo ClearBuildingContentPlayerActionBuildingsToClearInfo = AccessTools.Field(typeof(ClearBuildingContentPlayerAction), "BuildingsToClear");
        public static void Encode(ClearBuildingContentPlayerAction clearBuildingContentPlayerAction, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var BuildingsToClear = (IReadOnlyList<BuildingId>)ClearBuildingContentPlayerActionBuildingsToClearInfo.GetValue(clearBuildingContentPlayerAction);
            writer.Write(BuildingsToClear.Count);
            foreach (var building in BuildingsToClear)
            {
                Encode(Shapez2Multiplayer.MapModel.GetBuilding(building).Tile_G, stream);
            }
        }
        public static ClearBuildingContentPlayerAction DecodeClearBuildingContentPlayerAction(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var BuildingsToClear = new BuildingId[reader.ReadInt32()];
            for (int i = 0; i < BuildingsToClear.Length; i++)
            {
                BuildingsToClear[i] = Shapez2Multiplayer.MapModel.GetBuilding(DecodeGlobalTileCoordinate(stream)).Id;
            }
            return new ClearBuildingContentPlayerAction(Shapez2Multiplayer.MapModel, Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer, BuildingsToClear);
        }
        public static void Encode(ActionModifyIsland actionModifyIsland, Stream stream)
        {
            Encode(actionModifyIsland.Data, stream);
        }
        public static ActionModifyIsland DecodeActionModifyIsland(Stream stream)
        {
            return new ActionModifyIsland(Shapez2Multiplayer.MapModel, Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer, DecodeActionModifyIslandPayload(stream));
        }
        public static void Encode(ActionModifyIsland.Payload payload, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(payload.IgnorePlacementBlueprintCost);
            writer.Write(payload.RefundDeletionBlueprintCost);
            writer.Write(payload.Place.Count);
            foreach (var placePayload in payload.Place)
            {
                Encode(placePayload, stream);
            }
            writer.Write(payload.Delete.Count);
            foreach (var deletePayload in payload.Delete)
            {
                Encode(deletePayload, stream);
            }
        }
        public static ActionModifyIsland.Payload DecodeActionModifyIslandPayload(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var ignorePlacementBlueprintCost = reader.ReadBoolean();
            var refundDeletionBlueprintCost = reader.ReadBoolean();
            var place = new ActionModifyIsland.PlacePayload[reader.ReadInt32()];
            for (int i = 0; i < place.Length; i++)
            {
                place[i] = DecodeActionModifyIslandPlacePayload(stream); ;
            }
            var delete = new ActionModifyIsland.DeletePayload[reader.ReadInt32()];
            for (int i = 0; i < delete.Length; i++)
            {
                delete[i] = DecodeActionModifyIslandDeletePayload(stream);
            }
            return new ActionModifyIsland.Payload(place, delete, ignorePlacementBlueprintCost, refundDeletionBlueprintCost);
        }
        public static void Encode(ActionModifyIsland.DeletePayload deletePayload, Stream stream)
        {
            Encode(Shapez2Multiplayer.MapModel.GetIsland(deletePayload.IslandId).Position, stream);
        }
        public static ActionModifyIsland.DeletePayload DecodeActionModifyIslandDeletePayload(Stream stream)
        {
            var Coordinate = DecodeGlobalChunkCoordinate(stream);
            var Id = Shapez2Multiplayer.MapModel.GetIsland(Coordinate).Id;
            DeletedIslandIds[Coordinate] = Id;
            return new ActionModifyIsland.DeletePayload(Id);
        }
        public static byte[] Test(object obj)
        {
            using var Stream = new MemoryStream();
            AccessTools.Method(typeof(Encoding), "Encode", new Type[] { obj.GetType(), typeof(Stream) }).Invoke(null, new object[] { obj, Stream });
            return Stream.ToArray();
        }
        public static void Encode(ActionModifyIsland.PlacePayload placePayload, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            if (placePayload.IslandId != null && Shapez2Multiplayer.MapModel.TryGetIsland(placePayload.IslandId.Value, out IslandModel island))
            {
                writer.Write(true);
                Encode(island.Position, stream);
            } else
            {
                writer.Write(false);
            }
            Encode((IslandDefinition)placePayload.Definition, stream);
            writer.Write(placePayload.Configuration != null);
            if (placePayload.Configuration != null)
            {
                writer.Write(placePayload.Configuration.GetType().AssemblyQualifiedName);
                placePayload.Configuration.Sync(serializationVisitor);
            }
            Encode(placePayload.Origin_GC, stream);
            Encode(placePayload.Rotation, stream);
            writer.Write(placePayload.PlaceBuildings.Count);
            foreach (var placeBuildingPayload in placePayload.PlaceBuildings)
            {
                Encode(placeBuildingPayload, stream);
            }
        }
        public static readonly FieldInfo ActionModifyIslandPlacePayloadIslandId = AccessTools.Field(typeof(ActionModifyIsland.PlacePayload), "IslandId");
        public static ActionModifyIsland.PlacePayload DecodeActionModifyIslandPlacePayload(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            IslandId? islandId = null;
            if (reader.ReadBoolean())
            {
                var Coordinate = DecodeGlobalChunkCoordinate(stream);
                if (Shapez2Multiplayer.MapModel.TryGetIsland(Coordinate, out IslandModel island))
                {
                    islandId = island.Id;
                } else if (DeletedIslandIds.Remove(Coordinate, out IslandId id))
                {
                    islandId = id;
                }
            }
            var definition = DecodeIslandDefinition(stream);
            IIslandConfiguration configuration = null;
            if (reader.ReadBoolean())
            {
                configuration = (IIslandConfiguration)Activator.CreateInstance(Type.GetType(reader.ReadString()));
                configuration.Sync(serializationVisitor);
            }
            
            var Origin_GC = DecodeGlobalChunkCoordinate(stream);
            var rotation = DecodeGridRotation(stream);
            var placeBuildings = new PlaceBuildingPayload[reader.ReadInt32()];
            for (int i = 0; i < placeBuildings.Length; i++)
            {
                placeBuildings[i] = DecodePlaceBuildingPayload(stream);
            }
            var actionModifyIslandPlacePayload = new ActionModifyIsland.PlacePayload(definition, configuration, Origin_GC, rotation, placeBuildings);
            object boxed = actionModifyIslandPlacePayload;
            ActionModifyIslandPlacePayloadIslandId.SetValue(boxed, islandId);
            return (ActionModifyIsland.PlacePayload)boxed;
        }
        public static readonly FieldInfo ActionModifyBuildingsUseBunchEditMode = AccessTools.Field(typeof(ActionModifyBuildings), "UseBunchEditMode");
        public static void Encode(ActionModifyBuildings actionModifyBuildings, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            Encode(actionModifyBuildings.Data, stream);
            writer.Write((bool)ActionModifyBuildingsUseBunchEditMode.GetValue(actionModifyBuildings));
        }
        public static ActionModifyBuildings DecodeActionModifyBuildings(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            return new ActionModifyBuildings(Shapez2Multiplayer.MapModel, Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer, DecodeModifyBuildingsPayload(stream), reader.ReadBoolean());
        }
        public static void Encode(ModifyBuildingsPayload modifyBuildingsPayload, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(modifyBuildingsPayload.Place.Count);
            foreach (var placeBuildingPayload in modifyBuildingsPayload.Place)
            {
                Encode(placeBuildingPayload, stream);
            }
            writer.Write(modifyBuildingsPayload.Delete.Count);
            foreach (var deleteBuildingPayload in modifyBuildingsPayload.Delete)
            {
                Encode(deleteBuildingPayload, stream);
            }
            Encode(modifyBuildingsPayload.BlueprintCurrencyModification, stream);
        }
        public static ModifyBuildingsPayload DecodeModifyBuildingsPayload(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var place = new PlaceBuildingPayload[reader.ReadInt32()];
            for (int i = 0; i < place.Length; i++)
            {
                place[i] = DecodePlaceBuildingPayload(stream);
            }
            var delete = new DeleteBuildingPayload[reader.ReadInt32()];
            for (int i = 0; i < delete.Length; i++)
            {
                delete[i] = DecodeDeleteBuildingPayload(stream);
            }
            return new ModifyBuildingsPayload(place, delete, DecodeBlueprintCurrency(stream));
        }
        public static void Encode(DeleteBuildingPayload deleteBuildingPayload, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            Encode(Shapez2Multiplayer.MapModel.GetBuilding(deleteBuildingPayload.BuildingId).Tile_G, stream);
            writer.Write(deleteBuildingPayload.ForceAllowDelete);
        }
        public static DeleteBuildingPayload DecodeDeleteBuildingPayload(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var Coordinate = DecodeGlobalTileCoordinate(stream);
            var Id = Shapez2Multiplayer.MapModel.GetBuilding(Coordinate).Id;
            DeletedBuildingIds[Coordinate] = Id;
            return new DeleteBuildingPayload(Id, reader.ReadBoolean());
        }
        public static void Encode(PlaceBuildingPayload placeBuildingPayload, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            if (placeBuildingPayload.BuildingId != null && Shapez2Multiplayer.MapModel.TryGetBuilding(placeBuildingPayload.BuildingId.Value, out BuildingModel building))
            {
                writer.Write(true);
                Encode(building.Tile_G, stream);
            } else
            {
                writer.Write(false);
            }
            //Encode(placeBuildingPayload.IslandId, stream);
            writer.Write((uint)IslandIdId.GetValue(placeBuildingPayload.IslandId) > 0);
            if ((uint)IslandIdId.GetValue(placeBuildingPayload.IslandId) > 0) Encode(Shapez2Multiplayer.MapModel.GetIsland(placeBuildingPayload.IslandId).Position, stream);
            Encode((BuildingDefinition)placeBuildingPayload.Definition, stream);
            writer.Write(placeBuildingPayload.Configuration != null);
            if (placeBuildingPayload.Configuration != null)
            {
                writer.Write(placeBuildingPayload.Configuration.GetType().AssemblyQualifiedName);
                placeBuildingPayload.Configuration.Sync(serializationVisitor);
            }
            writer.Write(placeBuildingPayload.SerializedState != null);
            if (placeBuildingPayload.SerializedState != null)
            {
                writer.Write(placeBuildingPayload.SerializedState.Length);
                writer.Write(placeBuildingPayload.SerializedState);
            }
            writer.Write((int)placeBuildingPayload.AdditionalDataSavegameVersion);
            writer.Write(placeBuildingPayload.ForceAllowPlace);
            Encode(placeBuildingPayload.Transform_I, stream);
        }
        public static readonly FieldInfo PlaceBuildingPayloadBuildingId = AccessTools.Field(typeof(PlaceBuildingPayload), "BuildingId");
        public static PlaceBuildingPayload DecodePlaceBuildingPayload(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            BuildingId? buildingId = null;
            if (reader.ReadBoolean())
            {
                var Coordinate = DecodeGlobalTileCoordinate(stream);
                if (Shapez2Multiplayer.MapModel.TryGetBuilding(Coordinate, out BuildingModel building))
                {
                    DeletedBuildingIds.Remove(Coordinate);
                    buildingId = building.Id;
                } else if (DeletedBuildingIds.Remove(Coordinate, out BuildingId id))
                {
                    buildingId = id;
                }
            }
            //var islandId = DecodeIslandId(stream);
            IslandId islandId;
            if (reader.ReadBoolean())
            {
                islandId = Shapez2Multiplayer.MapModel.GetIsland(DecodeGlobalChunkCoordinate(stream)).Id;
            } else
            {
                islandId = (IslandId)IslandIdConstructor.Invoke(new object[] { (uint)0 });
            }
            var definition = DecodeBuildingDefinition(stream);
            IBuildingConfiguration configuration = null;
            if (reader.ReadBoolean())
            {
                var type = reader.ReadString();
                //configuration = (IBuildingConfiguration)Type.GetType(reader.ReadString()).Constructor().Invoke(new object[] { });
                configuration = (IBuildingConfiguration)Activator.CreateInstance(Type.GetType(type));
                configuration.Sync(serializationVisitor);
            }
            byte[] serializedState = null;
            if (reader.ReadBoolean()) serializedState = reader.ReadBytes(reader.ReadInt32());
            var additionalDataSavegameVersion = (GameVersion)reader.ReadInt32();
            var forceAllowPlace = reader.ReadBoolean();
            var transform_I = DecodeIslandTileTransform(stream);
            var placeBuildingPayload = new PlaceBuildingPayload(islandId, definition, configuration, transform_I, serializedState, additionalDataSavegameVersion, forceAllowPlace);
            object boxed = placeBuildingPayload;
            PlaceBuildingPayloadBuildingId.SetValue(boxed, buildingId);
            return (PlaceBuildingPayload)boxed;
        }
        public static void Encode(IslandTileTransform islandTileTransform, Stream stream)
        {
            Encode(islandTileTransform.Position, stream);
            Encode(islandTileTransform.Rotation, stream);
        }
        public static IslandTileTransform DecodeIslandTileTransform(Stream stream)
        {
            return new IslandTileTransform(DecodeIslandTileCoordinate(stream), DecodeGridRotation(stream));
        }
        public static readonly FieldInfo BuildingIdId = AccessTools.Field(typeof(BuildingId), "Id");
        public static readonly ConstructorInfo BuildingIdConstructor = AccessTools.Constructor(typeof(BuildingId), new Type[] { typeof(int) });
        [Obsolete("IslandId is not universal, get from position instead", true)]
        public static void Encode(BuildingId buildingId, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write((int)BuildingIdId.GetValue(buildingId));
        }
        [Obsolete("IslandId is not universal, get from position instead", true)]
        public static BuildingId DecodeBuildingId(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            return (BuildingId)BuildingIdConstructor.Invoke(new object[] { reader.ReadInt32() });
        }
        public static readonly FieldInfo IslandIdId = AccessTools.Field(typeof(IslandId), "Id");
        public static readonly ConstructorInfo IslandIdConstructor = AccessTools.Constructor(typeof(IslandId), new Type[] { typeof(uint) });
        [Obsolete("IslandId is not universal, get from position instead", true)]
        public static void Encode(IslandId buildingId, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write((uint)IslandIdId.GetValue(buildingId));
        }
        [Obsolete("IslandId is not universal, get from position instead", true)]
        public static IslandId DecodeIslandId(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            return (IslandId)IslandIdConstructor.Invoke(new object[] { reader.ReadUInt32() });
        }
        public static void Encode(IPlacementData placementData, Stream stream)
        {
            if (placementData is ConcurrentPlacementData concurrentPlacementData)
            {
                stream.WriteByte((byte)PlacementDataTypes.ConcurrentPlacementData);
                Encode(concurrentPlacementData, stream);
            }
            else if (placementData is FlatPlacementData flatPlacementData)
            {
                stream.WriteByte((byte)PlacementDataTypes.FlatPlacementData);
                Encode(flatPlacementData, stream);
            }
            else if (placementData is OverlappingPlacementData overlappingPlacementData)
            {
                stream.WriteByte((byte)PlacementDataTypes.OverlappingPlacementData);
                Encode(overlappingPlacementData, stream);
            }
        }
        public static IPlacementData DecodePlacementData(Stream stream)
        {
            return (PlacementDataTypes)stream.ReadByte() switch
            {
                PlacementDataTypes.ConcurrentPlacementData => DecodeConcurrentPlacementData(stream),
                PlacementDataTypes.FlatPlacementData => DecodeFlatPlacementData(stream),
                PlacementDataTypes.OverlappingPlacementData => DecodeOverlappingPlacementData(stream),
                _ => null,
            };
        }
        public static void Encode(ConcurrentPlacementData concurrentPlacementData, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(concurrentPlacementData.MaxBuildingIndex);
            writer.Write(concurrentPlacementData.MaxIslandIndex);
            writer.Write(concurrentPlacementData.CostBlueprintPoints);
            Encode(concurrentPlacementData.BlueprintCost, stream);
            Encode(concurrentPlacementData.ChunkCost, stream);
            writer.Write(concurrentPlacementData.CanAffordBlueprintCost);
            writer.Write(concurrentPlacementData.CanFitChunkLimit);
            writer.Write(concurrentPlacementData.IslandsCount);
            writer.Write(concurrentPlacementData.BuildingsCount);
            var _ExtraBuildingsToRemovePositions = (HashSet<GlobalTileCoordinate>)ConcurrentPlacementData_ExtraBuildingsToRemovePositionsInfo.GetValue(concurrentPlacementData);
            writer.Write(_ExtraBuildingsToRemovePositions.Count);
            foreach (var coord in _ExtraBuildingsToRemovePositions)
            {
                Encode(coord, stream);
            }
            var _ExtraIslandsToRemovePositions = (HashSet<GlobalChunkCoordinate>)ConcurrentPlacementData_ExtraIslandsToRemovePositionsInfo.GetValue(concurrentPlacementData);
            writer.Write(_ExtraIslandsToRemovePositions.Count);
            foreach (var coord in _ExtraIslandsToRemovePositions)
            {
                Encode(coord, stream);
            }
            //_AdditionalData unsupported
            var _IslandsMap = (DashMap<GlobalChunkCoordinate, IslandPlacement>)ConcurrentPlacementData_IslandsMapInfo.GetValue(concurrentPlacementData);
            writer.Write(_IslandsMap.Count);
            foreach (var kvp in _IslandsMap.GetAllKVPs())
            {
                Encode(kvp.Key, stream);
                Encode(kvp.Value, stream);
            }
            var _BuildingsMap = (DashMap<GlobalTileCoordinate, BuildingPlacement>)ConcurrentPlacementData_BuildingsMapInfo.GetValue(concurrentPlacementData);
            writer.Write(_BuildingsMap.Count);
            foreach (var kvp in _BuildingsMap.GetAllKVPs())
            {
                Encode(kvp.Key, stream);
                Encode(kvp.Value, stream);
            }
        }
        public static readonly PropertyInfo ConcurrentPlacementDataMaxBuildingIndex = AccessTools.Property(typeof(ConcurrentPlacementData), nameof(ConcurrentPlacementData.MaxBuildingIndex));
        public static readonly PropertyInfo ConcurrentPlacementDataMaxIslandIndex = AccessTools.Property(typeof(ConcurrentPlacementData), nameof(ConcurrentPlacementData.MaxIslandIndex));
        public static readonly PropertyInfo ConcurrentPlacementDataIslandsCount = AccessTools.Property(typeof(ConcurrentPlacementData), nameof(ConcurrentPlacementData.IslandsCount));
        public static readonly PropertyInfo ConcurrentPlacementDataBuildingsCount = AccessTools.Property(typeof(ConcurrentPlacementData), nameof(ConcurrentPlacementData.BuildingsCount));
        public static ConcurrentPlacementData DecodeConcurrentPlacementData(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var concurrentPlacementData = new ConcurrentPlacementData();
            ConcurrentPlacementDataMaxBuildingIndex.SetValue(concurrentPlacementData, reader.ReadInt32());
            ConcurrentPlacementDataMaxIslandIndex.SetValue(concurrentPlacementData, reader.ReadInt32());
            concurrentPlacementData.CostBlueprintPoints = reader.ReadBoolean();
            concurrentPlacementData.BlueprintCost = DecodeBlueprintCurrency(stream);
            concurrentPlacementData.ChunkCost = DecodeChunkLimitCurrency(stream);
            concurrentPlacementData.CanAffordBlueprintCost = reader.ReadBoolean();
            concurrentPlacementData.CanFitChunkLimit = reader.ReadBoolean();
            ConcurrentPlacementDataIslandsCount.SetValue(concurrentPlacementData, reader.ReadInt32());
            ConcurrentPlacementDataBuildingsCount.SetValue(concurrentPlacementData, reader.ReadInt32());
            var _ExtraBuildingsToRemovePositions = (HashSet<GlobalTileCoordinate>)ConcurrentPlacementData_ExtraBuildingsToRemovePositionsInfo.GetValue(concurrentPlacementData);
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                _ExtraBuildingsToRemovePositions.Add(DecodeGlobalTileCoordinate(stream));
            }
            var _ExtraIslandsToRemovePositions = (HashSet<GlobalChunkCoordinate>)ConcurrentPlacementData_ExtraIslandsToRemovePositionsInfo.GetValue(concurrentPlacementData);
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                _ExtraIslandsToRemovePositions.Add(DecodeGlobalChunkCoordinate(stream));
            }
            var _IslandsMap = (DashMap<GlobalChunkCoordinate, IslandPlacement>)ConcurrentPlacementData_IslandsMapInfo.GetValue(concurrentPlacementData);
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                _IslandsMap.Add(DecodeGlobalChunkCoordinate(stream), DecodeIslandPlacement(stream));
            }
            var _BuildingsMap = (DashMap<GlobalTileCoordinate, BuildingPlacement>)ConcurrentPlacementData_BuildingsMapInfo.GetValue(concurrentPlacementData);
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                _BuildingsMap.Add(DecodeGlobalTileCoordinate(stream), DecodeBuildingPlacement(stream));
            }
            return concurrentPlacementData;
        }
        public static void Encode(FlatPlacementData flatPlacementData, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(flatPlacementData.MaxBuildingIndex);
            writer.Write(flatPlacementData.MaxIslandIndex);
            writer.Write(flatPlacementData.CostBlueprintPoints);
            Encode(flatPlacementData.BlueprintCost, stream);
            Encode(flatPlacementData.ChunkCost, stream);
            writer.Write(flatPlacementData.CanAffordBlueprintCost);
            writer.Write(flatPlacementData.CanFitChunkLimit);
            var _ExtraBuildingsToRemovePositions = (HashSet<GlobalTileCoordinate>)FlatPlacementData_ExtraBuildingsToRemovePositionsInfo.GetValue(flatPlacementData);
            writer.Write(_ExtraBuildingsToRemovePositions.Count);
            foreach (var coord in _ExtraBuildingsToRemovePositions)
            {
                Encode(coord, stream);
            }
            var _ExtraIslandsToRemovePositions = (HashSet<GlobalChunkCoordinate>)FlatPlacementData_ExtraIslandsToRemovePositionsInfo.GetValue(flatPlacementData);
            writer.Write(_ExtraIslandsToRemovePositions.Count);
            foreach (var coord in _ExtraIslandsToRemovePositions)
            {
                Encode(coord, stream);
            }
            //_AdditionalData unsupported
            var _Buildings = (ScopedList<BuildingPlacement>)FlatPlacementData_Buildings.GetValue(flatPlacementData);
            writer.Write(_Buildings.Count);
            foreach (var building in _Buildings)
            {
                Encode(building, stream);
            }
            var _Islands = (ScopedList<IslandPlacement>)FlatPlacementData_Islands.GetValue(flatPlacementData);
            writer.Write(_Islands.Count);
            foreach (var island in _Islands)
            {
                Encode(island, stream);
            }
            var BuildingIndexMap = (ScopedDictionary<GlobalTileCoordinate, int>)FlatPlacementDataBuildingIndexMap.GetValue(flatPlacementData);
            writer.Write(BuildingIndexMap.Count);
            foreach (var kvp in BuildingIndexMap)
            {
                Encode(kvp.Key, stream);
                writer.Write(kvp.Value);
            }
            var IslandIndexMap = (ScopedDictionary<GlobalChunkCoordinate, int>)FlatPlacementDataIslandIndexMap.GetValue(flatPlacementData);
            writer.Write(IslandIndexMap.Count);
            foreach (var kvp in IslandIndexMap)
            {
                Encode(kvp.Key, stream);
                writer.Write(kvp.Value);
            }
        }
        public static readonly PropertyInfo FlatPlacementDataMaxBuildingIndex = AccessTools.Property(typeof(FlatPlacementData), nameof(FlatPlacementData.MaxBuildingIndex));
        public static readonly PropertyInfo FlatPlacementDataMaxIslandIndex = AccessTools.Property(typeof(FlatPlacementData), nameof(FlatPlacementData.MaxIslandIndex));
        public static FlatPlacementData DecodeFlatPlacementData(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var flatPlacementData = new FlatPlacementData(Shapez2Multiplayer.logger);
            FlatPlacementDataMaxBuildingIndex.SetValue(flatPlacementData, reader.ReadInt32());
            FlatPlacementDataMaxIslandIndex.SetValue(flatPlacementData, reader.ReadInt32());
            flatPlacementData.CostBlueprintPoints = reader.ReadBoolean();
            flatPlacementData.BlueprintCost = DecodeBlueprintCurrency(stream);
            flatPlacementData.ChunkCost = DecodeChunkLimitCurrency(stream);
            flatPlacementData.CanAffordBlueprintCost = reader.ReadBoolean();
            flatPlacementData.CanFitChunkLimit = reader.ReadBoolean();
            var _ExtraBuildingsToRemovePositions = (HashSet<GlobalTileCoordinate>)FlatPlacementData_ExtraBuildingsToRemovePositionsInfo.GetValue(flatPlacementData);
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                _ExtraBuildingsToRemovePositions.Add(DecodeGlobalTileCoordinate(stream));
            }
            var _ExtraIslandsToRemovePositions = (HashSet<GlobalChunkCoordinate>)FlatPlacementData_ExtraIslandsToRemovePositionsInfo.GetValue(flatPlacementData);
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                _ExtraIslandsToRemovePositions.Add(DecodeGlobalChunkCoordinate(stream));
            }
            var _Buildings = (ScopedList<BuildingPlacement>)FlatPlacementData_Buildings.GetValue(flatPlacementData);
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                _Buildings.Add(DecodeBuildingPlacement(stream));
            }
            var _Islands = (ScopedList<IslandPlacement>)FlatPlacementData_Islands.GetValue(flatPlacementData);
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                _Islands.Add(DecodeIslandPlacement(stream));
            }
            var BuildingIndexMap = (ScopedDictionary<GlobalTileCoordinate, int>)FlatPlacementDataBuildingIndexMap.GetValue(flatPlacementData);
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                BuildingIndexMap.Add(DecodeGlobalTileCoordinate(stream), reader.ReadInt32());
            }
            var IslandIndexMap = (ScopedDictionary<GlobalChunkCoordinate, int>)FlatPlacementDataIslandIndexMap.GetValue(flatPlacementData);
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                IslandIndexMap.Add(DecodeGlobalChunkCoordinate(stream), reader.ReadInt32());
            }
            return flatPlacementData;
        }
        public static readonly PropertyInfo OverlappingPlacementDataMaxBuildingIndex = AccessTools.Property(typeof(OverlappingPlacementData), nameof(OverlappingPlacementData.MaxBuildingIndex));
        public static readonly PropertyInfo OverlappingPlacementDataMaxIslandIndex = AccessTools.Property(typeof(OverlappingPlacementData), nameof(OverlappingPlacementData.MaxIslandIndex));
        public static void Encode(OverlappingPlacementData overlappingPlacementData, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(overlappingPlacementData.MaxBuildingIndex);
            writer.Write(overlappingPlacementData.MaxIslandIndex);
            writer.Write(overlappingPlacementData.CostBlueprintPoints);
            Encode(overlappingPlacementData.BlueprintCost, stream);
            Encode(overlappingPlacementData.ChunkCost, stream);
            writer.Write(overlappingPlacementData.CanAffordBlueprintCost);
            writer.Write(overlappingPlacementData.CanFitChunkLimit);
            var _ExtraBuildingsToRemovePositions = (HashSet<GlobalTileCoordinate>)OverlappingPlacementData_ExtraBuildingsToRemovePositionsInfo.GetValue(overlappingPlacementData);
            writer.Write(_ExtraBuildingsToRemovePositions.Count);
            foreach (var coord in _ExtraBuildingsToRemovePositions)
            {
                Encode(coord, stream);
            }
            var _ExtraIslandsToRemovePositions = (HashSet<GlobalChunkCoordinate>)OverlappingPlacementData_ExtraIslandsToRemovePositionsInfo.GetValue(overlappingPlacementData);
            writer.Write(_ExtraIslandsToRemovePositions.Count);
            foreach (var coord in _ExtraIslandsToRemovePositions)
            {
                Encode(coord, stream);
            }
            //_AdditionalData unsupported
            var _IslandsMap = (MultiValueDictionary<GlobalChunkCoordinate, IslandPlacement, ScopedHashSet<IslandPlacement>>)OverlappingPlacementData_IslandsMapInfo.GetValue(overlappingPlacementData);
            writer.Write(_IslandsMap.KeyCount);
            foreach (var key in _IslandsMap.Keys)
            {
                Encode(key, stream);
                var collection = _IslandsMap[key];
                writer.Write(collection.Count);
                foreach (var value in collection)
                {
                    Encode(value, stream);
                }
            }
            var _BuildingsMap = (MultiValueDictionary<GlobalTileCoordinate, BuildingPlacement, ScopedHashSet<BuildingPlacement>>)OverlappingPlacementData_BuildingsMapInfo.GetValue(overlappingPlacementData);
            writer.Write(_BuildingsMap.KeyCount);
            foreach (var key in _BuildingsMap.Keys)
            {
                Encode(key, stream);
                var collection = _BuildingsMap[key];
                writer.Write(collection.Count);
                foreach (var value in collection)
                {
                    Encode(value, stream);
                }
            }
        }
        public static OverlappingPlacementData DecodeOverlappingPlacementData(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var overlappingPlacementData = new OverlappingPlacementData();
            OverlappingPlacementDataMaxBuildingIndex.SetValue(overlappingPlacementData, reader.ReadInt32());
            OverlappingPlacementDataMaxIslandIndex.SetValue(overlappingPlacementData, reader.ReadInt32());
            overlappingPlacementData.CostBlueprintPoints = reader.ReadBoolean();
            overlappingPlacementData.BlueprintCost = DecodeBlueprintCurrency(stream);
            overlappingPlacementData.ChunkCost = DecodeChunkLimitCurrency(stream);
            overlappingPlacementData.CanAffordBlueprintCost = reader.ReadBoolean();
            overlappingPlacementData.CanFitChunkLimit = reader.ReadBoolean();
            var _ExtraBuildingsToRemovePositions = (HashSet<GlobalTileCoordinate>)OverlappingPlacementData_ExtraBuildingsToRemovePositionsInfo.GetValue(overlappingPlacementData);
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                _ExtraBuildingsToRemovePositions.Add(DecodeGlobalTileCoordinate(stream));
            }
            var _ExtraIslandsToRemovePositions = (HashSet<GlobalChunkCoordinate>)OverlappingPlacementData_ExtraIslandsToRemovePositionsInfo.GetValue(overlappingPlacementData);
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                _ExtraIslandsToRemovePositions.Add(DecodeGlobalChunkCoordinate(stream));
            }
            var _IslandsMap = (MultiValueDictionary<GlobalChunkCoordinate, IslandPlacement, ScopedHashSet<IslandPlacement>>)OverlappingPlacementData_IslandsMapInfo.GetValue(overlappingPlacementData);
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var key = DecodeGlobalChunkCoordinate(stream);
                var count2 = reader.ReadInt32();
                for (int v = 0; v < count2; v++)
                {
                    _IslandsMap.AddValue(key, DecodeIslandPlacement(stream));
                }
            }
            var _BuildingsMap = (MultiValueDictionary<GlobalTileCoordinate, BuildingPlacement, ScopedHashSet<BuildingPlacement>>)OverlappingPlacementData_BuildingsMapInfo.GetValue(overlappingPlacementData);
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var key = DecodeGlobalTileCoordinate(stream);
                var count2 = reader.ReadInt32();
                for (int v = 0; v < count2; v++)
                {
                    _BuildingsMap.AddValue(key, DecodeBuildingPlacement(stream));
                }
            }
            return overlappingPlacementData;
        }
        public static void Encode(BuildingPlacement buildingPlacement, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            Encode(buildingPlacement.Descriptor, stream);
            stream.WriteByte((byte)buildingPlacement.PlacementAllowability);
            writer.Write(buildingPlacement.PlacementIndex);
        }
        public static BuildingPlacement DecodeBuildingPlacement(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            //return new BuildingPlacement(DecodeBuildingDescriptor(stream), (PlacementAllowability)stream.ReadByte(), reader.ReadInt32());
            var descriptor = DecodeBuildingDescriptor(stream);
            var allowability = (PlacementAllowability)stream.ReadByte();
            var index = reader.ReadInt32();
            return new BuildingPlacement(descriptor, allowability, index);
        }
        public static void Encode(IslandPlacement islandPlacement, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            Encode(islandPlacement.Descriptor, stream);
            stream.WriteByte((byte)islandPlacement.PlacementAllowability);
            writer.Write(islandPlacement.PlacementIndex);
        }
        public static IslandPlacement DecodeIslandPlacement(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            return new IslandPlacement(DecodeIslandDescriptor(stream), (PlacementAllowability)stream.ReadByte(), reader.ReadInt32());
        }
        public static readonly FieldInfo SimulationStateContainerStateInfo = AccessTools.Field(typeof(SimulationStateContainer), "State");
        public static void Encode(IslandDescriptor islandDescriptor, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            Encode((IslandDefinition)islandDescriptor.Definition, stream);
            Encode(islandDescriptor.Transform, stream);
            //Encode(islandDescriptor.Configuration, stream);
            writer.Write(islandDescriptor.Configuration != null);
            if (islandDescriptor.Configuration != null)
            {
                writer.Write(islandDescriptor.Configuration.GetType().AssemblyQualifiedName);
                islandDescriptor.Configuration.Sync(serializationVisitor);
            }
            //Encode(islandDescriptor.State, stream);
            var state = (ISimulationState)SimulationStateContainerStateInfo.GetValue(islandDescriptor.State);
            writer.Write(state != null);
            if (state != null) islandDescriptor.State.Sync(serializationVisitor);
        }
        public static IslandDescriptor DecodeIslandDescriptor(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var type = DecodeIslandDefinition(stream);
            var tranform = DecodeGlobalChunkTransform(stream);
            IIslandConfiguration configuration = null;
            if (reader.ReadBoolean())
            {
                configuration = (IIslandConfiguration)Activator.CreateInstance(Type.GetType(reader.ReadString()));
                configuration.Sync(serializationVisitor);
            }
            var state = new SimulationStateContainer();
            if (reader.ReadBoolean()) state.Sync(serializationVisitor);
            return new IslandDescriptor(type, tranform, configuration, state);
        }
        public static void Encode(BuildingDescriptor buildingDescriptor, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            Encode((BuildingDefinition)buildingDescriptor.Definition, stream);
            Encode(buildingDescriptor.Transform, stream);
            //Encode(buildingDescriptor.Configuration, stream);
            writer.Write(buildingDescriptor.Configuration != null);
            if (buildingDescriptor.Configuration != null)
            {
                writer.Write(buildingDescriptor.Configuration.GetType().AssemblyQualifiedName);
                buildingDescriptor.Configuration.Sync(serializationVisitor);
            }
            //Encode(buildingDescriptor.State, stream);
            var state = (ISimulationState)SimulationStateContainerStateInfo.GetValue(buildingDescriptor.State);
            writer.Write(state != null);
            if (state != null) buildingDescriptor.State.Sync(serializationVisitor);
        }
        public static BuildingDescriptor DecodeBuildingDescriptor(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var type = DecodeBuildingDefinition(stream);
            var tranform = DecodeGlobalTileTransform(stream);
            IBuildingConfiguration configuration = null;
            if (reader.ReadBoolean())
            {
                configuration = (IBuildingConfiguration)Activator.CreateInstance(Type.GetType(reader.ReadString()));
                configuration.Sync(serializationVisitor);
            }
            var state = new SimulationStateContainer();
            if (reader.ReadBoolean()) state.Sync(serializationVisitor);
            return new BuildingDescriptor(type, tranform, configuration, state);
        }
        public static void Encode(GlobalTileTransform globalTileTransform, Stream stream)
        {
            Encode(globalTileTransform.Position, stream);
            Encode(globalTileTransform.Rotation, stream);
        }
        public static GlobalTileTransform DecodeGlobalTileTransform(Stream stream)
        {
            return new GlobalTileTransform(DecodeGlobalTileCoordinate(stream), DecodeGridRotation(stream));
        }
        public static void Encode(GlobalTilePivot globalTilePivot, Stream stream)
        {
            Encode(globalTilePivot.Position, stream);
            Encode(globalTilePivot.Direction, stream);
        }
        public static GlobalTilePivot DecodeGlobalTilePivot(Stream stream)
        {
            return new GlobalTilePivot(DecodeGlobalTileCoordinate(stream), DecodeTileDirection(stream));
        }
        public static void Encode(GlobalChunkPivot globalChunkPivot, Stream stream)
        {
            Encode(globalChunkPivot.Position, stream);
            Encode(globalChunkPivot.Direction, stream);
        }
        public static GlobalChunkPivot DecodeGlobalChunkPivot(Stream stream)
        {
            return new GlobalChunkPivot(DecodeGlobalChunkCoordinate(stream), DecodeChunkDirection(stream));
        }
        public static void Encode(TileDirection tileDirection, Stream stream)
        {
            stream.WriteByte((byte)tileDirection.Value);
        }
        public static TileDirection DecodeTileDirection(Stream stream)
        {
            return new TileDirection((byte)stream.ReadByte());
        }
        public static void Encode(IIslandConfiguration islandConfiguration, Stream stream)
        {
            if (islandConfiguration is ExchangeLayerFilterConfig exchangeLayerFilterConfig)
            {
                stream.WriteByte((byte)IslandConfigurationTypes.ExchangeLayerFilterConfig);
                //Encode(exchangeLayerFilterConfig, stream);
            }
            else if (islandConfiguration is RailConfiguration railConfiguration)
            {
                stream.WriteByte((byte)IslandConfigurationTypes.RailConfiguration);
                //Encode(railConfiguration, stream);
            }
            islandConfiguration.Sync(serializationVisitor);
        }
        public static IIslandConfiguration DecodeIIslandConfiguration(Stream stream)
        {
            switch ((IslandConfigurationTypes)stream.ReadByte())
            {
                case IslandConfigurationTypes.ExchangeLayerFilterConfig:
                    var exchangeLayerFilterConfig = new ExchangeLayerFilterConfig();
                    exchangeLayerFilterConfig.Sync(serializationVisitor);
                    return exchangeLayerFilterConfig;
                case IslandConfigurationTypes.RailConfiguration:
                    var railConfiguration = new RailConfiguration();
                    railConfiguration.Sync(serializationVisitor);
                    return railConfiguration;
                default:
                    return null;
            }
        }
        //public static readonly FieldInfo ExchangeLayerFilterConfigBlockedExchangesBitMask = AccessTools.Field(typeof(ExchangeLayerFilterConfig), "BlockedExchangesBitMask");
        //public static void Encode(ExchangeLayerFilterConfig exchangeLayerFilterConfig, Stream stream)
        //{
        //    using (BinaryWriter writer = new BinaryWriter(stream))
        //    {
        //        writer.Write((int)ExchangeLayerFilterConfigBlockedExchangesBitMask.GetValue(exchangeLayerFilterConfig));
        //    }
        //}
        //public static void Encode(RailConfiguration railConfiguration, Stream stream)
        //{
        //    using (BinaryWriter writer = new BinaryWriter(stream))
        //    {
        //        writer.Write(railConfiguration.ColorFiltersPerConnection.Length);
        //        foreach (var colorFilter in railConfiguration.ColorFiltersPerConnection)
        //        {
        //            Encode(colorFilter, stream);
        //        }
        //    }
        //}
        //public static void Encode(RailColorFilter railColorFilter, Stream stream)
        //{
        //    using (BinaryWriter writer = new BinaryWriter(stream))
        //    {
        //        writer.Write(railColorFilter)
        //    }
        //}
        enum IslandConfigurationTypes
        {
            ExchangeLayerFilterConfig,
            RailConfiguration
        }
        public static void Encode(GlobalChunkTransform globalChunkTransform, Stream stream)
        {
            Encode(globalChunkTransform.Position, stream);
            Encode(globalChunkTransform.Rotation, stream);
        }
        public static GlobalChunkTransform DecodeGlobalChunkTransform(Stream stream)
        {
            return new GlobalChunkTransform(DecodeGlobalChunkCoordinate(stream), DecodeGridRotation(stream));
        }
        public static void Encode(IslandDefinition islandDefinition, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            Encode(islandDefinition.Id, stream);
            //writer.Write(islandDefinition.Layout.Count);
            //foreach (var key in islandDefinition.Layout.GetChunkPositions())
            //{
            //    var value = islandDefinition.Layout.GetChunk_IC(key);
            //    Encode(key);
            //    Encode(value, stream);
            //}
            //if needed to be re-implimented at any point for some reason, CustomData would need to be added
        }
        public static IslandDefinition DecodeIslandDefinition(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var id = DecodeIslandDefinitionId(stream);
            //var layoutData = new List<KeyValuePair<ChunkVector, IslandChunkData>>();
            //var count = reader.ReadInt32();
            //for (int i = 0; i < count; i++)
            //{
            //    layoutData.Add(new KeyValuePair<ChunkVector, IslandChunkData>((ChunkVector)bf.Deserialize(stream), DecodeIslandChunkData(stream)));
            //}
            //var layout = new ChunkLayoutLookup<ChunkVector, IslandChunkData>(layoutData);
            //return new IslandDefinition(id, layout);
            return (IslandDefinition)Shapez2Multiplayer.Mode.Islands.GetDefinition(id);
        }
        public static void Encode(BuildingDefinition buildingDefinition, Stream stream)
        {
            Encode(buildingDefinition.Id, stream);
            //Encode((BuildingConnectorData)buildingDefinition.ConnectorData, stream);
            //if needed to be re-implimented at any point for some reason, CustomData would need to be added
        }
        public static BuildingDefinition DecodeBuildingDefinition(Stream stream)
        {
            //return new BuildingDefinition(DecodeBuildingDefinitionId(stream), DecodeBuildingConnectorData(stream));
            return (BuildingDefinition)Shapez2Multiplayer.Mode.Buildings.GetDefinition(DecodeBuildingDefinitionId(stream));
        }
        //public static readonly FieldInfo BuildingConnectorDataConnectionsByPivot = AccessTools.Field(typeof(BuildingConnectorData), "ConnectionsByPivot");
        //public static readonly FieldInfo BuildingConnectorDataLegacyBuildingIOMap = AccessTools.Field(typeof(BuildingConnectorData), "LegacyBuildingIOMap");
        //public static readonly FieldInfo BuildingConnectorDataBuildingIOMap = AccessTools.Field(typeof(BuildingConnectorData), "BuildingIOMap");
        //public static void Encode(BuildingConnectorData buildingConnectorData, Stream stream)
        //{
        //    using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
        //    writer.Write(buildingConnectorData.AllBuildingConnectors.Length);
        //    foreach (var buildingIO in buildingConnectorData.AllBuildingConnectors)
        //    {
        //        //writer.Write(buildingIO.GetType().FullName);
        //        Encode(buildingIO);
        //    }
        //    writer.Write(buildingConnectorData.Tiles.Length);
        //    foreach (var tile in buildingConnectorData.Tiles)
        //    {
        //        Encode(tile);
        //    }
        //    Encode(buildingConnectorData.TileBounds, stream);
        //    Encode(buildingConnectorData.TileBoundsCenter);
        //    Encode(buildingConnectorData.TileDimensions, stream);
        //    //var ConnectionsByPivot = (Dictionary<LocalTilePivot, IBuildingIO>)BuildingConnectorDataConnectionsByPivot.GetValue(buildingConnectorData);
        //    //writer.Write(ConnectionsByPivot.Count);
        //    //foreach (var kvp in ConnectionsByPivot)
        //    //{
        //    //    Encode(kvp.Key, stream);
        //    //    writer.Write(kvp.Value.GetType().FullName);
        //    //    Encode(kvp.Value);
        //    //}
        //    //var LegacyBuildingIOMap = (ConcurrentDictionary<Type, IReadOnlyList<IBuildingIO>>)BuildingConnectorDataLegacyBuildingIOMap.GetValue(buildingConnectorData);
        //    //writer.Write(LegacyBuildingIOMap.Count);
        //    //foreach (var kvp in LegacyBuildingIOMap)
        //    //{
        //    //    writer.Write(kvp.Key.FullName);
        //    //    writer.Write(kvp.Value.Count);
        //    //}
        //}
        //public static BuildingConnectorData DecodeBuildingConnectorData(Stream stream)
        //{
        //    using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
        //    var count = reader.ReadInt32();
        //    //var allInputs = new IBuildingIO[reader.ReadInt32()];
        //    var allInputs = new IBuildingIO[count];
        //    for (int i = 0; i < allInputs.Length; i++)
        //    {
        //        allInputs[i] = (IBuildingIO)bf.Deserialize(stream);
        //    }
        //    var tiles = new TileVector[reader.ReadInt32()];
        //    for (int i = 0; i < tiles.Length; i++)
        //    {
        //        tiles[i] = (TileVector)bf.Deserialize(stream);
        //    }
        //    return new BuildingConnectorData(allInputs, tiles, DecodeLocalTileBounds(stream), (LocalVector)bf.Deserialize(stream), DecodeTileDimensions(stream));
        //}
        //public static void Encode(LocalTilePivot localTilePivot, Stream stream)
        //{
        //    Encode(localTilePivot.Position);
        //    Encode(localTilePivot.Direction, stream);
        //}
        //public static LocalTilePivot DecodeLocalTilePivot(Stream stream)
        //{
        //    return new LocalTilePivot((TileVector)bf.Deserialize(stream), DecodeTileDirection(stream));
        //}
        //public static void Encode(TileDirection tileDirection, Stream stream)
        //{
        //    stream.WriteByte((byte)tileDirection.Value);
        //}
        //public static TileDirection DecodeTileDirection(Stream stream)
        //{
        //    return new TileDirection((byte)stream.ReadByte());
        //}
        //public static void Encode(TileDimensions tileDimensions, Stream stream)
        //{
        //    using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
        //    writer.Write(tileDimensions.x);
        //    writer.Write(tileDimensions.y);
        //    writer.Write(tileDimensions.z);
        //}
        //public static TileDimensions DecodeTileDimensions(Stream stream)
        //{
        //    using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
        //    return new TileDimensions(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
        //}
        //public static void Encode(LocalTileBounds localTileBounds, Stream stream)
        //{
        //    Encode(localTileBounds.Min);
        //    Encode(localTileBounds.Max);
        //}
        //public static LocalTileBounds DecodeLocalTileBounds(Stream stream)
        //{
        //    return new LocalTileBounds((TileVector)bf.Deserialize(stream), (TileVector)bf.Deserialize(stream));
        //}
        public static void Encode(IslandDefinitionId islandDefinitionId, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(islandDefinitionId.Name);
        }
        public static IslandDefinitionId DecodeIslandDefinitionId(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            return new IslandDefinitionId(reader.ReadString());
        }
        public static void Encode(BuildingDefinitionId buildingDefinitionId, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            writer.Write(buildingDefinitionId.Name);
        }
        public static BuildingDefinitionId DecodeBuildingDefinitionId(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            return new BuildingDefinitionId(reader.ReadString());
        }
        public static void Encode(IslandChunkData islandChunkData, Stream stream)
        {
            using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            Encode(islandChunkData.Chunk_L, stream);
            writer.Write(islandChunkData.Notches.Length);
            foreach (var notch in islandChunkData.Notches)
            {
                Encode(notch, stream);
            }
            writer.Write(islandChunkData.TileNotchFlags_L.Length);
            foreach (var notchFlag in islandChunkData.TileNotchFlags_L)
            {
                writer.Write(notchFlag != null);
                if (notchFlag != null)
                {
                    Encode(notchFlag.Value, stream);
                }
            }
            writer.Write(islandChunkData.IsBuildable);
            writer.Write(islandChunkData.TileBuildableFlags_L.Length);
            foreach (var buildableFlag in islandChunkData.TileBuildableFlags_L)
            {
                writer.Write(buildableFlag);
            }
            writer.Write(islandChunkData.EdgeTypes.Length);
            foreach (var type in islandChunkData.EdgeTypes)
            {
                stream.WriteByte((byte)type);
            }
            writer.Write(islandChunkData.TileVoidFlags_L.Length);
            foreach (var voidFlag in islandChunkData.TileVoidFlags_L)
            {
                writer.Write(voidFlag);
            }
            writer.Write(islandChunkData.NotchDirections_L.Length);
            foreach (var direction in islandChunkData.NotchDirections_L)
            {
                Encode(direction, stream);
            }
        }
        public static IslandChunkData DecodeIslandChunkData(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            var tile_IC = DecodeChunkVector(stream);
            var notches = new ChunkDirection[reader.ReadInt32()];
            for (int i = 0; i < notches.Length; i++)
            {
                notches[i] = DecodeChunkDirection(stream);
            }
            var tileNotchFlags_L = new ChunkDirection?[reader.ReadInt32()];
            for (int i = 0; i < tileNotchFlags_L.Length; i++)
            {
                if (reader.ReadBoolean())
                {
                    tileNotchFlags_L[i] = DecodeChunkDirection(stream);
                }
                else
                {
                    tileNotchFlags_L[i] = null;
                }
            }
            var isBuildable = reader.ReadBoolean();
            var tileBuildableFlags_L = new bool[reader.ReadInt32()];
            for (int i = 0; i < tileBuildableFlags_L.Length; i++)
            {
                tileBuildableFlags_L[i] = reader.ReadBoolean();
            }
            var edgeTypes = new ChunkEdgeType[reader.ReadInt32()];
            for (int i = 0; i < edgeTypes.Length; i++)
            {
                edgeTypes[i] = (ChunkEdgeType)stream.ReadByte();
            }
            var islandChunkData = new IslandChunkData(tile_IC, notches, tileNotchFlags_L, isBuildable, tileBuildableFlags_L, edgeTypes);
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                islandChunkData.TileVoidFlags_L[i] = reader.ReadBoolean();
            }
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                islandChunkData.NotchDirections_L[i] = DecodeChunkDirection(stream);
            }
            return islandChunkData;
        }
        public static void Encode(ChunkDirection chunkDirection, Stream stream)
        {
            stream.WriteByte((byte)chunkDirection.Value);
        }
        public static ChunkDirection DecodeChunkDirection(Stream stream)
        {
            return new ChunkDirection((byte)stream.ReadByte());
        }
        public static readonly FieldInfo NotchDefinitionEffectiveRotation = AccessTools.Field(typeof(NotchDefinition), "EffectiveRotation");
        public static void Encode(NotchDefinition notchDefinition, Stream stream)
        {
            //using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
            //writer.Write(notchDefinition.NotchTiles_L.Length);
            //foreach (var notchTile in notchDefinition.NotchTiles_L)
            //{
            //    Encode(notchTile);
            //}
            //Encode(notchDefinition.NotchCenter_L);
            Encode(notchDefinition.Direction_L, stream);
            //Encode((GridRotation)NotchDefinitionEffectiveRotation.GetValue(notchDefinition), stream);
        }
        public static NotchDefinition DecodeNotchDefinition(Stream stream)
        {
            //using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
            //var notchTiles = new ChunkTileCoordinate[reader.ReadInt32()];
            //for (int i = 0; i < notchTiles.Length; i++)
            //{
            //    notchTiles = (ChunkTileCoordinate)bf.Deserialize(stream);
            //}
            //var notchDefinition = new NotchDefinition
            //{
            //    NotchTiles_L = notchTiles,
            //    NotchCenter_L = (ChunkTileCoordinate)bf.Deserialize(stream),
            //    Direction_L = DecodeChunkDirection(stream)
            //};
            //NotchDefinitionEffectiveRotation.SetValue(notchDefinition, DecodeGridRotation(stream));
            //return notchDefinition;
            return new NotchDefinition(DecodeChunkDirection(stream));
        }
        public static void Encode(GridRotation gridRotation, Stream stream)
        {
            stream.WriteByte((byte)gridRotation.Value);
        }
        public static GridRotation DecodeGridRotation(Stream stream)
        {
            return new GridRotation((byte)stream.ReadByte());
        }
        enum PlacementDataTypes : byte
        {
            ConcurrentPlacementData,
            FlatPlacementData,
            OverlappingPlacementData
        }
        //public static void Encode(DefaultPreferredPlacementMode defaultPreferredPlacementMode, Stream stream)
        //{
        //    stream.WriteByte((byte)defaultPreferredPlacementMode);
        //}
        //public static DefaultPreferredPlacementMode DecodeDefaultPreferredPlacementMode(Stream stream)
        //{
        //    return (DefaultPreferredPlacementMode)stream.ReadByte();
        //}
        //public static void Encode(EntityPlacementPreferenceData entityPlacementPreferenceData, Stream stream)
        //{
        //    using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
        //    writer.Write(entityPlacementPreferenceData.AutoSnapToConnectors);
        //    writer.Write(entityPlacementPreferenceData.ConnectorsAutoSnapScoreMultiplier);
        //}
        //public static EntityPlacementPreferenceData DecodeEntityPlacementPreferenceData(Stream stream)
        //{
        //    using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
        //    return new EntityPlacementPreferenceData(reader.ReadBoolean(), reader.ReadInt32());
        //}
        //public static void Encode(EntityReplacementPreferenceData entityReplacementPreferenceData, Stream stream)
        //{
        //    using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
        //    writer.Write(entityReplacementPreferenceData.AllowNonForcingReplacementByEntitiesInDifferentGroup);
        //    writer.Write(entityReplacementPreferenceData.IsTransportBuilding);
        //    writer.Write(entityReplacementPreferenceData.ShouldSkipReplacementIOChecks);
        //}
        //public static EntityReplacementPreferenceData DecodeEntityReplacementPreferenceData(Stream stream)
        //{
        //    using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
        //    return new EntityReplacementPreferenceData(reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadBoolean());
        //}
        //public static void Encode(BuildingDefinitionGroup buildingDefinitionGroup, Stream stream)
        //{
        //    //using BinaryWriter writer = new BinaryWriter(stream, UTF8Encoding.UTF8, leaveOpen: true);
        //    //probably don't need to encode group, just id
        //    Encode(buildingDefinitionGroup.Id);
        //}
        //public static BuildingDefinitionGroup DecodeBuildingDefinitionGroup(Stream stream)
        //{
        //    //using BinaryReader reader = new BinaryReader(stream, UTF8Encoding.UTF8, leaveOpen: true);
        //    var id = (BuildingDefinitionGroupId)bf.Deserialize(stream);
        //    return (BuildingDefinitionGroup)Shapez2Multiplayer.Mode.Buildings.All.First(building => building.Id == id);
        //}
    }
}
