namespace HexaEngine.Core.IO.Binary.Meshes.Processing
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO.Binary.Meshes;
    using MeshDecimator;
    using MeshDecimator.Algorithms;
    using System.Numerics;

    /// <summary>
    /// Provides functionality for mesh simplification/optimization.
    /// </summary>
    public class SimplifyProcess
    {
        /// <summary>
        /// Simplifies the given mesh data.
        /// </summary>
        /// <param name="data">The mesh data to be simplified.</param>
        public static MeshLODData Simplify(MeshLODData data, int j)
        {
            Mesh mesh;
            mesh = new(data.Positions, data.Indices);
            mesh.Colors = data.Colors;
            mesh.Normals = data.Normals;
            mesh.Tangents = data.Tangents;
            mesh.SetUVs(0, data.UVs);
            mesh.BoneWeights = data.BoneWeights?.Select(x => new BoneWeight(x.BoneIds, x.Weights)).ToArray();

            Logger.Info($"Simplify Input: Vertices {data.VertexCount}, Indices {data.IndexCount}, Tris: {mesh.TriangleCount}");

            uint faceCount = data.IndexCount / 3;

            FastQuadricMeshSimplification algorithm = (FastQuadricMeshSimplification)MeshDecimation.CreateAlgorithm(Algorithm.FastQuadricMesh);

            algorithm.PreserveBorders = true;
            algorithm.PreserveSeams = true;

            int targetTri = (int)(mesh.TriangleCount * (1 / (float)(j + 2)));

            var outputMesh = MeshDecimation.DecimateMesh(algorithm, mesh, (int)(targetTri));

            MeshLODData outputData = new(0, (uint)outputMesh.VertexCount, (uint)outputMesh.Indices.Length, default, default, default, default, default, default, default, default, default);

            outputData.Indices = outputMesh.Indices.Select(x => (uint)x).ToArray();
            outputData.Colors = outputMesh.Colors;
            outputData.Positions = new Vector3[outputMesh.VertexCount];
            outputData.Normals = outputMesh.Normals;
            outputData.Tangents = outputMesh.Tangents;
            outputData.UVs = new Vector3[outputMesh.VertexCount];
            outputData.BoneWeights = outputMesh.BoneWeights?.Select(x => new IO.BoneWeight(x.BoneIndex0, x.BoneIndex1, x.BoneIndex2, x.BoneIndex3, x.BoneWeight0, x.BoneWeight1, x.BoneWeight2, x.BoneWeight3)).ToArray();

            for (int i = 0; i < outputMesh.VertexCount; i++)
            {
                outputData.Positions[i] = outputMesh.Vertices[i];
            }

            outputData.UVs = outputMesh.GetUVs3D(0);

            outputData.GenerateBounds();

            Logger.Info($"Simplify Done: Vertices {outputData.VertexCount}, Indices {outputData.IndexCount}, Tris: {outputMesh.TriangleCount}, Ratio: {outputMesh.TriangleCount / (float)mesh.TriangleCount}");

            return outputData;
        }
    }
}