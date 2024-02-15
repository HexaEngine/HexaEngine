namespace HexaEngine.Core.IO.Binary.Meshes.Processing
{
    using HexaEngine.Core.IO.Binary.Meshes;
    using System.Numerics;

    /// <summary>
    /// Helper class for various mesh processing operations.
    /// </summary>
    public class ProcessingHelper
    {
        /// <summary>
        /// Computes the minimum and maximum bounds of an array of vectors.
        /// </summary>
        /// <param name="vectors">The array of vectors.</param>
        /// <param name="min">The minimum vector (output).</param>
        /// <param name="max">The maximum vector (output).</param>
        public static void ArrayBounds(Vector3[] vectors, ref Vector3 min, ref Vector3 max)
        {
            for (int i = 0; i < vectors.Length; ++i)
            {
                min = Vector3.Min(vectors[i], min);
                max = Vector3.Max(vectors[i], max);
            }
        }

        /// <summary>
        /// Computes the position epsilon for the given mesh data.
        /// </summary>
        /// <param name="mesh">The mesh data.</param>
        /// <returns>The computed position epsilon.</returns>
        public static float ComputePositionEpsilon(MeshData mesh)
        {
            float epsilon = 1e-4f;

            // Calculate the position bounds so we have a reliable epsilon to check position differences against
            Vector3 minVec = default, maxVec = default;
            ArrayBounds(mesh.Positions, ref minVec, ref maxVec);
            return (maxVec - minVec).Length() * epsilon;
        }

        /// <summary>
        /// Computes the position epsilon for the given vector bounds.
        /// </summary>
        /// <param name="min">The minimum vector bounds.</param>
        /// <param name="max">The maximum vector bounds.</param>
        /// <returns>The computed position epsilon.</returns>
        public static float ComputePositionEpsilon(Vector3 min, Vector3 max)
        {
            float epsilon = 1e-4f;

            return (max - min).Length() * epsilon;
        }
    }
}