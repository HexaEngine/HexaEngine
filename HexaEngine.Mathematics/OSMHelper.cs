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
            matrices[0] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos + Vector3.UnitX, Vector3.UnitY) * proj);  // X+ 0
            matrices[1] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos - Vector3.UnitX, Vector3.UnitY) * proj);  // X- 1
            matrices[2] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos + Vector3.UnitY, -Vector3.UnitZ) * proj); // Y+ 2
            matrices[3] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos - Vector3.UnitY, Vector3.UnitZ) * proj);  // Y- 3
            matrices[4] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos + Vector3.UnitZ, Vector3.UnitY) * proj);  // Z+ 4
            matrices[5] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos - Vector3.UnitZ, Vector3.UnitY) * proj);  // Z- 5
            box = new(new Vector3(pos.X - far, pos.Y - far, pos.Z - far), new Vector3(pos.X + far, pos.Y + far, pos.Z + far));
        }

        public static unsafe void GetLightSpaceMatrix(Transform light, float far, Matrix4x4* matrix, int pass, ref BoundingBox box)
        {
            Vector3 pos = light.GlobalPosition;
            Matrix4x4 proj = GetProjectionMatrix(far);

            switch (pass)
            {
                case 0:
                    matrix[0] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos + Vector3.UnitX, Vector3.UnitY) * proj);
                    break;

                case 1:
                    matrix[0] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos - Vector3.UnitX, Vector3.UnitY) * proj);
                    break;

                case 2:
                    matrix[0] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos + Vector3.UnitY, -Vector3.UnitZ) * proj);
                    break;

                case 3:
                    matrix[0] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos - Vector3.UnitY, Vector3.UnitZ) * proj);
                    break;

                case 4:
                    matrix[0] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos + Vector3.UnitZ, Vector3.UnitY) * proj);
                    break;

                case 5:
                    matrix[0] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos - Vector3.UnitZ, Vector3.UnitY) * proj);
                    break;
            }

            box = new(new Vector3(pos.X - far, pos.Y - far, pos.Z - far), new Vector3(pos.X + far, pos.Y + far, pos.Z + far));
        }
    }
}