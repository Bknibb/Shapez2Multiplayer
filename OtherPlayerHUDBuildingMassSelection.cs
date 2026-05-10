using Core.Collections.Scoped;
using Core.Dependency;
using Game.Core.Coordinates;
using Game.Placement.Data;
using Game.Placement.Processing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace Shapez2Multiplayer
{
    public class OtherPlayerHUDBuildingMassSelection : OtherPlayerHUDMassSelectionBase<BuildingModel, GlobalTileCoordinate>
    {
        public OtherPlayerHUDBuildingMassSelection(Player player, IBuildingPlacementIndicatorAccessor buildingPlacementIndicators, ITutorialHighlightProvider tutorialHighlightProvider)
        {
            this.BuildingPlacementIndicatorDrawer = new BuildingPlacementIndicatorDrawer(player.CurrentMap, buildingPlacementIndicators, tutorialHighlightProvider);
        }
        public void Update(HUDBuildingMassSelection hudBuildingMassSelection)
        {
            Update((GlobalTileCoordinate?)Encoding.HUDBuildingMassSelectionAreaSelectionEnd_GInfo.GetValue(hudBuildingMassSelection),
                (GlobalTileCoordinate?)Encoding.HUDBuildingMassSelectionAreaSelectionStart_GInfo.GetValue(hudBuildingMassSelection),
                (HUDMassSelectionMode)Encoding.HUDBuildingMassSelectionAreaCurrentModeInfo.GetValue(hudBuildingMassSelection),
                (HashSet<BuildingModel>)Encoding.HUDBuildingMassSelectionAreaPendingSelectionInfo.GetValue(hudBuildingMassSelection),
                HoverAnimationsFromIList((IList)Encoding.HUDBuildingMassSelectionAreaHoverAnimationsInfo.GetValue(hudBuildingMassSelection)));
        }
        protected override void Draw_PendingSelection(FrameDrawOptions options, IReadOnlyCollection<BuildingModel> entities, HUDMassSelectionSelectionType type)
        {
            if (entities.Count == 0)
            {
                return;
            }
            using (ScopedList<SmartBuildingBlueprintRenderer.DrawData> scopedList = ScopedList<SmartBuildingBlueprintRenderer.DrawData>.Get())
            {
                using (OverlappingPlacementData overlappingPlacementData = new OverlappingPlacementData())
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
                            placementAllowability = PlacementAllowability.ValidPlacement;
                            break;
                    }
                    PlacementAllowability placementAllowability2 = placementAllowability;
                    int num = 0;
                    foreach (BuildingModel buildingModel in entities)
                    {
                        List<SmartBuildingBlueprintRenderer.DrawData> list = scopedList;
                        IBuildingDefinition definition = buildingModel.Definition;
                        GlobalTileTransform globalTileTransform = buildingModel.Transform;
                        list.Add(new SmartBuildingBlueprintRenderer.DrawData(definition, in globalTileTransform, placementAllowability2));
                        OverlappingPlacementData overlappingPlacementData2 = overlappingPlacementData;
                        IBuildingDefinition definition2 = buildingModel.Definition;
                        globalTileTransform = buildingModel.Transform;
                        overlappingPlacementData2.AddBuildingPlacement(new BuildingPlacement(new BuildingDescriptor(definition2, in globalTileTransform, buildingModel.Configuration, buildingModel.State), placementAllowability2, num));
                        num++;
                    }
                    this.BuildingPlacementIndicatorDrawer.SetNewPlacementData(overlappingPlacementData, default(PlacementInputHolder));
                    this.BuildingPlacementIndicatorDrawer.Draw(options);
                    SmartBuildingBlueprintRenderer.Draw(options, scopedList, null);
                }
            }
        }
        protected override void Draw_AreaSelection(FrameDrawOptions options, GlobalTileCoordinate from, GlobalTileCoordinate to, HUDMassSelectionSelectionType type)
        {
            HUDMassSelectionDrawUtils.Draw_BuildingAreaSelection(options, from, to, type, false);
        }
        protected override void Draw_ExistingSelection(FrameDrawOptions options, IReadOnlyCollection<BuildingModel> selection)
        {
            if (selection.Count == 0)
            {
                return;
            }
            using (ScopedList<SmartBuildingBlueprintRenderer.DrawData> scopedList = ScopedList<SmartBuildingBlueprintRenderer.DrawData>.Get())
            {
                using (OverlappingPlacementData overlappingPlacementData = new OverlappingPlacementData())
                {
                    PlacementAllowability placementAllowability = PlacementAllowability.ValidPlacement;
                    int num = 0;
                    foreach (BuildingModel buildingModel in selection)
                    {
                        List<SmartBuildingBlueprintRenderer.DrawData> list = scopedList;
                        IBuildingDefinition definition = buildingModel.Definition;
                        GlobalTileTransform globalTileTransform = buildingModel.Transform;
                        list.Add(new SmartBuildingBlueprintRenderer.DrawData(definition, in globalTileTransform, placementAllowability));
                        OverlappingPlacementData overlappingPlacementData2 = overlappingPlacementData;
                        IBuildingDefinition definition2 = buildingModel.Definition;
                        globalTileTransform = buildingModel.Transform;
                        overlappingPlacementData2.AddBuildingPlacement(new BuildingPlacement(new BuildingDescriptor(definition2, in globalTileTransform, buildingModel.Configuration, buildingModel.State), placementAllowability, num));
                        num++;
                    }
                    this.BuildingPlacementIndicatorDrawer.SetNewPlacementData(overlappingPlacementData, default(PlacementInputHolder));
                    this.BuildingPlacementIndicatorDrawer.Draw(options);
                    SmartBuildingBlueprintRenderer.Draw(options, scopedList, null);
                }
            }
        }
        protected override void Draw_HoverState(FrameDrawOptions options, BuildingModel building, float alpha)
        {
            IMeshReference meshReference;
            if (!building.Definition.CustomData.Get<IBuildingDrawData>().CombinedBlueprintMesh.TryGet(options.LOD.BuildingLOD, out meshReference))
            {
                return;
            }
            WorldCoordinate worldCoordinate = building.Transform.Position.ToCenter_W(0f);
            options.Renderers.RegularNonInstanced.DrawMesh(meshReference, options.Theme.BaseResources.UXBuildingHoverIndicatorMaterial, FastMatrix.TranslateRotate(in worldCoordinate, building.Rotation_G), RenderCategory.AnalogUI, MaterialPropertyHelpers.CreateAlphaBlock(alpha), default(ShadowToken), default(ShadowToken));
        }
        private BuildingPlacementIndicatorDrawer BuildingPlacementIndicatorDrawer;
    }
}
