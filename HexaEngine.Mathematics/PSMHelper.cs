namespace HexaEngine.Mathematics
{
    using System.Numerics;

    public static class PSMHelper
    {
        public static Matrix4x4 GetProjectionMatrix(float fov)
        {
            return MathUtil.PerspectiveFovLH(fov, 1, 1, 25);
        }

        public static Matrix4x4[] GetLightSpaceMatrices(Transform light, float fov)
        {
            Vector3 pos = light.Position;
            Matrix4x4 proj = GetProjectionMatrix(fov);
            Matrix4x4[] matrices = new Matrix4x4[1];
            matrices[0] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos + light.Forward, light.Up) * proj);
            return matrices;
        }
    }
}