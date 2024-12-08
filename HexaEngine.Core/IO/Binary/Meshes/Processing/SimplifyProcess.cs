namespace HexaEngine.Core.IO.Binary.Meshes.Processing
{
    using Hexa.NET.Logging;
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
        /// <param name="j"></param>
        /// <param name="logger"></param>
        public static MeshLODData Simplify(MeshLODData data, int j, ILogger logger)
        {
            Mesh mesh;
            mesh = new(data.Positions, data.Indices);
            mesh.Colors = data.Colors;
            mesh.Normals = data.Normals;
            mesh.Tangents = data.Tangents;

            for (int i = 0; i < UVChannelInfo.MaxChannels; i++)
            {
                var channel = data.UVChannels[i];
                switch (channel.Type)
                {
                    case UVType.UV2D:
                        mesh.SetUVs(i, channel.GetUV2D());
                        break;

                    case UVType.UV3D:
                        mesh.SetUVs(i, channel.GetUV3D());
                        break;

                    case UVType.UV4D:
                        mesh.SetUVs(i, channel.GetUV4D());
                        break;
                }
            }

            mesh.BoneWeights = data.BoneWeights?.Select(x => new BoneWeight(x.BoneIds, x.Weights)).ToArray();

            logger.Info($"Simplify Input: Vertices {data.VertexCount}, Indices {data.IndexCount}, Tris: {mesh.TriangleCount}");

            uint faceCount = data.IndexCount / 3;

            FastQuadricMeshSimplification algorithm = (FastQuadricMeshSimplification)MeshDecimation.CreateAlgorithm(Algorithm.FastQuadricMesh);

            algorithm.PreserveBorders = true;
            algorithm.PreserveSeams = true;

            uint targetTri = (uint)(mesh.TriangleCount * (1 / (float)(j + 2)));

            var outputMesh = MeshDecimation.DecimateMesh(algorithm, mesh, targetTri);

            MeshLODData outputData = new(0, (uint)outputMesh.VertexCount, (uint)outputMesh.Indices.Length, default, default, default!, default!, default!, default!, default!, default!, default);

            outputData.Indices = outputMesh.Indices.Select(x => x).ToArray();
            outputData.Colors = outputMesh.Colors!;
            outputData.Positions = new Vector3[outputMesh.VertexCount];
            outputData.Normals = outputMesh.Normals!;
            outputData.Tangents = outputMesh.Tangents!;
            outputData.UVChannels = new UVChannel[UVChannelInfo.MaxChannels];
            outputData.BoneWeights = outputMesh.BoneWeights?.Select(x => new IO.BoneWeight(x.BoneIndex0, x.BoneIndex1, x.BoneIndex2, x.BoneIndex3, x.BoneWeight0, x.BoneWeight1, x.BoneWeight2, x.BoneWeight3)).ToArray();

            for (int i = 0; i < outputMesh.VertexCount; i++)
            {
                outputData.Positions[i] = outputMesh.Vertices[i];
            }

            for (int i = 0; i < UVChannelInfo.MaxChannels; i++)
            {
                var channel = outputData.UVChannels[i];
                switch (channel.Type)
                {
                    case UVType.UV2D:
                        channel.SetUVs(outputMesh.GetUVs2D(0)!);
                        break;

                    case UVType.UV3D:
                        channel.SetUVs(outputMesh.GetUVs3D(0)!);
                        break;

                    case UVType.UV4D:
                        channel.SetUVs(outputMesh.GetUVs4D(0)!);
                        break;
                }
                outputData.UVChannels[i] = channel;
            }

            outputData.GenerateBounds();

            logger.Info($"Simplify Done: Vertices {outputData.VertexCount}, Indices {outputData.IndexCount}, Tris: {outputMesh.TriangleCount}, Ratio: {outputMesh.TriangleCount / (float)mesh.TriangleCount}");

            return outputData;
        }
    }
}