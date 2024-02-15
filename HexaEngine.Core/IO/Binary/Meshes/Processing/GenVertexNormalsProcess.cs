namespace HexaEngine.Core.IO.Binary.Meshes.Processing
{
    using HexaEngine.Core.IO.Binary.Meshes;
    using HexaEngine.Core.IO.Binary.Terrains;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;

    /// <summary>
    /// Provides methods for generating vertex normals for a mesh.
    /// </summary>
    public static class GenVertexNormalsProcess
    {
        /// <summary>
        /// Configuration parameter for the maximum angle in radians.
        /// </summary>
        public static float ConfigMaxAngle = 90f.ToRad();

        /// <summary>
        /// Generates vertex normals for the given mesh data.
        /// </summary>
        /// <param name="pMesh">The mesh data to process.</param>
        /// <returns>True if the process is successful, otherwise false.</returns>
        public static unsafe bool GenMeshVertexNormals(MeshData pMesh)
        {
            // Allocate the array to hold the output normals
            pMesh.Normals = new Vector3[pMesh.VerticesCount];

            // Compute per-face normals but store them per-vertex
            for (uint a = 0; a < pMesh.IndicesCount / 3; a++)
            {
                Face face = pMesh.GetFaceAtIndex(a);

                Vector3 pV1 = pMesh.Positions[face[0]];
                Vector3 pV2 = pMesh.Positions[face[1]];
                Vector3 pV3 = pMesh.Positions[face[2]];

                Vector3 vNor = Vector3.Normalize(Vector3.Cross(pV2 - pV1, pV3 - pV1));

                for (uint i = 0; i < 3; ++i)
                {
                    pMesh.Normals[face[i]] = vNor;
                }
            }

            // Set up a SpatialSort to quickly find all vertices close to a given position
            // check whether we can reuse the SpatialSort of a previous step.
            SpatialSort vertexFinder = new(pMesh.Positions, sizeof(Vector3));
            float posEpsilon = ProcessingHelper.ComputePositionEpsilon(pMesh);

            List<uint> verticesFound = new();
            Vector3[] pcNew = new Vector3[pMesh.VerticesCount];

            if (ConfigMaxAngle >= 175f.ToRad())
            {
                // There is no angle limit. Thus all vertices with positions close
                // to each other will receive the same vertex normal. This allows us
                // to optimize the whole algorithm a little bit ...
                bool[] abHad = new bool[pMesh.VerticesCount];
                for (uint i = 0; i < pMesh.VerticesCount; ++i)
                {
                    if (abHad[(int)i])
                    {
                        continue;
                    }

                    // Get all vertices that share this one ...
                    vertexFinder.FindPositions(pMesh.Positions[i], posEpsilon, verticesFound);

                    Vector3 pcNor = default;
                    for (uint a = 0; a < verticesFound.Count; ++a)
                    {
                        Vector3 v = pMesh.Normals[verticesFound[(int)a]];
                        if (!float.IsNaN(v.X)) pcNor += v;
                    }
                    pcNor = Vector3.Normalize(pcNor);

                    // Write the smoothed normal back to all affected normals
                    for (uint a = 0; a < verticesFound.Count; ++a)
                    {
                        uint vidx = verticesFound[(int)a];
                        pcNew[vidx] = pcNor;
                        abHad[(int)vidx] = true;
                    }
                }
            }
            // Slower code path if a smooth angle is set. There are many ways to achieve
            // the effect, this one is the most straightforward one.
            else
            {
                float fLimit = MathF.Cos(ConfigMaxAngle);
                for (uint i = 0; i < pMesh.VerticesCount; ++i)
                {
                    // Get all vertices that share this one ...
                    vertexFinder.FindPositions(pMesh.Positions[i], posEpsilon, verticesFound);

                    Vector3 vr = pMesh.Normals[i];

                    Vector3 pcNor = default;
                    for (uint a = 0; a < verticesFound.Count; ++a)
                    {
                        Vector3 v = pMesh.Normals[verticesFound[(int)a]];

                        // Check whether the angle between the two normals is not too large.
                        // Skip the angle check on our own normal to avoid false negatives
                        // (v*v is not guaranteed to be 1.0 for all unit vectors v)
                        if (!float.IsNaN(v.X) && (verticesFound[(int)a] == i || Vector3.Dot(v, vr) >= fLimit))
                        {
                            pcNor += v * MathF.Acos(Vector3.Dot(v, vr));
                        }
                    }
                    pcNew[i] = Vector3.Normalize(pcNor);
                }
            }

            pMesh.Normals = pcNew;

            return true;
        }

        /// <summary>
        /// Generates vertex normals for the given mesh data using an alternative approach.
        /// </summary>
        /// <param name="pMesh">The mesh data to process.</param>
        /// <returns>True if the process is successful, otherwise false.</returns>
        public static unsafe bool GenMeshVertexNormals2(MeshData pMesh)
        {
            Vector3* vertNormals = AllocT<Vector3>(pMesh.VerticesCount);
            Memset(vertNormals, 0, (int)pMesh.VerticesCount);
            uint nFaces = pMesh.IndicesCount / 3;

            for (int face = 0; face < nFaces; ++face)
            {
                uint i0 = pMesh.Indices[face * 3];
                uint i1 = pMesh.Indices[face * 3 + 1];
                uint i2 = pMesh.Indices[face * 3 + 2];

                Vector3 p0 = pMesh.Positions[i0];
                Vector3 p1 = pMesh.Positions[i1];
                Vector3 p2 = pMesh.Positions[i2];

                Vector3 u = p1 - p0;
                Vector3 v = p2 - p0;

                Vector3 faceNormal = Vector3.Normalize(Vector3.Cross(u, v));

                if (float.IsNaN(faceNormal.X))
                {
                    continue;
                }

                Vector3 a = Vector3.Normalize(u);
                Vector3 b = Vector3.Normalize(v);
                float w0 = Vector3.Dot(a, b);
                w0 = Math.Clamp(w0, -1, 1);
                w0 = MathF.Acos(w0);

                Vector3 c = Vector3.Normalize(p2 - p1);
                Vector3 d = Vector3.Normalize(p0 - p1);
                float w1 = Vector3.Dot(c, d);
                w1 = Math.Clamp(w1, -1, 1);
                w1 = MathF.Acos(w1);

                Vector3 e = Vector3.Normalize(p0 - p2);
                Vector3 f = Vector3.Normalize(p1 - p2);
                float w2 = Vector3.Dot(e, f);
                w2 = Math.Clamp(w2, -1, 1);
                w2 = MathF.Acos(w2);

                vertNormals[i0] = faceNormal * w0 + vertNormals[i0];
                vertNormals[i1] = faceNormal * w1 + vertNormals[i1];
                vertNormals[i2] = faceNormal * w2 + vertNormals[i2];
            }

            for (int i = 0; i < pMesh.VerticesCount; ++i)
            {
                pMesh.Normals[i] = Vector3.Normalize(vertNormals[i]);
            }

            Free(vertNormals);

            return true;
        }

        /// <summary>
        /// Generates vertex normals for the given terrain cell data using an alternative approach.
        /// </summary>
        /// <param name="pMesh">The terrain cell data to process.</param>
        /// <returns>True if the process is successful, otherwise false.</returns>
        public static unsafe bool GenMeshVertexNormals2(TerrainCellData pMesh)
        {
            Vector3* vertNormals = AllocT<Vector3>(pMesh.VerticesCount);
            Memset(vertNormals, 0, (int)pMesh.VerticesCount);
            uint nFaces = pMesh.IndicesCount / 3;

            for (int face = 0; face < nFaces; ++face)
            {
                uint i0 = pMesh.Indices[face * 3];
                uint i1 = pMesh.Indices[face * 3 + 1];
                uint i2 = pMesh.Indices[face * 3 + 2];

                Vector3 p0 = pMesh.Positions[i0];
                Vector3 p1 = pMesh.Positions[i1];
                Vector3 p2 = pMesh.Positions[i2];

                Vector3 u = p1 - p0;
                Vector3 v = p2 - p0;

                Vector3 faceNormal = Vector3.Normalize(Vector3.Cross(u, v));

                if (float.IsNaN(faceNormal.X))
                {
                    continue;
                }

                Vector3 a = Vector3.Normalize(u);
                Vector3 b = Vector3.Normalize(v);
                float w0 = Vector3.Dot(a, b);
                w0 = Math.Clamp(w0, -1, 1);
                w0 = MathF.Acos(w0);

                Vector3 c = Vector3.Normalize(p2 - p1);
                Vector3 d = Vector3.Normalize(p0 - p1);
                float w1 = Vector3.Dot(c, d);
                w1 = Math.Clamp(w1, -1, 1);
                w1 = MathF.Acos(w1);

                Vector3 e = Vector3.Normalize(p0 - p2);
                Vector3 f = Vector3.Normalize(p1 - p2);
                float w2 = Vector3.Dot(e, f);
                w2 = Math.Clamp(w2, -1, 1);
                w2 = MathF.Acos(w2);

                vertNormals[i0] = faceNormal * w0 + vertNormals[i0];
                vertNormals[i1] = faceNormal * w1 + vertNormals[i1];
                vertNormals[i2] = faceNormal * w2 + vertNormals[i2];
            }

            for (int i = 0; i < pMesh.VerticesCount; ++i)
            {
                pMesh.Normals[i] = Vector3.Normalize(vertNormals[i]);
            }

            Free(vertNormals);

            return true;
        }
    }
}