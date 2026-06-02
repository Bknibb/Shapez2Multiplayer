using Unity.Mathematics;
using UnityEngine;

namespace Shapez2Multiplayer
{
    public static class ExtraRaycastHelpers
    {
        public static double2 WorldToScreenPointDouble(double3 worldPos, Camera camera)
        {
            double4 viewPos = math.mul(
                ToDouble4x4(camera.worldToCameraMatrix),
                new double4(worldPos, 1.0)
            );
            double4 clipPos = math.mul(
                ToDouble4x4(camera.projectionMatrix),
                viewPos
            );
            if (clipPos.w <= 0.0)
            {
                return new double2(-1.0, -1.0);
            }
            double3 ndc = clipPos.xyz / clipPos.w;
            double screenX = (ndc.x + 1.0) * 0.5 * Screen.width;
            double screenY = (ndc.y + 1.0) * 0.5 * Screen.height;
            return new double2(screenX, screenY);
        }
        public static double4x4 ToDouble4x4(Matrix4x4 m)
        {
            return new double4x4(
                m.m00, m.m01, m.m02, m.m03,
                m.m10, m.m11, m.m12, m.m13,
                m.m20, m.m21, m.m22, m.m23,
                m.m30, m.m31, m.m32, m.m33
            );
        }
    }
}
