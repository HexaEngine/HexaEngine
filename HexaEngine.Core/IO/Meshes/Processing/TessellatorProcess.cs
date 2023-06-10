namespace HexaEngine.Core.IO.Meshes.Processing
{
    using HexaEngine.Core.IO.Terrains;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public class TessellatorProcess
    {
        public static void Tessellate(MeshData data)
        {
            uint faces = data.IndicesCount / 3;

            uint newVertexCount = faces * 6;

            bool hasUVs = (data.Flags & VertexFlags.UVs) != 0;
            bool hasNormals = (data.Flags & VertexFlags.Normals) != 0;
            bool hasTangents = (data.Flags & VertexFlags.Tangents) != 0;
            bool hasBitangents = (data.Flags & VertexFlags.Bitangents) != 0;
            bool hasColors = (data.Flags & VertexFlags.Colors) != 0;

            Vector3[] positions = new Vector3[newVertexCount];
            Vector3[] uvs = hasUVs ? new Vector3[newVertexCount] : null;
            Vector3[] normals = hasNormals ? new Vector3[newVertexCount] : null;
            Vector3[] tangents = hasTangents ? new Vector3[newVertexCount] : null;
            Vector3[] bitangents = hasBitangents ? new Vector3[newVertexCount] : null;
            Vector4[] colors = hasColors ? new Vector4[newVertexCount] : null;

            uint newIndexCount = faces * 12;

            uint[] indices = new uint[newIndexCount];

            uint v = 0;
            uint k = 0;
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
                    Vector3 uv0 = uvs[v] = data.UVs[i0];
                    Vector3 uv1 = uvs[v + 1] = data.UVs[i1];
                    Vector3 uv2 = uvs[v + 2] = data.UVs[i2];
                    uvs[v + 3] = uvw0.X * uv0 + uvw0.Y * uv1 + uvw0.Z * uv2;
                    uvs[v + 4] = uvw1.X * uv0 + uvw1.Y * uv1 + uvw1.Z * uv2;
                    uvs[v + 5] = uvw2.X * uv0 + uvw2.Y * uv1 + uvw2.Z * uv2;
                }

                if (hasNormals)
                {
                    Vector3 normal0 = normals[v] = data.Normals[i0];
                    Vector3 normal1 = normals[v + 1] = data.Normals[i1];
                    Vector3 normal2 = normals[v + 2] = data.Normals[i2];
                    normals[v + 3] = uvw0.X * normal0 + uvw0.Y * normal1 + uvw0.Z * normal2;
                    normals[v + 4] = uvw1.X * normal0 + uvw1.Y * normal1 + uvw1.Z * normal2;
                    normals[v + 5] = uvw2.X * normal0 + uvw2.Y * normal1 + uvw2.Z * normal2;
                }

                if (hasTangents)
                {
                    Vector3 tangent0 = tangents[v] = data.Tangents[i0];
                    Vector3 tangent1 = tangents[v + 1] = data.Tangents[i1];
                    Vector3 tangent2 = tangents[v + 2] = data.Tangents[i2];
                    tangents[v + 3] = uvw0.X * tangent0 + uvw0.Y * tangent1 + uvw0.Z * tangent2;
                    tangents[v + 4] = uvw1.X * tangent0 + uvw1.Y * tangent1 + uvw1.Z * tangent2;
                    tangents[v + 5] = uvw2.X * tangent0 + uvw2.Y * tangent1 + uvw2.Z * tangent2;
                }

                if (hasBitangents)
                {
                    Vector3 bitangent0 = bitangents[v] = data.Bitangents[i0];
                    Vector3 bitangent1 = bitangents[v + 1] = data.Bitangents[i1];
                    Vector3 bitangent2 = bitangents[v + 2] = data.Bitangents[i2];
                    bitangents[v + 3] = uvw0.X * bitangent0 + uvw0.Y * bitangent1 + uvw0.Z * bitangent2;
                    bitangents[v + 4] = uvw1.X * bitangent0 + uvw1.Y * bitangent1 + uvw1.Z * bitangent2;
                    bitangents[v + 5] = uvw2.X * bitangent0 + uvw2.Y * bitangent1 + uvw2.Z * bitangent2;
                }

                if (hasColors)
                {
                    Vector4 bitangent0 = colors[v] = data.Colors[i0];
                    Vector4 bitangent1 = colors[v + 1] = data.Colors[i1];
                    Vector4 bitangent2 = colors[v + 2] = data.Colors[i2];
                    colors[v + 3] = uvw0.X * bitangent0 + uvw0.Y * bitangent1 + uvw0.Z * bitangent2;
                    colors[v + 4] = uvw1.X * bitangent0 + uvw1.Y * bitangent1 + uvw1.Z * bitangent2;
                    colors[v + 5] = uvw2.X * bitangent0 + uvw2.Y * bitangent1 + uvw2.Z * bitangent2;
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

            data.Positions = positions;
            data.UVs = uvs;
            data.Normals = normals;
            data.Tangents = tangents;
            data.Bitangents = bitangents;
            data.Colors = colors;

            data.Indices = indices;

            data.VerticesCount = newVertexCount;

            data.IndicesCount = newIndexCount;
        }

        public static void Tessellate(Terrain terrain)
        {
            uint faces = terrain.IndicesCount / 3;

            uint newVertexCount = faces * 6;

            bool hasUVs = (terrain.Flags & TerrainFlags.UVs) != 0;
            bool hasNormals = (terrain.Flags & TerrainFlags.Normals) != 0;
            bool hasTangents = (terrain.Flags & TerrainFlags.Tangents) != 0;
            bool hasBitangents = (terrain.Flags & TerrainFlags.Bitangents) != 0;

            Vector3[] positions = new Vector3[newVertexCount];
            Vector3[] uvs = hasUVs ? new Vector3[newVertexCount] : null;
            Vector3[] normals = hasNormals ? new Vector3[newVertexCount] : null;
            Vector3[] tangents = hasTangents ? new Vector3[newVertexCount] : null;
            Vector3[] bitangents = hasBitangents ? new Vector3[newVertexCount] : null;

            uint newIndexCount = faces * 12;

            uint[] indices = new uint[newIndexCount];

            uint v = 0;
            uint k = 0;
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

                if (hasUVs)
                {
                    Vector3 uv0 = uvs[v] = terrain.UVs[i0];
                    Vector3 uv1 = uvs[v + 1] = terrain.UVs[i1];
                    Vector3 uv2 = uvs[v + 2] = terrain.UVs[i2];
                    uvs[v + 3] = uvw0.X * uv0 + uvw0.Y * uv1 + uvw0.Z * uv2;
                    uvs[v + 4] = uvw1.X * uv0 + uvw1.Y * uv1 + uvw1.Z * uv2;
                    uvs[v + 5] = uvw2.X * uv0 + uvw2.Y * uv1 + uvw2.Z * uv2;
                }

                if (hasNormals)
                {
                    Vector3 normal0 = normals[v] = terrain.Normals[i0];
                    Vector3 normal1 = normals[v + 1] = terrain.Normals[i1];
                    Vector3 normal2 = normals[v + 2] = terrain.Normals[i2];
                    normals[v + 3] = uvw0.X * normal0 + uvw0.Y * normal1 + uvw0.Z * normal2;
                    normals[v + 4] = uvw1.X * normal0 + uvw1.Y * normal1 + uvw1.Z * normal2;
                    normals[v + 5] = uvw2.X * normal0 + uvw2.Y * normal1 + uvw2.Z * normal2;
                }

                if (hasTangents)
                {
                    Vector3 tangent0 = tangents[v] = terrain.Tangents[i0];
                    Vector3 tangent1 = tangents[v + 1] = terrain.Tangents[i1];
                    Vector3 tangent2 = tangents[v + 2] = terrain.Tangents[i2];
                    tangents[v + 3] = uvw0.X * tangent0 + uvw0.Y * tangent1 + uvw0.Z * tangent2;
                    tangents[v + 4] = uvw1.X * tangent0 + uvw1.Y * tangent1 + uvw1.Z * tangent2;
                    tangents[v + 5] = uvw2.X * tangent0 + uvw2.Y * tangent1 + uvw2.Z * tangent2;
                }

                if (hasBitangents)
                {
                    Vector3 bitangent0 = bitangents[v] = terrain.Bitangents[i0];
                    Vector3 bitangent1 = bitangents[v + 1] = terrain.Bitangents[i1];
                    Vector3 bitangent2 = bitangents[v + 2] = terrain.Bitangents[i2];
                    bitangents[v + 3] = uvw0.X * bitangent0 + uvw0.Y * bitangent1 + uvw0.Z * bitangent2;
                    bitangents[v + 4] = uvw1.X * bitangent0 + uvw1.Y * bitangent1 + uvw1.Z * bitangent2;
                    bitangents[v + 5] = uvw2.X * bitangent0 + uvw2.Y * bitangent1 + uvw2.Z * bitangent2;
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

            terrain.Positions = positions;
            terrain.UVs = uvs;
            terrain.Normals = normals;
            terrain.Tangents = tangents;
            terrain.Bitangents = bitangents;

            terrain.Indices = indices;

            terrain.VerticesCount = newVertexCount;

            terrain.IndicesCount = newIndexCount;
        }
    }
}