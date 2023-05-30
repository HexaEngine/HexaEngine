namespace HexaEngine.Core.IO.Meshes
{
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;

    public static class CalcTangentsProcess
    {
        public static float ConfigMaxAngle = 45f.ToRad();

        public static unsafe bool ProcessMesh(ref MeshData pMesh)
        {
            const float angleEpsilon = 0.9999f;
            UnsafeList<bool> vertexDone = new((int)pMesh.VerticesCount);

            pMesh.Tangents = new Vector3[pMesh.VerticesCount];
            pMesh.Bitangents = new Vector3[pMesh.VerticesCount];

            Vector3[] meshPos = pMesh.Positions;
            Vector3[] meshNorm = pMesh.Normals;
            Vector3[] meshTex = pMesh.UVs;
            Vector3[] meshTang = pMesh.Tangents;
            Vector3[] meshBitang = pMesh.Bitangents;

            // calculate the tangent and bitangent for every face
            for (uint a = 0; a < pMesh.IndicesCount / 3; a++)
            {
                Face face = pMesh.GetFaceAtIndex(a);

                // triangle or polygon... we always use only the first three indices. A polygon
                // is supposed to be planar anyways....
                // FIXME: (thom) create correct calculation for multi-vertex polygons maybe?
                uint p0 = face.Index1, p1 = face.Index2, p2 = face.Index3;

                // position differences p1->p2 and p1->p3
                Vector3 v = meshPos[p1] - meshPos[p0], w = meshPos[p2] - meshPos[p0];

                // texture offset p1->p2 and p1->p3
                float sx = meshTex[p1].X - meshTex[p0].X, sy = meshTex[p1].Y - meshTex[p0].Y;
                float tx = meshTex[p2].X - meshTex[p0].X, ty = meshTex[p2].Y - meshTex[p0].Y;
                float dirCorrection = (tx * sy - ty * sx) < 0.0f ? -1.0f : 1.0f;
                // when t1, t2, t3 in same position in UV space, just use default UV direction.
                if (sx * ty == sy * tx)
                {
                    sx = 0.0f;
                    sy = 1.0f;
                    tx = 1.0f;
                    ty = 0.0f;
                }

                // tangent points in the direction where to positive X axis of the texture coord's would point in model space
                // bitangent's points along the positive Y axis of the texture coord's, respectively
                Vector3 tangent, bitangent;
                tangent.X = (w.X * sy - v.X * ty) * dirCorrection;
                tangent.Y = (w.Y * sy - v.Y * ty) * dirCorrection;
                tangent.Z = (w.Z * sy - v.Z * ty) * dirCorrection;
                bitangent.X = (-w.X * sx + v.X * tx) * dirCorrection;
                bitangent.Y = (-w.Y * sx + v.Y * tx) * dirCorrection;
                bitangent.Z = (-w.Z * sx + v.Z * tx) * dirCorrection;

                // store for every vertex of that face
                for (uint b = 0; b < 3; ++b)
                {
                    uint p = face[b];

                    // project tangent and bitangent into the plane formed by the vertex' normal
                    Vector3 localTangent = tangent - meshNorm[p] * (tangent * meshNorm[p]);
                    Vector3 localBitangent = bitangent - meshNorm[p] * (bitangent * meshNorm[p]) - localTangent * (bitangent * localTangent);
                    localTangent = Vector3.Normalize(localTangent); // localTangent.NormalizeSafe();
                    localBitangent = Vector3.Normalize(localBitangent); // localBitangent.NormalizeSafe();

                    // reconstruct tangent/bitangent according to normal and bitangent/tangent when it's infinite or NaN.

                    bool invalid_tangent = IsSpecialFloat(localTangent.X) || IsSpecialFloat(localTangent.Y) || IsSpecialFloat(localTangent.Z);
                    bool invalid_bitangent = IsSpecialFloat(localBitangent.X) || IsSpecialFloat(localBitangent.Y) || IsSpecialFloat(localBitangent.Z);
                    if (invalid_tangent != invalid_bitangent)
                    {
                        if (invalid_tangent)
                        {
                            localTangent = Vector3.Cross(meshNorm[p], localBitangent);
                            localTangent = Vector3.Normalize(localTangent);
                        }
                        else
                        {
                            localBitangent = Vector3.Cross(localTangent, meshNorm[p]);
                            localBitangent = Vector3.Normalize(localBitangent);
                        }
                    }

                    // and write it into the mesh.
                    meshTang[p] = localTangent;
                    meshBitang[p] = localBitangent;
                }
            }

            // create a helper to quickly find locally close vertices among the vertex array
            // FIX: check whether we can reuse the SpatialSort of a previous step
            SpatialSort vertexFinder = new(pMesh.Positions, sizeof(Vector3));
            float posEpsilon = ProcessingHelper.ComputePositionEpsilon(pMesh);

            List<uint> verticesFound = new();

            float fLimit = MathF.Cos(ConfigMaxAngle);
            List<uint> closeVertices = new();

            // in the second pass we now smooth out all tangents and bitangents at the same local position
            // if they are not too far off.
            for (uint a = 0; a < pMesh.VerticesCount; a++)
            {
                if (vertexDone[(int)a])
                    continue;

                Vector3 origPos = pMesh.Positions[a];
                Vector3 origNorm = pMesh.Normals[a];
                Vector3 origTang = pMesh.Tangents[a];
                Vector3 origBitang = pMesh.Bitangents[a];
                closeVertices.Clear();

                // find all vertices close to that position
                vertexFinder.FindPositions(origPos, posEpsilon, verticesFound);

                closeVertices.EnsureCapacity(verticesFound.Count + 5);
                closeVertices.Add(a);

                // look among them for other vertices sharing the same normal and a close-enough tangent/bitangent
                for (uint b = 0; b < verticesFound.Count; b++)
                {
                    uint idx = verticesFound[(int)b];
                    if (vertexDone[(int)idx])
                        continue;
                    if (Vector3.Dot(meshNorm[(int)idx], origNorm) < angleEpsilon)
                        continue;
                    if (Vector3.Dot(meshTang[(int)idx], origTang) < fLimit)
                        continue;
                    if (Vector3.Dot(meshBitang[(int)idx], origBitang) < fLimit)
                        continue;

                    // it's similar enough -> add it to the smoothing group
                    closeVertices.Add(idx);
                    vertexDone[(int)idx] = true;
                }

                // smooth the tangents and bitangents of all vertices that were found to be close enough
                Vector3 smoothTangent = default, smoothBitangent = default;

                for (uint b = 0; b < closeVertices.Count; ++b)

                {
                    smoothTangent += meshTang[closeVertices[(int)b]];
                    smoothBitangent += meshBitang[closeVertices[(int)b]];
                }

                smoothTangent = Vector3.Normalize(smoothTangent);
                smoothBitangent = Vector3.Normalize(smoothBitangent);

                // and write it back into all affected tangents
                for (uint b = 0; b < closeVertices.Count; ++b)
                {
                    meshTang[closeVertices[(int)b]] = smoothTangent;
                    meshBitang[closeVertices[(int)b]] = smoothBitangent;
                }
            }

            vertexDone.Free();

            return true;
        }

        public static unsafe bool ProcessMesh2(MeshData pMesh)
        {
            var nFace = pMesh.IndicesCount / 3;

            Vector3* vertTangents = Alloc<Vector3>(nFace);

            for (uint i = 0; i < nFace; i++)
            {
                var face = new Face(pMesh.Indices[i * 3], pMesh.Indices[i * 3 + 1], pMesh.Indices[i * 3 + 2]);
                var vtxP1 = pMesh.Positions[face.Index1];
                var vtxP2 = pMesh.Positions[face.Index2];
                var vtxP3 = pMesh.Positions[face.Index3];

                Vector3 v = vtxP2 - vtxP1;
                Vector3 w = vtxP3 - vtxP1;

                float sx = pMesh.UVs[face.Index2].X - pMesh.UVs[face.Index1].X, sy = pMesh.UVs[face.Index2].Y - pMesh.UVs[face.Index1].Y;
                float tx = pMesh.UVs[face.Index3].X - pMesh.UVs[face.Index1].X, ty = pMesh.UVs[face.Index3].Y - pMesh.UVs[face.Index1].Y;

                float dirCorrection = (tx * sy - ty * sx) < 0.0f ? -1.0f : 1.0f;

                if (sx * ty == sy * tx)
                {
                    sx = 0.0f;
                    sy = 1.0f;
                    tx = 1.0f;
                    ty = 0.0f;
                }

                Vector3 tangent, bitangent;
                tangent.X = (w.X * sy - v.X * ty) * dirCorrection;
                tangent.Y = (w.Y * sy - v.Y * ty) * dirCorrection;
                tangent.Z = (w.Z * sy - v.Z * ty) * dirCorrection;
                bitangent.X = (-w.X * sx + v.X * tx) * dirCorrection;
                bitangent.Y = (-w.Y * sx + v.Y * tx) * dirCorrection;
                bitangent.Z = (-w.Z * sx + v.Z * tx) * dirCorrection;
                vertTangents[i] = tangent;
            }

            Vector3 tangentSum = default;

            for (int i = 0; i < pMesh.VerticesCount; ++i)
            {
                // Check which triangles use this vertex
                for (int j = 0; j < nFace; ++j)
                {
                    if (pMesh.Indices[j * 3] == i || pMesh.Indices[j * 3 + 1] == i || pMesh.Indices[j * 3 + 2] == i)
                    {
                        tangentSum += vertTangents[j];
                    }
                }

                tangentSum = Vector3.Normalize(tangentSum);

                pMesh.Tangents[i] = tangentSum;
                pMesh.Bitangents[i] = Vector3.Cross(pMesh.Normals[i], tangentSum);

                tangentSum = default;
            }

            Free(vertTangents);

            return true;
        }

        private static bool IsSpecialFloat(float x)
        {
            return float.IsNaN(x) || float.IsInfinity(x);
        }
    }
}