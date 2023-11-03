namespace HexaEngine.Mathematics
{
    using System.Numerics;

    /// <summary>
    /// A helper class for working with bounding boxes.
    /// </summary>
    public static class BoundingBoxHelper
    {
        /// <summary>
        /// Computes a bounding box that encloses an array of 3D positions.
        /// </summary>
        /// <param name="positions">An array of 3D positions to compute the bounding box from.</param>
        /// <returns>The computed bounding box that encloses the specified positions.</returns>
        public static BoundingBox Compute(Vector3[] positions)
        {
            Vector3 min = default;
            Vector3 max = default;

            for (int i = 0; i < positions.Length; i++)
            {
                min = Vector3.Min(min, positions[i]);
                max = Vector3.Max(max, positions[i]);
            }

            return new BoundingBox(min, max);
        }

        /// <summary>
        /// Computes a bounding box from a pointer to an array of vertices.
        /// </summary>
        /// <param name="vertices">A pointer to the array of vertices.</param>
        /// <param name="verticesCount">The number of vertices in the array.</param>
        /// <param name="vertexStride">The stride (in bytes) between vertices in the array.</param>
        /// <returns>The computed bounding box that encloses the specified vertices.</returns>
        public static unsafe BoundingBox Compute(void* vertices, uint verticesCount, uint vertexStride)
        {
            Vector3 min = default;
            Vector3 max = default;

            for (int i = 0; i < verticesCount; i++)
            {
                var position = *(Vector3*)vertices;
                min = Vector3.Min(min, position);
                max = Vector3.Max(max, position);
                vertices = (byte*)vertices + vertexStride;
            }

            return new BoundingBox(min, max);
        }
    }
}