namespace HexaEngine.Mathematics
{
    using System.Numerics;

    public static class OSMHelper
    {
        public static Matrix4x4 GetProjectionMatrix(float far)
        {
            return MathUtil.PerspectiveFovLH(90f.ToRad(), 1, 0.001f, far);
        }

        public static unsafe void GetLightSpaceMatrices(Transform light, float far, Matrix4x4* matrices, BoundingFrustum[] frusta)
        {
            Vector3 pos = light.GlobalPosition;
            Matrix4x4 proj = GetProjectionMatrix(far);
            matrices[0] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos + Vector3.UnitX, Vector3.UnitY) * proj);
            matrices[1] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos - Vector3.UnitX, Vector3.UnitY) * proj);
            matrices[2] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos + Vector3.UnitY, -Vector3.UnitZ) * proj);
            matrices[3] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos - Vector3.UnitY, Vector3.UnitZ) * proj);
            matrices[4] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos + Vector3.UnitZ, Vector3.UnitY) * proj);
            matrices[5] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos - Vector3.UnitZ, Vector3.UnitY) * proj);
            frusta[0].Initialize(matrices[0]);
            frusta[1].Initialize(matrices[1]);
            frusta[2].Initialize(matrices[2]);
            frusta[3].Initialize(matrices[3]);
            frusta[4].Initialize(matrices[4]);
            frusta[5].Initialize(matrices[5]);
        }
    }
}