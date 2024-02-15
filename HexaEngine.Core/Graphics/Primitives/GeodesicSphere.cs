namespace HexaEngine.Core.Graphics.Primitives
{
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Numerics;

    /// <summary>
    /// Represents a geodesic sphere primitive in 3D space.
    /// </summary>
    public sealed class GeodesicSphere : Primitive<MeshVertex, uint>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeodesicSphere"/> class.
        /// </summary>
        /// <param name="device">The graphics device used for mesh creation.</param>
        public GeodesicSphere(IGraphicsDevice device) : base(device)
        {
        }

        /// <summary>
        /// Initializes the geodesic sphere mesh with vertices and indices.
        /// </summary>
        /// <param name="device">The graphics device used for mesh creation.</param>
        /// <returns>
        /// A tuple containing the vertex buffer and optional index buffer of the geodesic sphere mesh.
        /// </returns>
        protected override (VertexBuffer<MeshVertex>, IndexBuffer<uint>?) InitializeMesh(IGraphicsDevice device)
        {
            CreateGeodesicSphere(device, out VertexBuffer<MeshVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer);
            return (vertexBuffer, indexBuffer);
        }

        private static readonly Vector3[] OctahedronVertices =
        {
            // when looking down the negative z-axis (into the screen)
            new(0,  1,  0), // 0 top
            new(0,  0, -1), // 1 front
            new(1,  0,  0), // 2 right
            new(0,  0,  1), // 3 back
            new(-1,  0,  0), // 4 left
            new(0, -1,  0), // 5 bottom
        };

        private static readonly uint[] OctahedronIndices =
        {
            0, 1, 2, // top front-right face
            0, 2, 3, // top back-right face
            0, 3, 4, // top back-left face
            0, 4, 1, // top front-left face
            5, 1, 4, // bottom front-left face
            5, 4, 3, // bottom back-left face
            5, 3, 2, // bottom back-right face
            5, 2, 1, // bottom front-right face
        };

        /// <summary>
        /// Generates vertices and indices for a geodesic sphere mesh.
        /// </summary>
        /// <param name="device">The graphics device used for mesh creation.</param>
        /// <param name="vertexBuffer">The vertex buffer of the geodesic sphere mesh.</param>
        /// <param name="indexBuffer">The optional index buffer of the geodesic sphere mesh.</param>
        /// <param name="diameter">The diameter of the geodesic sphere.</param>
        /// <param name="tessellation">The level of tessellation for the geodesic sphere.</param>
        public static unsafe void CreateGeodesicSphere(IGraphicsDevice device, out VertexBuffer<MeshVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer, float diameter = 1, uint tessellation = 3)
        {
            float radius = diameter / 2.0f;

            UnsafeList<Vector3> vertexPositions = new(OctahedronVertices);

            UnsafeList<uint> indices = new(OctahedronIndices);

            // We know these values by looking at the above index list for the octahedron. Despite the subdivisions that are
            // about to go on, these values aren't ever going to change because the vertices don't move around in the array.
            // We'll need these values later on to fix the singularities that show up at the poles.
            const ushort northPoleIndex = 0;
            const ushort southPoleIndex = 5;

            for (uint iSubdivision = 0; iSubdivision < tessellation; ++iSubdivision)
            {
                Trace.Assert(indices.Size % 3 == 0); // sanity

                // We use this to keep track of which edges have already been subdivided.
                Dictionary<(uint, uint), uint> subdividedEdges = new();

                // The new index collection after subdivision.
                UnsafeList<uint> newIndices = new();

                uint triangleCount = indices.Size / 3;
                for (uint iTriangle = 0; iTriangle < triangleCount; ++iTriangle)
                {
                    // For each edge on this triangle, create a new vertex in the middle of that edge.
                    // The winding order of the triangles we output are the same as the winding order of the inputs.

                    // Indices of the vertices making up this triangle
                    uint iv0 = indices[iTriangle * 3 + 0];
                    uint iv1 = indices[iTriangle * 3 + 1];
                    uint iv2 = indices[iTriangle * 3 + 2];

                    // Get the new vertices
                    Vector3 v01; // vertex on the midpoint of v0 and v1
                    Vector3 v12; // ditto v1 and v2
                    Vector3 v20; // ditto v2 and v0
                    uint iv01; // index of v01
                    uint iv12; // index of v12
                    uint iv20; // index of v20

                    // Function that, when given the index of two vertices, creates a new vertex at the midpoint of those vertices.
                    void divideEdge(uint i0, uint i1, out Vector3 outVertex, out uint outIndex)
                    {
                        (uint, uint) edge = (i0, i1);

                        if (subdividedEdges.TryGetValue(edge, out iv0))
                        {
                            outIndex = iv0;
                            outVertex = vertexPositions[iv0];
                        }
                        else
                        {
                            outVertex = (vertexPositions[i0] + vertexPositions[i1]) * 0.5f;
                            outIndex = vertexPositions.Size;
                            CheckIndexOverflow(outIndex);
                            vertexPositions.PushBack(outVertex);
                            subdividedEdges.Add(edge, outIndex);
                        }
                    }

                    // Add/get new vertices and their indices
                    divideEdge(iv0, iv1, out v01, out iv01);
                    divideEdge(iv1, iv2, out v12, out iv12);
                    divideEdge(iv0, iv2, out v20, out iv20);

                    // Add the new indices. We have four new triangles from our original one:
                    //        v0
                    //        o
                    //       /a\
                    //  v20 o---o v01
                    //     /b\c/d\
                    // v2 o---o---o v1
                    //       v12
                    newIndices.PushBack(iv0);
                    newIndices.PushBack(iv01);
                    newIndices.PushBack(iv20);

                    newIndices.PushBack(iv20);
                    newIndices.PushBack(iv12);
                    newIndices.PushBack(iv2);

                    newIndices.PushBack(iv20);
                    newIndices.PushBack(iv01);
                    newIndices.PushBack(iv12);

                    newIndices.PushBack(iv01);
                    newIndices.PushBack(iv1);
                    newIndices.PushBack(iv12);
                }

                indices.Move(newIndices);
            }

            // Now that we've completed subdivision, fill in the final vertex collection
            UnsafeList<MeshVertex> vertices = new(vertexPositions.Size);
            for (uint i = 0; i < vertexPositions.Size; i++)
            {
                Vector3 p = vertexPositions[i];

                Vector3 normal = Vector3.Normalize(p);
                Vector3 pos = normal * radius;

                Vector3 tangent;
                if (Vector3.Dot(Vector3.UnitY, normal) == 1.0f)
                {
                    tangent = Vector3.UnitX;
                }
                else
                {
                    tangent = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, normal));
                }

                Vector3 bitangent = Vector3.Cross(normal, tangent);

                // calculate texture coordinates for this vertex
                float longitude = MathF.Atan2(normal.X, -normal.Z);
                float latitude = MathF.Acos(normal.Y);

                float u = longitude / MathUtil.PI2 + 0.5f;
                float v = latitude / MathUtil.PI;

                Vector3 uv = new(1 - u, v, 0);
                vertices.PushBack(new(pos, uv, normal, tangent, bitangent));
            }

            vertexPositions.Release();

            // There are a couple of fixes to do. One is a texture coordinate wraparound fixup. At some point, there will be
            // a set of triangles somewhere in the mesh with texture coordinates such that the wraparound across 0.0/1.0
            // occurs across that triangle. Eg. when the left hand side of the triangle has a U coordinate of 0.98 and the
            // right hand side has a U coordinate of 0.0. The intent is that such a triangle should render with a U of 0.98 to
            // 1.0, not 0.98 to 0.0. If we don't do this fixup, there will be a visible seam across one side of the sphere.
            //
            // Luckily this is relatively easy to fix. There is a straight edge which runs down the prime meridian of the
            // completed sphere. If you imagine the vertices along that edge, they circumscribe a semicircular arc starting at
            // y=1 and ending at y=-1, and sweeping across the range of z=0 to z=1. x stays zero. It's along this edge that we
            // need to duplicate our vertices - and provide the correct texture coordinates.
            uint preFixupVertexCount = vertices.Size;
            for (uint i = 0; i < preFixupVertexCount; i++)
            {
                bool isOnPrimeMeridian = MathUtil.NearEqual(
                    new Vector4(vertices[i].UV.X, vertices[i].UV.Y, 0, 0),
                    Vector4.Zero,
                    MathUtil.SplatEpsilon);

                if (isOnPrimeMeridian)
                {
                    uint newIndex = vertices.Size; // the index of this vertex that we're about to add
                    CheckIndexOverflow(newIndex);

                    // copy this vertex, correct the texture coordinate, and add the vertex
                    MeshVertex v = vertices[i];
                    v.UV.X = 1;
                    vertices.PushBack(v);

                    // Now find all the triangles which contain this vertex and update them if necessary
                    for (uint j = 0; j < indices.Size; j += 3)
                    {
                        uint* triIndex0 = indices.GetPointer(j + 0);
                        uint* triIndex1 = indices.GetPointer(j + 1);
                        uint* triIndex2 = indices.GetPointer(j + 2);

                        if (*triIndex0 == i)
                        {
                            // nothing; just keep going
                        }
                        else if (*triIndex1 == i)
                        {
                            Swap(triIndex0, triIndex1); // swap the pointers (not the values)
                        }
                        else if (*triIndex2 == i)
                        {
                            Swap(triIndex0, triIndex2); // swap the pointers (not the values)
                        }
                        else
                        {
                            // this triangle doesn't use the vertex we're interested in
                            continue;
                        }

                        // If we got to this point then triIndex0 is the pointer to the index to the vertex we're looking at
                        Trace.Assert(*triIndex0 == i);
                        Trace.Assert(*triIndex1 != i && *triIndex2 != i); // assume no degenerate triangles

                        MeshVertex* v0 = vertices.GetPointer(*triIndex0);
                        MeshVertex* v1 = vertices.GetPointer(*triIndex1);
                        MeshVertex* v2 = vertices.GetPointer(*triIndex2);

                        // check the other two vertices to see if we might need to fix this triangle

                        if (MathF.Abs(v0->UV.X - v1->UV.X) > 0.5f ||
                            MathF.Abs(v0->UV.X - v2->UV.X) > 0.5f)
                        {
                            // yep; replace the specified index to point to the new, corrected vertex
                            *triIndex0 = newIndex;
                        }
                    }
                }
            }

            // And one last fix we need to do: the poles. A common use-case of a sphere mesh is to map a rectangular texture onto
            // it. If that happens, then the poles become singularities which map the entire top and bottom rows of the texture
            // onto a single point. In general there's no real way to do that right. But to match the behavior of non-geodesic
            // spheres, we need to duplicate the pole vertex for every triangle that uses it. This will introduce seams near the
            // poles, but reduce stretching.

            void fixPole(uint poleIndex)
            {
                MeshVertex* poleVertex = vertices.GetPointer(poleIndex);
                bool overwrittenPoleVertex = false; // overwriting the original pole vertex saves us one vertex

                for (uint i = 0; i < indices.Size; i += 3)
                {
                    // These pointers point to the three indices which make up this triangle. pPoleIndex is the pointer to the
                    // entry in the index array which represents the pole index, and the other two pointers point to the other
                    // two indices making up this triangle.
                    uint* pPoleIndex;
                    uint* pOtherIndex0;
                    uint* pOtherIndex1;
                    if (indices[i + 0] == poleIndex)
                    {
                        pPoleIndex = indices.GetPointer(i + 0);
                        pOtherIndex0 = indices.GetPointer(i + 1);
                        pOtherIndex1 = indices.GetPointer(i + 2);
                    }
                    else if (indices[i + 1] == poleIndex)
                    {
                        pPoleIndex = indices.GetPointer(i + 1);
                        pOtherIndex0 = indices.GetPointer(i + 2);
                        pOtherIndex1 = indices.GetPointer(i + 0);
                    }
                    else if (indices[i + 2] == poleIndex)
                    {
                        pPoleIndex = indices.GetPointer(i + 2);
                        pOtherIndex0 = indices.GetPointer(i + 0);
                        pOtherIndex1 = indices.GetPointer(i + 1);
                    }
                    else
                    {
                        continue;
                    }

                    MeshVertex* otherVertex0 = vertices.GetPointer(*pOtherIndex0);
                    MeshVertex* otherVertex1 = vertices.GetPointer(*pOtherIndex1);

                    // Calculate the texcoords for the new pole vertex, add it to the vertices and update the index
                    MeshVertex newPoleVertex = *poleVertex;
                    newPoleVertex.UV.X = (otherVertex0->UV.X + otherVertex1->UV.X) / 2;
                    newPoleVertex.UV.Y = poleVertex->UV.Y;

                    if (!overwrittenPoleVertex)
                    {
                        vertices[poleIndex] = newPoleVertex;
                        overwrittenPoleVertex = true;
                    }
                    else
                    {
                        CheckIndexOverflow(vertices.Size);

                        *pPoleIndex = vertices.Size;
                        vertices.PushBack(newPoleVertex);
                    }
                }
            }

            fixPole(northPoleIndex);
            fixPole(southPoleIndex);

            vertexBuffer = new VertexBuffer<MeshVertex>(device, vertices.Data, vertices.Size, CpuAccessFlags.None);
            indexBuffer = new IndexBuffer<uint>(device, indices.Data, indices.Size, CpuAccessFlags.None);

            vertices.Release();
            indices.Release();
        }

        private static void CheckIndexOverflow(uint value)
        {
            if (value >= uint.MaxValue)
                throw new IndexOutOfRangeException("Index _value out of range: cannot tesselate primitive so finely");
        }
    }
}