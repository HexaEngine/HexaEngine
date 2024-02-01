namespace HexaEngine.Mathematics
{
    using System.Numerics;

    /// <summary>
    /// Helper methods for working with rendering in the OSM system.
    /// </summary>
    public static class OSMHelper
    {
        /// <summary>
        /// Point light Z-Near.
        /// </summary>
        public const float ZNear = 0.001f;

        /// <summary>
        /// Gets the projection matrix for rendering with the specified far clipping distance.
        /// </summary>
        /// <param name="far">The far clipping distance.</param>
        /// <returns>The projection matrix.</returns>
        public static Matrix4x4 GetProjectionMatrix(float far)
        {
            return MathUtil.PerspectiveFovLH(90f.ToRad(), 1, ZNear, far);
        }

        /// <summary>
        /// Gets the light space matrices and bounding box for shadow mapping.
        /// </summary>
        /// <param name="light">The light source transform.</param>
        /// <param name="far">The far clipping distance for the shadow map.</param>
        /// <param name="matrices">An array to store the light space matrices.</param>
        /// <param name="box">The bounding box for the shadow map.</param>
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

        /// <summary>
        /// Gets a single light space matrix for a specific rendering pass and updates the bounding box.
        /// </summary>
        /// <param name="light">The light source transform.</param>
        /// <param name="far">The far clipping distance for the shadow map.</param>
        /// <param name="matrix">The light space matrix to update.</param>
        /// <param name="pass">The rendering pass.</param>
        /// <param name="box">The bounding box for the shadow map.</param>
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