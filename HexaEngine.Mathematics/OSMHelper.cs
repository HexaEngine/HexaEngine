namespace HexaEngine.Mathematics
{
    using System.Numerics;

    public static class OSMHelper
    {
        public static Matrix4x4 GetProjectionMatrix(float far)
        {
            return MathUtil.PerspectiveFovLH(90f.ToRad(), 1, 0.001f, far);
        }

        public static unsafe void GetLightSpaceMatrices(Transform light, float far, Matrix4x4* matrices, ref BoundingBox box)
        {
            Vector3 pos = light.GlobalPosition;
            Matrix4x4 proj = GetProjectionMatrix(far);
            matrices[0] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos + Vector3.UnitX, Vector3.UnitY) * proj);
            matrices[1] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos - Vector3.UnitX, Vector3.UnitY) * proj);
            matrices[2] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos + Vector3.UnitY, -Vector3.UnitZ) * proj);
            matrices[3] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos - Vector3.UnitY, Vector3.UnitZ) * proj);
            matrices[4] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos + Vector3.UnitZ, Vector3.UnitY) * proj);
            matrices[5] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos - Vector3.UnitZ, Vector3.UnitY) * proj);
            box = new(new Vector3(pos.X - far, pos.Y - far, pos.Z - far), new Vector3(pos.X + far, pos.Y + far, pos.Z + far));
        }
    }
}