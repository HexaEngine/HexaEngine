namespace HexaEngine.Core.IO.Binary.Meshes.Processing
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.IO.Binary.Meshes;
    using HexaEngine.Core.IO.Binary.Terrains;
    using System.Numerics;

    /// <summary>
    /// Contains static methods for tessellating mesh or terrain data.
    /// </summary>
    public static class TessellatorProcess
    {
        /// <summary>
        /// Tessellates the given mesh data by subdividing each triangle into six smaller triangles.
        /// </summary>
        /// <param name="data">The mesh data to tessellate.</param>
        /// <param name="flags"></param>
        public static void Tessellate(MeshLODData data, VertexFlags flags)
        {
            // Calculate the number of faces and new vertex count
            uint faces = data.IndexCount / 3;
            uint newVertexCount = faces * 6;

            // Check which vertex attributes are present in the input mesh data
            bool hasUVs = (flags & VertexFlags.UVs) != 0;
            bool hasNormals = (flags & VertexFlags.Normals) != 0;
            bool hasTangents = (flags & VertexFlags.Tangents) != 0;
            bool hasColors = (flags & VertexFlags.Colors) != 0;

            // Initialize arrays for new vertex attributes based on the presence of these attributes
            Vector3[] positions = new Vector3[newVertexCount];
            UVChannel[] uvChannels = hasUVs ? new UVChannel[data.UVChannels.Length] : null!;
            Vector3[] normals = hasNormals ? new Vector3[newVertexCount] : null!;
            Vector3[] tangents = hasTangents ? new Vector3[newVertexCount] : null!;
            Vector4[] colors = hasColors ? new Vector4[newVertexCount] : null!;

            for (int i = 0; i < UVChannelInfo.MaxChannels; i++)
            {
                uvChannels[i] = new UVChannel(data.UVChannels[i].Type, newVertexCount);
            }

            // Initialize an array for new indices
            uint newIndexCount = faces * 12;
            uint[] indices = new uint[newIndexCount];

            uint v = 0;
            uint k = 0;

            // Tessellate each face
            for (uint i = 0; i < faces; i++)
            {
                uint i0 = data.Indices[i * 3];
                uint i1 = data.Indices[i * 3 + 1];
                uint i2 = data.Indices[i * 3 + 2];

                Vector3 pos0 = positions[v] = data.Positions[i0];
                Vector3 pos1 = positions[v + 1] = data.Positions[i1];
                Vector3 pos2 = positions[v + 2] = data.Positions[i2];

                Triangle.GetMiddlePoints(pos0, pos1, pos2, out Vector3 a, out Vector3 b, out Vector3 c);

                Vector3 uvw0 = Triangle.Barycentric(a, pos0, pos1, pos2);
                Vector3 uvw1 = Triangle.Barycentric(b, pos0, pos1, pos2);
                Vector3 uvw2 = Triangle.Barycentric(c, pos0, pos1, pos2);

                positions[v + 3] = uvw0.X * pos0 + uvw0.Y * pos1 + uvw0.Z * pos2;
                positions[v + 4] = uvw1.X * pos0 + uvw1.Y * pos1 + uvw1.Z * pos2;
                positions[v + 5] = uvw2.X * pos0 + uvw2.Y * pos1 + uvw2.Z * pos2;

                if (hasUVs)
                {
                    for (int j = 0; j < UVChannelInfo.MaxChannels; j++)
                    {
                        var oldChannel = data.UVChannels[j];

                        if (oldChannel.Type == UVType.Empty)
                        {
                            continue;
                        }

                        var newChannel = uvChannels[j];

                        switch (oldChannel.Type)
                        {
                            case UVType.UV2D:
                                InterpolateUVs(oldChannel.GetUV2D(), newChannel.GetUV2D(), v, i0, i1, i2, uvw0, uvw1, uvw2);
                                break;

                            case UVType.UV3D:
                                InterpolateUVs(oldChannel.GetUV3D(), newChannel.GetUV3D(), v, i0, i1, i2, uvw0, uvw1, uvw2);
                                break;

                            case UVType.UV4D:
                                InterpolateUVs(oldChannel.GetUV4D(), newChannel.GetUV4D(), v, i0, i1, i2, uvw0, uvw1, uvw2);
                                break;
                        }
                    }
                }

                if (hasNormals)
                {
                    Vector3 normal0 = normals![v] = data.Normals[i0];
                    Vector3 normal1 = normals[v + 1] = data.Normals[i1];
                    Vector3 normal2 = normals[v + 2] = data.Normals[i2];
                    normals[v + 3] = uvw0.X * normal0 + uvw0.Y * normal1 + uvw0.Z * normal2;
                    normals[v + 4] = uvw1.X * normal0 + uvw1.Y * normal1 + uvw1.Z * normal2;
                    normals[v + 5] = uvw2.X * normal0 + uvw2.Y * normal1 + uvw2.Z * normal2;
                }

                if (hasTangents)
                {
                    Vector3 tangent0 = tangents![v] = data.Tangents[i0];
                    Vector3 tangent1 = tangents[v + 1] = data.Tangents[i1];
                    Vector3 tangent2 = tangents[v + 2] = data.Tangents[i2];
                    tangents[v + 3] = uvw0.X * tangent0 + uvw0.Y * tangent1 + uvw0.Z * tangent2;
                    tangents[v + 4] = uvw1.X * tangent0 + uvw1.Y * tangent1 + uvw1.Z * tangent2;
                    tangents[v + 5] = uvw2.X * tangent0 + uvw2.Y * tangent1 + uvw2.Z * tangent2;
                }

                if (hasColors)
                {
                    Vector4 color0 = colors![v] = data.Colors[i0];
                    Vector4 color1 = colors[v + 1] = data.Colors[i1];
                    Vector4 color2 = colors[v + 2] = data.Colors[i2];
                    colors[v + 3] = uvw0.X * color0 + uvw0.Y * color1 + uvw0.Z * color2;
                    colors[v + 4] = uvw1.X * color0 + uvw1.Y * color1 + uvw1.Z * color2;
                    colors[v + 5] = uvw2.X * color0 + uvw2.Y * color1 + uvw2.Z * color2;
                }

                indices[k++] = v;
                indices[k++] = v + 3;
                indices[k++] = v + 5;
                indices[k++] = v + 3;
                indices[k++] = v + 1;
                indices[k++] = v + 4;
                indices[k++] = v + 5;
                indices[k++] = v + 3;
                indices[k++] = v + 4;
                indices[k++] = v + 5;
                indices[k++] = v + 4;
                indices[k++] = v + 2;
                v += 6;
            }

            // Update the MeshData object with the tessellated data

            data.Positions = positions;
            data.UVChannels = uvChannels!;
            data.Normals = normals!;
            data.Tangents = tangents!;
            data.Colors = colors!;

            data.Indices = indices;

            data.VertexCount = newVertexCount;

            data.IndexCount = newIndexCount;
        }

        private static void InterpolateUVs<T>(T[] oldUVs, T[] newUVs, uint baseIndex, uint i0, uint i1, uint i2, Vector3 uvw0, Vector3 uvw1, Vector3 uvw2)
        {
            switch (oldUVs)
            {
                case Vector2[] v2Old when newUVs is Vector2[] v2New:
                    {
                        Vector2 uv0 = v2New[baseIndex] = v2Old[i0];
                        Vector2 uv1 = v2New[baseIndex + 1] = v2Old[i1];
                        Vector2 uv2 = v2New[baseIndex + 2] = v2Old[i2];
                        v2New[baseIndex + 3] = uvw0.X * uv0 + uvw0.Y * uv1 + uvw0.Z * uv2;
                        v2New[baseIndex + 4] = uvw1.X * uv0 + uvw1.Y * uv1 + uvw1.Z * uv2;
                        v2New[baseIndex + 5] = uvw2.X * uv0 + uvw2.Y * uv1 + uvw2.Z * uv2;
                        break;
                    }

                case Vector3[] v3Old when newUVs is Vector3[] v3New:
                    {
                        Vector3 uv0 = v3New[baseIndex] = v3Old[i0];
                        Vector3 uv1 = v3New[baseIndex + 1] = v3Old[i1];
                        Vector3 uv2 = v3New[baseIndex + 2] = v3Old[i2];
                        v3New[baseIndex + 3] = uvw0.X * uv0 + uvw0.Y * uv1 + uvw0.Z * uv2;
                        v3New[baseIndex + 4] = uvw1.X * uv0 + uvw1.Y * uv1 + uvw1.Z * uv2;
                        v3New[baseIndex + 5] = uvw2.X * uv0 + uvw2.Y * uv1 + uvw2.Z * uv2;
                        break;
                    }

                case Vector4[] v4Old when newUVs is Vector4[] v4New:
                    {
                        Vector4 uv0 = v4New[baseIndex] = v4Old[i0];
                        Vector4 uv1 = v4New[baseIndex + 1] = v4Old[i1];
                        Vector4 uv2 = v4New[baseIndex + 2] = v4Old[i2];
                        v4New[baseIndex + 3] = uvw0.X * uv0 + uvw0.Y * uv1 + uvw0.Z * uv2;
                        v4New[baseIndex + 4] = uvw1.X * uv0 + uvw1.Y * uv1 + uvw1.Z * uv2;
                        v4New[baseIndex + 5] = uvw2.X * uv0 + uvw2.Y * uv1 + uvw2.Z * uv2;
                        break;
                    }
                default:
                    throw new ArgumentException($"Unsupported UV type: {typeof(T).Name}. Expected Vector2, Vector3, or Vector4.");
            }
        }

        private static void InterpolateBoneWeights(BoneWeight[] oldWeights, BoneWeight[] newWeights, uint baseIndex, uint i0, uint i1, uint i2, Vector3 uvw0, Vector3 uvw1, Vector3 uvw2)
        {
            BoneWeight weight0 = newWeights[baseIndex] = oldWeights[i0];
            BoneWeight weight1 = newWeights[baseIndex + 1] = oldWeights[i1];
            BoneWeight weight2 = newWeights[baseIndex + 2] = oldWeights[i2];

            newWeights[baseIndex + 3] = InterpolateWeight(weight0, weight1, weight2, uvw0);
            newWeights[baseIndex + 4] = InterpolateWeight(weight0, weight1, weight2, uvw1);
            newWeights[baseIndex + 5] = InterpolateWeight(weight0, weight1, weight2, uvw2);
        }

        private static unsafe BoneWeight InterpolateWeight(BoneWeight boneWeight0, BoneWeight boneWeight1, BoneWeight boneWeight2, Vector3 barycentric)
        {
            int* boneIds = stackalloc int[4 * 3]; // 3 weights each can have 4 bone ids.
            float* boneWeights = stackalloc float[4 * 3]; // each bone id can have 3 total weights.
            for (int i = 0; i < 4; i++)
            {
                boneIds[i] = boneWeight0.BoneIds[i];
                boneWeights[i] = boneWeight0.Weights[i] * barycentric.X;

                boneIds[i + 4] = boneWeight1.BoneIds[i];
                boneWeights[i + 4] = boneWeight1.Weights[i] * barycentric.Y;

                boneIds[i + 8] = boneWeight2.BoneIds[i];
                boneWeights[i + 8] = boneWeight2.Weights[i] * barycentric.Z;
            }

            int* uniqueBoneIds = stackalloc int[4 * 3]; // 3 weights each can have 4 bone ids.
            float* uniqueBoneWeights = stackalloc float[4 * 3]; // each bone id can have 3 total weights.
            int uniqueBoneCount = 0;

            for (int i = 0; i < 12; i++)
            {
                if (boneIds[i] == -1) continue; // Skip disabled bones

                bool found = false;

                for (int j = 0; j < uniqueBoneCount; j++)
                {
                    if (uniqueBoneIds[j] == boneIds[i])
                    {
                        uniqueBoneWeights[j] += boneWeights[i];
                        found = true;
                        break;
                    }
                }

                // If not found, insert bone ID and weight in sorted order
                if (!found)
                {
                    uniqueBoneIds[uniqueBoneCount] = boneIds[i];
                    uniqueBoneWeights[uniqueBoneCount] = boneWeights[i];
                    uniqueBoneCount++;

                    // Insertion-sort based approach to maintain descending order of weights
                    for (int j = uniqueBoneCount - 1; j > 0 && uniqueBoneWeights[j] > uniqueBoneWeights[j - 1]; j--)
                    {
                        // Swap bone IDs and weights to keep in descending order
                        (uniqueBoneIds[j], uniqueBoneIds[j - 1]) = (uniqueBoneIds[j - 1], uniqueBoneIds[j]);
                        (uniqueBoneWeights[j], uniqueBoneWeights[j - 1]) = (uniqueBoneWeights[j - 1], uniqueBoneWeights[j]);
                    }
                }
            }

            Point4 finalBoneIds = default;
            Vector4 finalWeights = default;
            float totalWeight = 0;
            for (int i = 0; i < 4; i++)
            {
                if (i < uniqueBoneCount)
                {
                    finalBoneIds[i] = uniqueBoneIds[i];
                    finalWeights[i] = uniqueBoneWeights[i];
                    totalWeight += uniqueBoneWeights[i];
                }
                else
                {
                    finalBoneIds[i] = -1;   // Zero out if there are fewer than 4 bones
                    finalWeights[i] = 0.0f;
                }
            }

            // Normalize the final weights to ensure they sum to 1
            finalWeights /= totalWeight;

            return new(finalBoneIds, finalWeights);
        }

        /// <summary>
        /// Tessellates the given terrain cell data by subdividing each triangle into six smaller triangles.
        /// </summary>
        /// <param name="terrain">The terrain cell data to tessellate.</param>
        public static void Tessellate(TerrainCellLODData terrain)
        {
#nullable disable
            // Calculate the number of faces and new vertex count
            uint faces = terrain.IndexCount / 3;
            uint newVertexCount = faces * 6;

            // Initialize arrays for new vertex attributes based on the presence of these attributes
            Vector3[] positions = new Vector3[newVertexCount];
            Vector2[] uvs = new Vector2[newVertexCount];
            Vector3[] normals = new Vector3[newVertexCount];
            Vector3[] tangents = new Vector3[newVertexCount];

            // Initialize an array for new indices
            uint newIndexCount = faces * 12;
            uint[] indices = new uint[newIndexCount];

            uint v = 0;
            uint k = 0;

            // Tessellate each face
            for (uint i = 0; i < faces; i++)
            {
                uint i0 = terrain.Indices[i * 3];
                uint i1 = terrain.Indices[i * 3 + 1];
                uint i2 = terrain.Indices[i * 3 + 2];

                Vector3 pos0 = positions[v] = terrain.Positions[i0];
                Vector3 pos1 = positions[v + 1] = terrain.Positions[i1];
                Vector3 pos2 = positions[v + 2] = terrain.Positions[i2];

                Triangle.GetMiddlePoints(pos0, pos1, pos2, out Vector3 a, out Vector3 b, out Vector3 c);

                Vector3 uvw0 = Triangle.Barycentric(a, pos0, pos1, pos2);
                Vector3 uvw1 = Triangle.Barycentric(b, pos0, pos1, pos2);
                Vector3 uvw2 = Triangle.Barycentric(c, pos0, pos1, pos2);

                positions[v + 3] = uvw0.X * pos0 + uvw0.Y * pos1 + uvw0.Z * pos2;
                positions[v + 4] = uvw1.X * pos0 + uvw1.Y * pos1 + uvw1.Z * pos2;
                positions[v + 5] = uvw2.X * pos0 + uvw2.Y * pos1 + uvw2.Z * pos2;

                {
                    Vector2 uv0 = uvs[v] = terrain.UVs[i0];
                    Vector2 uv1 = uvs[v + 1] = terrain.UVs[i1];
                    Vector2 uv2 = uvs[v + 2] = terrain.UVs[i2];
                    uvs[v + 3] = uvw0.X * uv0 + uvw0.Y * uv1 + uvw0.Z * uv2;
                    uvs[v + 4] = uvw1.X * uv0 + uvw1.Y * uv1 + uvw1.Z * uv2;
                    uvs[v + 5] = uvw2.X * uv0 + uvw2.Y * uv1 + uvw2.Z * uv2;
                }

                {
                    Vector3 normal0 = normals[v] = terrain.Normals[i0];
                    Vector3 normal1 = normals[v + 1] = terrain.Normals[i1];
                    Vector3 normal2 = normals[v + 2] = terrain.Normals[i2];
                    normals[v + 3] = uvw0.X * normal0 + uvw0.Y * normal1 + uvw0.Z * normal2;
                    normals[v + 4] = uvw1.X * normal0 + uvw1.Y * normal1 + uvw1.Z * normal2;
                    normals[v + 5] = uvw2.X * normal0 + uvw2.Y * normal1 + uvw2.Z * normal2;
                }

                {
                    Vector3 tangent0 = tangents[v] = terrain.Tangents[i0];
                    Vector3 tangent1 = tangents[v + 1] = terrain.Tangents[i1];
                    Vector3 tangent2 = tangents[v + 2] = terrain.Tangents[i2];
                    tangents[v + 3] = uvw0.X * tangent0 + uvw0.Y * tangent1 + uvw0.Z * tangent2;
                    tangents[v + 4] = uvw1.X * tangent0 + uvw1.Y * tangent1 + uvw1.Z * tangent2;
                    tangents[v + 5] = uvw2.X * tangent0 + uvw2.Y * tangent1 + uvw2.Z * tangent2;
                }

                indices[k++] = v;
                indices[k++] = v + 3;
                indices[k++] = v + 5;
                indices[k++] = v + 3;
                indices[k++] = v + 1;
                indices[k++] = v + 4;
                indices[k++] = v + 5;
                indices[k++] = v + 3;
                indices[k++] = v + 4;
                indices[k++] = v + 5;
                indices[k++] = v + 4;
                indices[k++] = v + 2;
                v += 6;
            }

            // Update the TerrainCellData object with the tessellated data

            terrain.Positions = positions;
            terrain.UVs = uvs;
            terrain.Normals = normals;
            terrain.Tangents = tangents;

            terrain.Indices = indices;

            terrain.VertexCount = newVertexCount;

            terrain.IndexCount = newIndexCount;
#nullable restore
        }
    }
}