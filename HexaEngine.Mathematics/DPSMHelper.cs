namespace HexaEngine.Mathematics
{
    using System.Numerics;

    /// <summary>
    /// Helper methods for working with rendering in the OSM system in dual paraboloid mode.
    /// </summary>
    public static class DPSMHelper
    {
        /// <summary>
        /// Point light Z-Near.
        /// </summary>
        public const float ZNear = 0.001f;

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
            matrices[0] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos + Vector3.UnitX, Vector3.UnitY));  // X+ 0
            matrices[1] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos - Vector3.UnitX, Vector3.UnitY));  // X- 1
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

            switch (pass)
            {
                case 0:
                    matrix[0] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos + Vector3.UnitX, Vector3.UnitY));
                    break;

                case 1:
                    matrix[0] = Matrix4x4.Transpose(MathUtil.LookAtLH(pos, pos - Vector3.UnitX, Vector3.UnitY));
                    break;
            }

            box = new(new Vector3(pos.X - far, pos.Y - far, pos.Z - far), new Vector3(pos.X + far, pos.Y + far, pos.Z + far));
        }
    }
}