using Core.Collections.Scoped;
using Game.Core.Coordinates;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Shapez2Multiplayer
{
    public class OtherPlayerHUDIslandMassSelection : OtherPlayerHUDMassSelectionBase<IslandModel, GlobalChunkCoordinate>
    {
        public OtherPlayerHUDIslandMassSelection(IMapModel map, IIslandPreviewDrawer islandPreviewDrawer, IConnection? connection)
        {
            this.Map = map;
            this.IslandPreviewDrawer = islandPreviewDrawer;
            this.connection = connection;
        }
        public void Update(HUDIslandMassSelection hudIslandMassSelection)
        {
            Update(hudIslandMassSelection.AreaSelectionEnd_G,
                hudIslandMassSelection.AreaSelectionStart_G,
                hudIslandMassSelection.CurrentMode,
                hudIslandMassSelection.PendingSelection,
                HoverAnimationsFromIList(hudIslandMassSelection.HoverAnimations));
        }
        protected override void OnHover(IslandModel target)
        {

        }
        protected override IslandModel FindEntityBelowCursor()
        {
            var cursor = connection != null ? HUDMultiplayerCursors.Instance.GetOrAddCursor(connection) : HUDMultiplayerCursors.Instance.GetOrAddHostCursor();
            var screenPosition = (float2)ExtraScreenUtils.WorldToScreenPointDouble(Shapez2Multiplayer.GameSessionOrchestrator.Viewport, cursor.WorldPosition);
            if (screenPosition.x < 0 || screenPosition.y < 0 || screenPosition.x > Screen.width || screenPosition.y > Screen.height)
            {
                return IslandModel.Invalid;
            }
            SelectionUtils.TryFindIslandAtScreenPosition(Shapez2Multiplayer.GameSessionOrchestrator.Viewport, in screenPosition, Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer.CurrentMap, true, out var islandModel, out var globalChunkCoordinate, out var ray, out var num, cursor.ViewportIslandLayer, cursor.ViewportShowAllIslandLayers.HasValue ? (cursor.ViewportShowAllIslandLayers.Value ? Shapez2Multiplayer.GameSessionOrchestrator.LocalPlayer.CurrentMap.MaxIslandLayer : cursor.ViewportIslandLayer) : null);
            return islandModel;
        }
        protected override PlayerInteractionState GetTargetScopeState()
        {
            return PlayerInteractionState.IslandsIdle;
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
                options.Draw3DPlaneWithPerInstanceData(options.Theme.BaseResources.UXIslandHoverMaterial, matrix4x, new AlphaPerInstanceData(alpha));
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
