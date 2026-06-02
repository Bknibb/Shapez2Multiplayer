using Core.Collections.Scoped;
using Game.Core.Coordinates;
using Game.HUD.CameraManager;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Mathematics;
using UnityEngine;

namespace Shapez2Multiplayer
{
    public static class ExtraScreenUtils
    {
        public static double2 WorldToScreenPointDouble(Viewport viewport, float3 worldCoordinate)
        {
            return ExtraRaycastHelpers.WorldToScreenPointDouble(worldCoordinate, viewport.MainCamera);
        }
        public static MethodInfo ScreenUtilsRaytraceTileCoordinatesInfo = AccessTools.Method(typeof(ScreenUtils), "RaytraceTileCoordinates");
        private static void RaytraceTileCoordinates(WorldCoordinate origin, WorldVector direction, float distance, ICollection<GlobalTileCoordinate> tiles)
        {
            ScreenUtilsRaytraceTileCoordinatesInfo.Invoke(null, new object[] { origin, direction, distance, tiles });
        }
        public static bool TryFindBuildingAtScreenPosition(Viewport viewport, float2 screenPosition, IMapModel map, out BuildingModel building, short? overrideIslandLayer = null, short? overrideBuildingLayer = null, bool? overrideShowAllBuildingLayers = null)
        {
            var islandLayer = overrideIslandLayer ?? viewport.IslandLayer;
            var buildingLayer = overrideBuildingLayer ?? viewport.BuildingLayer;
            short layer = (short)(islandLayer * 20 + buildingLayer);
            bool showAllBuildingLayers = overrideShowAllBuildingLayers ?? viewport.ShowAllBuildingLayers;
            short minLayer = (short)(islandLayer * 20);
            short maxLayer = (short)(islandLayer * 20 + map.MaxBuildingLayer);
            double3 planePoint2 = new double3(0.0, (double)minLayer - 0.01, 0.0);
            double3 planePoint = new double3(0.0, (double)maxLayer + 1.0 - 0.01, 0.0);
            Ray cursorRay = viewport.ScreenCoordinateToRay(screenPosition);
            ValueTuple<double3, double3> ray = RaycastHelpers.CustomScreenPointToRayDouble(math.clamp(screenPosition, new double2(0), new double2((double)Screen.width, (double)Screen.height)), viewport.MainCamera);
            double3 planeNormal = new double3(0.0, 1.0, 0.0);
            bool flag = RaycastHelpers.RaycastPlane(ray.Item1, ray.Item2, planeNormal, planePoint, out double3 intersection, out double rayDistance1);
            RaycastHelpers.RaycastPlane(ray.Item1, ray.Item2, planeNormal, planePoint2, out double3 intersection2, out double rayDistance2);
            intersection = (flag ? intersection : new double3(cursorRay.origin));
            using ScopedList<GlobalTileCoordinate> scopedList = ScopedList<GlobalTileCoordinate>.Get();
            ExtraScreenUtils.RaytraceTileCoordinates((float3)intersection, (float3)ray.Item2, (float)(rayDistance2 - rayDistance1), scopedList);
            Bounds bounds = default;
            for (int i = 0; i < scopedList.Count; i++)
            {
                GlobalTileCoordinate globalTileCoordinate = scopedList[i];
                if (globalTileCoordinate.z >= layer && (globalTileCoordinate.z <= layer || showAllBuildingLayers) && map.TryGetBuilding(globalTileCoordinate, out building))
                {
                    foreach (CollisionBox collisionBox in building.Definition.CustomData.Get<IBuildingDrawData>().Colliders)
                    {
                        LocalVector center_L = collisionBox.Center_L;
                        GlobalTileTransform transform = building.Transform;
                        bounds.center = center_L * transform;
                        bounds.size = collisionBox.DimensionsByRotation_W[building.Rotation_G];
                        if (bounds.IntersectRay(cursorRay))
                        {
                            return true;
                        }
                    }
                }
            }
            building = BuildingModel.Invalid;
            return false;
        }
    }
}
