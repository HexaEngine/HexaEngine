namespace HexaEngine.Core.IO.Meshes.Processing
{
    using System.Numerics;

    public class ProcessingHelper
    {
        public static void ArrayBounds(Vector3[] vectors, ref Vector3 min, ref Vector3 max)
        {
            for (int i = 0; i < vectors.Length; ++i)
            {
                min = Vector3.Min(vectors[i], min);
                max = Vector3.Max(vectors[i], max);
            }
        }

        public static float ComputePositionEpsilon(MeshData mesh)
        {
            float epsilon = 1e-4f;

            // calculate the position bounds so we have a reliable epsilon to check position differences against
            Vector3 minVec = default, maxVec = default;
            ArrayBounds(mesh.Positions, ref minVec, ref maxVec);
            return (maxVec - minVec).Length() * epsilon;
        }

        public static float ComputePositionEpsilon(Vector3 min, Vector3 max)
        {
            float epsilon = 1e-4f;

            return (max - min).Length() * epsilon;
        }
    }
}