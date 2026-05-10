using Core.Collections.Scoped;
using Core.Dependency;
using Core.Events;
using Game.Core.Coordinates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

namespace Shapez2Multiplayer
{
    public class OtherPlayerHUDIslandMassSelection : OtherPlayerHUDMassSelectionBase<IslandModel, GlobalChunkCoordinate>
    {
        public OtherPlayerHUDIslandMassSelection(IMapModel map, IIslandPreviewDrawer islandPreviewDrawer)
        {
            this.Map = map;
            this.IslandPreviewDrawer = islandPreviewDrawer;
        }
        public void Update(HUDIslandMassSelection hudIslandMassSelection)
        {
            Update((GlobalChunkCoordinate?)Encoding.HUDIslandMassSelectionAreaSelectionEnd_GInfo.GetValue(hudIslandMassSelection),
                (GlobalChunkCoordinate?)Encoding.HUDIslandMassSelectionAreaSelectionStart_GInfo.GetValue(hudIslandMassSelection),
                (HUDMassSelectionMode)Encoding.HUDIslandMassSelectionAreaCurrentModeInfo.GetValue(hudIslandMassSelection),
                (HashSet<IslandModel>)Encoding.HUDIslandMassSelectionAreaPendingSelectionInfo.GetValue(hudIslandMassSelection),
                HoverAnimationsFromIList((IList)Encoding.HUDIslandMassSelectionAreaHoverAnimationsInfo.GetValue(hudIslandMassSelection)));
        }
        protected override void Draw_AreaSelection(FrameDrawOptions options, GlobalChunkCoordinate from_GC, GlobalChunkCoordinate to_GC, HUDMassSelectionSelectionType type)
        {
            HUDMassSelectionDrawUtils.Draw_IslandAreaSelection(options, from_GC, to_GC, type, false);
        }
        protected override void Draw_ExistingSelection(FrameDrawOptions options, IReadOnlyCollection<IslandModel> selection)
        {
            using (ScopedList<IslandPreviewRenderData> scopedList = ScopedList<IslandPreviewRenderData>.Get())
            {
                foreach (IslandModel islandModel in selection)
                {
                    scopedList.Add(new IslandPreviewRenderData(islandModel.Definition, islandModel.Transform, islandModel.Configuration, PlacementAllowability.ValidPlacement));
                }
                this.IslandPreviewDrawer.Draw(this.Map.Layout, options, scopedList, false, false, false);
            }
        }
        protected override void Draw_HoverState(FrameDrawOptions options, IslandModel island, float alpha)
        {
            IReadOnlyList<ChunkVector> chunkPositions = island.LayoutQuery.ChunkLookup.GetChunkPositions();
            for (int i = 0; i < chunkPositions.Count; i++)
            {
                ChunkVector chunkVector = chunkPositions[i];
                GlobalChunkTransform transform = island.Transform;
                float3 @float = chunkVector.ToGlobal(in transform).ToCenter_W(-3.9f);
                float3 float2 = new float3(20);
                Matrix4x4 matrix4x = FastMatrix.TranslateScale(in @float, in float2);
                options.Draw3DPlaneWithProperties_SLOW_AVOID(options.Theme.BaseResources.UXIslandHoverMaterial, matrix4x, MaterialPropertyHelpers.CreateAlphaBlock(alpha));
            }
        }
        protected override void Draw_PendingSelection(FrameDrawOptions options, IReadOnlyCollection<IslandModel> entities, HUDMassSelectionSelectionType type)
        {
            PlacementAllowability placementAllowability;
            switch (type)
            {
                case HUDMassSelectionSelectionType.Select:
                    placementAllowability = PlacementAllowability.ValidPlacement;
                    break;
                case HUDMassSelectionSelectionType.Deselect:
                    placementAllowability = PlacementAllowability.ValidPlacementButDisplaysWarning;
                    break;
                case HUDMassSelectionSelectionType.Delete:
                    placementAllowability = PlacementAllowability.InvalidPlacement;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
            PlacementAllowability placementAllowability2 = placementAllowability;
            using (ScopedList<IslandPreviewRenderData> scopedList = ScopedList<IslandPreviewRenderData>.Get())
            {
                foreach (IslandModel islandModel in entities)
                {
                    scopedList.Add(new IslandPreviewRenderData(islandModel.Definition, islandModel.Transform, islandModel.Configuration, placementAllowability2));
                }
                this.IslandPreviewDrawer.Draw(this.Map.Layout, options, scopedList, false, false, false);
            }
        }
        private IIslandPreviewDrawer IslandPreviewDrawer;
        private IMapModel Map;
    }
}
