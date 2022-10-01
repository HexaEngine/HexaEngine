﻿namespace HexaEngine.Mathematics
{
    using System.Numerics;

    public static class PSMHelper
    {
        public static Matrix4x4 GetProjectionMatrix(float fov)
        {
            return MathUtil.PerspectiveFovLH(fov, 1, 0.01f, 100);
        }

        public static Matrix4x4[] GetLightSpaceMatrices(Transform light, float fov)
        {
            Vector3 pos = light.GlobalPosition;
            Matrix4x4 proj = GetProjectionMatrix(fov);
            Matrix4x4[] matrices = new Matrix4x4[1];
            matrices[0] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos + light.Forward, light.Up) * proj);
            return matrices;
        }
    }
}