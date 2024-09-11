#region License

/*
MIT License

Copyright(c) 2017-2018 Mattias Edlund

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

#endregion

#region Original License

/////////////////////////////////////////////
//
// Mesh Simplification Tutorial
//
// (C) by Sven Forstmann in 2014
//
// License : MIT
// http://opensource.org/licenses/MIT
//
//https://github.com/sp4cerat/Fast-Quadric-Mesh-Simplification

#endregion

using Hexa.NET.Logging;
using Hexa.NET.Mathematics;
using HexaEngine.Core.MeshDecimator.Collections;
using System.Numerics;

namespace HexaEngine.Core.MeshDecimator.Algorithms
{
    /// <summary>
    /// The fast quadric mesh simplification algorithm.
    /// </summary>
    public sealed class FastQuadricMeshSimplification : DecimationAlgorithm
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(FastQuadricMeshSimplification));

        #region Consts

        private const double DoubleEpsilon = 1.0E-3;

        #endregion

        #region Classes

        #region Triangle

        private struct Triangle
        {
            #region Fields

            public uint V0;
            public uint V1;
            public uint V2;
            public int SubMeshIndex;

            public uint VA0;
            public uint VA1;
            public uint VA2;

            public double Err0;
            public double Err1;
            public double Err2;
            public double Err3;

            public bool Deleted;
            public bool Dirty;
            public Vector3D N;

            #endregion

            #region Properties

            public uint this[uint index]
            {
                readonly get
                {
                    return index == 0 ? V0 : index == 1 ? V1 : V2;
                }
                set
                {
                    switch (index)
                    {
                        case 0:
                            V0 = value;
                            break;

                        case 1:
                            V1 = value;
                            break;

                        case 2:
                            V2 = value;
                            break;

                        default:
                            throw new IndexOutOfRangeException();
                    }
                }
            }

            #endregion

            #region Constructor

            public Triangle(uint v0, uint v1, uint v2, int subMeshIndex)
            {
                V0 = v0;
                V1 = v1;
                V2 = v2;
                SubMeshIndex = subMeshIndex;

                VA0 = v0;
                VA1 = v1;
                VA2 = v2;

                Err0 = Err1 = Err2 = Err3 = 0;
                Deleted = Dirty = false;
                N = new Vector3D();
            }

            #endregion

            #region Public Methods

            public readonly void GetAttributeIndices(uint[] attributeIndices)
            {
                attributeIndices[0] = VA0;
                attributeIndices[1] = VA1;
                attributeIndices[2] = VA2;
            }

            public void SetAttributeIndex(uint index, uint value)
            {
                switch (index)
                {
                    case 0:
                        VA0 = value;
                        break;

                    case 1:
                        VA1 = value;
                        break;

                    case 2:
                        VA2 = value;
                        break;

                    default:
                        throw new IndexOutOfRangeException();
                }
            }

            public readonly void GetErrors(double[] err)
            {
                err[0] = Err0;
                err[1] = Err1;
                err[2] = Err2;
            }

            #endregion
        }

        #endregion

        #region Vertex

        private struct Vertex
        {
            public Vector3D p;
            public uint tstart;
            public uint tcount;
            public SymmetricMatrix q;
            public bool border;
            public bool seam;
            public bool foldover;

            public Vertex(Vector3D p)
            {
                this.p = p;
                tstart = 0;
                tcount = 0;
                q = new SymmetricMatrix();
                border = true;
                seam = false;
                foldover = false;
            }
        }

        #endregion

        #region Ref

        private struct Ref
        {
            public uint tid;
            public uint tvertex;

            public void Set(uint tid, uint tvertex)
            {
                this.tid = tid;
                this.tvertex = tvertex;
            }
        }

        #endregion

        #region Border Vertex

        private struct BorderVertex
        {
            public uint index;
            public int hash;

            public BorderVertex(uint index, int hash)
            {
                this.index = index;
                this.hash = hash;
            }
        }

        #endregion

        #region Border Vertex Comparer

        private class BorderVertexComparer : IComparer<BorderVertex>
        {
            public static readonly BorderVertexComparer instance = new();

            public int Compare(BorderVertex x, BorderVertex y)
            {
                return x.hash.CompareTo(y.hash);
            }
        }

        #endregion

        #endregion

        #region Fields

        private bool preserveSeams = false;
        private bool preserveFoldovers = false;
        private bool enableSmartLink = true;
        private int maxIterationCount = 100;
        private double agressiveness = 7.0;
        private double vertexLinkDistanceSqr = double.Epsilon;
        private const double DenomEpilson = 0.00000001;

        private int subMeshCount = 0;
        private readonly ResizableArray<Triangle> triangles;
        private readonly ResizableArray<Vertex> vertices;
        private readonly ResizableArray<Ref> refs;

        private ResizableArray<Vector3>? vertNormals;
        private ResizableArray<Vector3>? vertTangents;
        private UVChannels<Vector2>? vertUV2D;
        private UVChannels<Vector3>? vertUV3D;
        private UVChannels<Vector4>? vertUV4D;
        private ResizableArray<Vector4>? vertColors;
        private ResizableArray<BoneWeight>? vertBoneWeights;

        private int remainingVertices = 0;

        // Pre-allocated buffers
        private readonly double[] errArr = new double[3];

        private readonly uint[] attributeIndexArr = new uint[3];

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets if seams should be preserved.
        /// Default value: false
        /// </summary>
        public bool PreserveSeams
        {
            get { return preserveSeams; }
            set { preserveSeams = value; }
        }

        /// <summary>
        /// Gets or sets if foldovers should be preserved.
        /// Default value: false
        /// </summary>
        public bool PreserveFoldovers
        {
            get { return preserveFoldovers; }
            set { preserveFoldovers = value; }
        }

        /// <summary>
        /// Gets or sets if a feature for smarter vertex linking should be enabled, reducing artifacts in the
        /// decimated result at the cost of a slightly more expensive initialization by treating vertices at
        /// the same position as the same vertex while separating the attributes.
        /// Default value: true
        /// </summary>
        public bool EnableSmartLink
        {
            get { return enableSmartLink; }
            set { enableSmartLink = value; }
        }

        /// <summary>
        /// Gets or sets the maximum iteration count. Higher number is more expensive but can bring you closer to your target quality.
        /// Sometimes a lower maximum count might be desired in order to lower the performance cost.
        /// Default value: 10000
        /// </summary>
        public int MaxIterationCount
        {
            get { return maxIterationCount; }
            set { maxIterationCount = value; }
        }

        /// <summary>
        /// Gets or sets the agressiveness of this algorithm. Higher number equals higher quality, but more expensive to run.
        /// Default value: 10.0
        /// </summary>
        public double Agressiveness
        {
            get { return agressiveness; }
            set { agressiveness = value; }
        }

        /// <summary>
        /// Gets or sets the maximum squared distance between two vertices in order to link them.
        /// Note that this value is only used if EnableSmartLink is true.
        /// Default value: double.Epsilon
        /// </summary>
        public double VertexLinkDistanceSqr
        {
            get { return vertexLinkDistanceSqr; }
            set { vertexLinkDistanceSqr = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new fast quadric mesh simplification algorithm.
        /// </summary>
        public FastQuadricMeshSimplification()
        {
            triangles = new ResizableArray<Triangle>(0);
            vertices = new ResizableArray<Vertex>(0);
            refs = new ResizableArray<Ref>(0);
        }

        #endregion

        #region Private Methods

        #region Initialize Vertex Attribute

        private ResizableArray<T>? InitializeVertexAttribute<T>(T[]? attributeValues, string attributeName)
        {
            if (attributeValues != null && attributeValues.Length == vertices.Length)
            {
                ResizableArray<T> newArray = new(attributeValues.Length, attributeValues.Length);
                T[] newArrayData = newArray.Data;
                Array.Copy(attributeValues, 0, newArrayData, 0, attributeValues.Length);
                return newArray;
            }
            else if (attributeValues != null && attributeValues.Length > 0)
            {
                Logger.Error($"Failed to set vertex attribute '{attributeName}' with {attributeValues.Length} length of array, when {vertices.Length} was needed.");
            }
            return null;
        }

        #endregion

        #region Calculate Error

        private static double VertexError(ref SymmetricMatrix q, double x, double y, double z)
        {
            return q.M11 * x * x + 2 * q.M12 * x * y + 2 * q.M13 * x * z + 2 * q.M14 * x + q.M22 * y * y
                + 2 * q.M23 * y * z + 2 * q.M24 * y + q.M33 * z * z + 2 * q.M34 * z + q.M44;
        }

        private static double CalculateError(ref Vertex vert0, ref Vertex vert1, out Vector3D result, out int resultIndex)
        {
            // compute interpolated vertex
            SymmetricMatrix q = vert0.q + vert1.q;
            bool border = vert0.border & vert1.border;
            double error;
            double det = q.Determinant1();
            if (det != 0.0 && !border)
            {
                // q_delta is invertible
                result = new Vector3D(
                    -1.0 / det * q.Determinant2(),  // vx = A41/det(q_delta)
                    1.0 / det * q.Determinant3(),   // vy = A42/det(q_delta)
                    -1.0 / det * q.Determinant4()); // vz = A43/det(q_delta)
                error = VertexError(ref q, result.X, result.Y, result.Z);
                resultIndex = 2;
            }
            else
            {
                // det = 0 -> try to find best result
                Vector3D p1 = vert0.p;
                Vector3D p2 = vert1.p;
                Vector3D p3 = (p1 + p2) * 0.5f;
                double error1 = VertexError(ref q, p1.X, p1.Y, p1.Z);
                double error2 = VertexError(ref q, p2.X, p2.Y, p2.Z);
                double error3 = VertexError(ref q, p3.X, p3.Y, p3.Z);
                error = Math.Min(error1, Math.Min(error2, error3));
                if (error == error3)
                {
                    result = p3;
                    resultIndex = 2;
                }
                else if (error == error2)
                {
                    result = p2;
                    resultIndex = 1;
                }
                else if (error == error1)
                {
                    result = p1;
                    resultIndex = 0;
                }
                else
                {
                    result = p3;
                    resultIndex = 2;
                }
            }
            return error;
        }

        #endregion

        #region Flipped

        /// <summary>
        /// Check if a triangle flips when this edge is removed
        /// </summary>
        private bool Flipped(ref Vector3D p, uint i0, uint i1, ref Vertex v0, bool[] deleted)
        {
            uint tcount = v0.tcount;
            Ref[] refs = this.refs.Data;
            Triangle[] triangles = this.triangles.Data;
            Vertex[] vertices = this.vertices.Data;
            for (int k = 0; k < tcount; k++)
            {
                Ref r = refs[v0.tstart + k];
                if (triangles[r.tid].Deleted)
                {
                    continue;
                }

                uint s = r.tvertex;
                uint id1 = triangles[r.tid][(s + 1) % 3];
                uint id2 = triangles[r.tid][(s + 2) % 3];
                if (id1 == i1 || id2 == i1)
                {
                    deleted[k] = true;
                    continue;
                }

                Vector3D d1 = vertices[id1].p - p;
                d1 = Vector3D.Normalize(d1);
                Vector3D d2 = vertices[id2].p - p;
                d2 = Vector3D.Normalize(d2);
                double dot = Vector3D.Dot(d1, d2);
                if (Math.Abs(dot) > 0.999)
                {
                    return true;
                }

                Vector3D n = Vector3D.Cross(d1, d2);
                n = Vector3D.Normalize(n);
                deleted[k] = false;
                dot = Vector3D.Dot(n, triangles[r.tid].N);
                if (dot < 0.2)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Update Triangles

        /// <summary>
        /// Update triangle connections and edge error after a edge is collapsed.
        /// </summary>
        private void UpdateTriangles(uint i0, uint ia0, ref Vertex v, ResizableArray<bool> deleted, ref uint deletedTriangles)
        {
            uint tcount = v.tcount;
            Triangle[] triangles = this.triangles.Data;
            Vertex[] vertices = this.vertices.Data;
            for (int k = 0; k < tcount; k++)
            {
                Ref r = refs[v.tstart + k];
                uint tid = r.tid;
                Triangle t = triangles[tid];
                if (t.Deleted)
                {
                    continue;
                }

                if (deleted[k])
                {
                    triangles[tid].Deleted = true;
                    ++deletedTriangles;
                    continue;
                }

                t[r.tvertex] = i0;
                if (ia0 != unchecked((uint)-1))
                {
                    t.SetAttributeIndex(r.tvertex, ia0);
                }

                t.Dirty = true;
                t.Err0 = CalculateError(ref vertices[t.V0], ref vertices[t.V1], out _, out _);
                t.Err1 = CalculateError(ref vertices[t.V1], ref vertices[t.V2], out _, out _);
                t.Err2 = CalculateError(ref vertices[t.V2], ref vertices[t.V0], out _, out _);
                t.Err3 = Math.Min(t.Err0, Math.Min(t.Err1, t.Err2));
                triangles[tid] = t;
                refs.Add(r);
            }
        }

        #endregion

        #region Are UVs The Same

        private bool AreUVsTheSame(int channel, uint indexA, uint indexB)
        {
            if (vertUV2D != null)
            {
                ResizableArray<Vector2>? vertUV = vertUV2D[channel];
                if (vertUV != null)
                {
                    Vector2 uvA = vertUV[indexA];
                    Vector2 uvB = vertUV[indexB];
                    return uvA == uvB;
                }
            }

            if (vertUV3D != null)
            {
                ResizableArray<Vector3>? vertUV = vertUV3D[channel];
                if (vertUV != null)
                {
                    Vector3 uvA = vertUV[indexA];
                    Vector3 uvB = vertUV[indexB];
                    return uvA == uvB;
                }
            }

            if (vertUV4D != null)
            {
                ResizableArray<Vector4>? vertUV = vertUV4D[channel];
                if (vertUV != null)
                {
                    Vector4 uvA = vertUV[indexA];
                    Vector4 uvB = vertUV[indexB];
                    return uvA == uvB;
                }
            }

            return false;
        }

        #endregion

        #region Remove Vertex Pass

        /// <summary>
        /// Remove vertices and mark deleted triangles
        /// </summary>
        private void RemoveVertexPass(uint startTrisCount, uint targetTrisCount, double threshold, ResizableArray<bool> deleted0, ResizableArray<bool> deleted1, ref uint deletedTris)
        {
            Triangle[] triangles = this.triangles.Data;
            uint triangleCount = this.triangles.Length;
            Vertex[] vertices = this.vertices.Data;

            bool preserveBorders = PreserveBorders;
            uint maxVertexCount = MaxVertexCount;
            if (maxVertexCount <= 0)
            {
                maxVertexCount = int.MaxValue;
            }

            for (uint tid = 0; tid < triangleCount; tid++)
            {
                if (triangles[tid].Dirty || triangles[tid].Deleted || triangles[tid].Err3 > threshold)
                {
                    continue;
                }

                triangles[tid].GetErrors(errArr);
                triangles[tid].GetAttributeIndices(attributeIndexArr);
                for (uint edgeIndex = 0; edgeIndex < 3; edgeIndex++)
                {
                    if (errArr[edgeIndex] > threshold)
                    {
                        continue;
                    }

                    uint nextEdgeIndex = (edgeIndex + 1) % 3;
                    uint i0 = triangles[tid][edgeIndex];
                    uint i1 = triangles[tid][nextEdgeIndex];

                    // Border check
                    if (vertices[i0].border != vertices[i1].border)
                    {
                        continue;
                    }
                    // Seam check
                    else if (vertices[i0].seam != vertices[i1].seam)
                    {
                        continue;
                    }
                    // Foldover check
                    else if (vertices[i0].foldover != vertices[i1].foldover)
                    {
                        continue;
                    }
                    // If borders should be preserved
                    else if (preserveBorders && vertices[i0].border)
                    {
                        continue;
                    }
                    // If seams should be preserved
                    else if (preserveSeams && vertices[i0].seam)
                    {
                        continue;
                    }
                    // If foldovers should be preserved
                    else if (preserveFoldovers && vertices[i0].foldover)
                    {
                        continue;
                    }

                    // Compute vertex to collapse to
                    CalculateError(ref vertices[i0], ref vertices[i1], out Vector3D p, out int pIndex);
                    deleted0.Resize(vertices[i0].tcount); // normals temporarily
                    deleted1.Resize(vertices[i1].tcount); // normals temporarily

                    // Don't remove if flipped
                    if (Flipped(ref p, i0, i1, ref vertices[i0], deleted0.Data))
                    {
                        continue;
                    }

                    if (Flipped(ref p, i1, i0, ref vertices[i1], deleted1.Data))
                    {
                        continue;
                    }

                    // Calculate the barycentric coordinates within the triangle
                    uint nextNextEdgeIndex = (edgeIndex + 2) % 3;
                    uint i2 = triangles[tid][nextNextEdgeIndex];
                    CalculateBarycentricCoords(ref p, ref vertices[i0].p, ref vertices[i1].p, ref vertices[i2].p, out Vector3 barycentricCoord);

                    // Not flipped, so remove edge
                    vertices[i0].p = p;
                    vertices[i0].q += vertices[i1].q;

                    uint ia0 = attributeIndexArr[edgeIndex];
                    uint ia1 = attributeIndexArr[nextEdgeIndex];
                    uint ia2 = attributeIndexArr[nextNextEdgeIndex];
                    InterpolateVertexAttributes(ia0, ia0, ia1, ia2, ref barycentricCoord);

                    if (vertices[i0].seam)
                    {
                        ia0 = unchecked((uint)-1);
                    }

                    uint tstart = refs.Length;
                    UpdateTriangles(i0, ia0, ref vertices[i0], deleted0, ref deletedTris);
                    UpdateTriangles(i0, ia0, ref vertices[i1], deleted1, ref deletedTris);

                    uint tcount = refs.Length - tstart;
                    if (tcount <= vertices[i0].tcount)
                    {
                        // save ram
                        if (tcount > 0)
                        {
                            Ref[] refsArr = refs.Data;
                            Array.Copy(refsArr, tstart, refsArr, vertices[i0].tstart, tcount);
                        }
                    }
                    else
                    {
                        // append
                        vertices[i0].tstart = tstart;
                    }

                    vertices[i0].tcount = tcount;
                    --remainingVertices;
                    break;
                }

                // Check if we are already done
                if (startTrisCount - deletedTris <= targetTrisCount && remainingVertices < maxVertexCount)
                {
                    break;
                }
            }
        }

        #region Calculate Barycentric Coordinates

        private static void CalculateBarycentricCoords(ref Vector3D point, ref Vector3D a, ref Vector3D b, ref Vector3D c, out Vector3 result)
        {
            Vector3D v0 = b - a, v1 = c - a, v2 = point - a;
            double d00 = Vector3D.Dot(v0, v0);
            double d01 = Vector3D.Dot(v0, v1);
            double d11 = Vector3D.Dot(v1, v1);
            double d20 = Vector3D.Dot(v2, v0);
            double d21 = Vector3D.Dot(v2, v1);
            double denom = d00 * d11 - d01 * d01;

            // Make sure the denominator is not too small to cause math problems
            if (Math.Abs(denom) < DenomEpilson)
            {
                denom = DenomEpilson;
            }

            double v = (d11 * d20 - d01 * d21) / denom;
            double w = (d00 * d21 - d01 * d20) / denom;
            double u = 1.0 - v - w;
            result = new Vector3((float)u, (float)v, (float)w);
        }

        #endregion

        #region Interpolate Vertex Attributes

        private void InterpolateVertexAttributes(uint dst, uint i0, uint i1, uint i2, ref Vector3 barycentricCoord)
        {
            if (vertNormals != null)
            {
                vertNormals[dst] = Vector3.Normalize(vertNormals[i0] * barycentricCoord.X + vertNormals[i1] * barycentricCoord.Y + vertNormals[i2] * barycentricCoord.Z);
            }
            if (vertTangents != null)
            {
                vertTangents[dst] = Vector3.Normalize(vertTangents[i0] * barycentricCoord.X + vertTangents[i1] * barycentricCoord.Y + vertTangents[i2] * barycentricCoord.Z);
            }
            if (vertUV2D != null)
            {
                for (int i = 0; i < Mesh.UVChannelCount; i++)
                {
                    var vertUV = vertUV2D[i];
                    if (vertUV != null)
                    {
                        vertUV[dst] = vertUV[i0] * barycentricCoord.X + vertUV[i1] * barycentricCoord.Y + vertUV[i2] * barycentricCoord.Z;
                    }
                }
            }
            if (vertUV3D != null)
            {
                for (int i = 0; i < Mesh.UVChannelCount; i++)
                {
                    var vertUV = vertUV3D[i];
                    if (vertUV != null)
                    {
                        vertUV[dst] = vertUV[i0] * barycentricCoord.X + vertUV[i1] * barycentricCoord.Y + vertUV[i2] * barycentricCoord.Z;
                    }
                }
            }
            if (vertUV4D != null)
            {
                for (int i = 0; i < Mesh.UVChannelCount; i++)
                {
                    var vertUV = vertUV4D[i];
                    if (vertUV != null)
                    {
                        vertUV[dst] = vertUV[i0] * barycentricCoord.X + vertUV[i1] * barycentricCoord.Y + vertUV[i2] * barycentricCoord.Z;
                    }
                }
            }
            if (vertColors != null)
            {
                vertColors[dst] = vertColors[i0] * barycentricCoord.X + vertColors[i1] * barycentricCoord.Y + vertColors[i2] * barycentricCoord.Z;
            }

            // TODO: Do we have to blend bone weights at all or can we just keep them as it is in this scenario?
        }

        #endregion

        #endregion

        #region Update Mesh

        /// <summary>
        /// Compact triangles, compute edge error and build reference list.
        /// </summary>
        /// <param name="iteration">The iteration index.</param>
        private void UpdateMesh(int iteration)
        {
            Triangle[] triangles = this.triangles.Data;
            Vertex[] vertices = this.vertices.Data;

            uint triangleCount = this.triangles.Length;
            uint vertexCount = this.vertices.Length;
            if (iteration > 0) // compact triangles
            {
                uint dst = 0;
                for (int i = 0; i < triangleCount; i++)
                {
                    if (!triangles[i].Deleted)
                    {
                        if (dst != i)
                        {
                            triangles[dst] = triangles[i];
                        }
                        dst++;
                    }
                }
                this.triangles.Resize(dst);
                triangles = this.triangles.Data;
                triangleCount = dst;
            }

            UpdateReferences();

            // Identify boundary : vertices[].border=0,1
            if (iteration == 0)
            {
                Ref[] refs = this.refs.Data;

                List<int> vcount = new(8);
                List<uint> vids = new(8);
                for (int i = 0; i < vertexCount; i++)
                {
                    vertices[i].border = false;
                    vertices[i].seam = false;
                    vertices[i].foldover = false;
                }

                int ofs;
                uint id;
                int borderVertexCount = 0;
                double borderMinX = double.MaxValue;
                double borderMaxX = double.MinValue;
                for (uint i = 0; i < vertexCount; i++)
                {
                    uint tstart = vertices[i].tstart;
                    uint tcount = vertices[i].tcount;
                    vcount.Clear();
                    vids.Clear();
                    int vsize = 0;

                    for (uint j = 0; j < tcount; j++)
                    {
                        uint tid = refs[tstart + j].tid;
                        for (uint k = 0; k < 3; k++)
                        {
                            ofs = 0;
                            id = triangles[tid][k];
                            while (ofs < vsize)
                            {
                                if (vids[ofs] == id)
                                {
                                    break;
                                }

                                ++ofs;
                            }

                            if (ofs == vsize)
                            {
                                vcount.Add(1);
                                vids.Add(id);
                                ++vsize;
                            }
                            else
                            {
                                ++vcount[ofs];
                            }
                        }
                    }

                    for (int j = 0; j < vsize; j++)
                    {
                        if (vcount[j] == 1)
                        {
                            id = vids[j];
                            vertices[id].border = true;
                            ++borderVertexCount;

                            if (enableSmartLink)
                            {
                                if (vertices[id].p.X < borderMinX)
                                {
                                    borderMinX = vertices[id].p.X;
                                }
                                if (vertices[id].p.X > borderMaxX)
                                {
                                    borderMaxX = vertices[id].p.X;
                                }
                            }
                        }
                    }
                }

                if (enableSmartLink)
                {
                    // First find all border vertices
                    BorderVertex[] borderVertices = new BorderVertex[borderVertexCount];
                    uint borderIndexCount = 0;
                    double borderAreaWidth = borderMaxX - borderMinX;
                    for (uint i = 0; i < vertexCount; i++)
                    {
                        if (vertices[i].border)
                        {
                            int vertexHash = (int)(((vertices[i].p.X - borderMinX) / borderAreaWidth * 2.0 - 1.0) * int.MaxValue);
                            borderVertices[borderIndexCount] = new BorderVertex(i, vertexHash);
                            ++borderIndexCount;
                        }
                    }

                    // Sort the border vertices by hash
                    Array.Sort(borderVertices, BorderVertexComparer.instance);

                    // Calculate the maximum hash distance based on the maximum vertex link distance
                    double vertexLinkDistance = Math.Sqrt(vertexLinkDistanceSqr);
                    int hashMaxDistance = Math.Max((int)(vertexLinkDistance / borderAreaWidth * int.MaxValue), 1);

                    // Then find identical border vertices and bind them together as one
                    for (int i = 0; i < borderIndexCount; i++)
                    {
                        uint myIndex = borderVertices[i].index;
                        if (myIndex == unchecked((uint)-1))
                        {
                            continue;
                        }

                        Vector3D myPoint = vertices[myIndex].p;
                        for (int j = i + 1; j < borderIndexCount; j++)
                        {
                            uint otherIndex = borderVertices[j].index;
                            if (otherIndex == unchecked((uint)-1))
                            {
                                continue;
                            }
                            else if (borderVertices[j].hash - borderVertices[i].hash > hashMaxDistance) // There is no point to continue beyond this point
                            {
                                break;
                            }

                            Vector3D otherPoint = vertices[otherIndex].p;
                            double sqrX = (myPoint.X - otherPoint.X) * (myPoint.X - otherPoint.X);
                            double sqrY = (myPoint.Y - otherPoint.Y) * (myPoint.Y - otherPoint.Y);
                            double sqrZ = (myPoint.Z - otherPoint.Z) * (myPoint.Z - otherPoint.Z);
                            double sqrMagnitude = sqrX + sqrY + sqrZ;

                            if (sqrMagnitude <= vertexLinkDistanceSqr)
                            {
                                borderVertices[j].index = unchecked((uint)-1); // NOTE: This makes sure that the "other" vertex is not processed again
                                vertices[myIndex].border = false;
                                vertices[otherIndex].border = false;

                                if (AreUVsTheSame(0, myIndex, otherIndex))
                                {
                                    vertices[myIndex].foldover = true;
                                    vertices[otherIndex].foldover = true;
                                }
                                else
                                {
                                    vertices[myIndex].seam = true;
                                    vertices[otherIndex].seam = true;
                                }

                                uint otherTriangleCount = vertices[otherIndex].tcount;
                                uint otherTriangleStart = vertices[otherIndex].tstart;
                                for (uint k = 0; k < otherTriangleCount; k++)
                                {
                                    Ref r = refs[otherTriangleStart + k];
                                    triangles[r.tid][r.tvertex] = myIndex;
                                }
                            }
                        }
                    }

                    // Update the references again
                    UpdateReferences();
                }

                // Init Quadrics by Plane & Edge Errors
                //
                // required at the beginning ( iteration == 0 )
                // recomputing during the simplification is not required,
                // but mostly improves the result for closed meshes
                for (int i = 0; i < vertexCount; i++)
                {
                    vertices[i].q = new SymmetricMatrix();
                }

                uint v0, v1, v2;
                Vector3D p0, p1, p2, p10, p20;
                SymmetricMatrix sm;
                for (int i = 0; i < triangleCount; i++)
                {
                    v0 = triangles[i].V0;
                    v1 = triangles[i].V1;
                    v2 = triangles[i].V2;

                    p0 = vertices[v0].p;
                    p1 = vertices[v1].p;
                    p2 = vertices[v2].p;
                    p10 = p1 - p0;
                    p20 = p2 - p0;
                    Vector3D n = Vector3D.Cross(p10, p20);
                    n = Vector3D.Normalize(n);
                    triangles[i].N = n;

                    sm = new SymmetricMatrix(n.X, n.Y, n.Z, -Vector3D.Dot(n, p0));
                    vertices[v0].q += sm;
                    vertices[v1].q += sm;
                    vertices[v2].q += sm;
                }

                for (int i = 0; i < triangleCount; i++)
                {
                    // Calc Edge Error
                    Triangle triangle = triangles[i];
                    triangles[i].Err0 = CalculateError(ref vertices[triangle.V0], ref vertices[triangle.V1], out _, out _);
                    triangles[i].Err1 = CalculateError(ref vertices[triangle.V1], ref vertices[triangle.V2], out _, out _);
                    triangles[i].Err2 = CalculateError(ref vertices[triangle.V2], ref vertices[triangle.V0], out _, out _);
                    triangles[i].Err3 = Math.Min(triangles[i].Err0, Math.Min(triangles[i].Err1, triangles[i].Err2));
                }
            }
        }

        #endregion

        #region Update References

        private void UpdateReferences()
        {
            uint triangleCount = this.triangles.Length;
            uint vertexCount = this.vertices.Length;
            Triangle[] triangles = this.triangles.Data;
            Vertex[] vertices = this.vertices.Data;

            // Init Reference ID list
            for (uint i = 0; i < vertexCount; i++)
            {
                vertices[i].tstart = 0;
                vertices[i].tcount = 0;
            }

            for (uint i = 0; i < triangleCount; i++)
            {
                ++vertices[triangles[i].V0].tcount;
                ++vertices[triangles[i].V1].tcount;
                ++vertices[triangles[i].V2].tcount;
            }

            uint tstart = 0;
            remainingVertices = 0;
            for (uint i = 0; i < vertexCount; i++)
            {
                vertices[i].tstart = tstart;
                if (vertices[i].tcount > 0)
                {
                    tstart += vertices[i].tcount;
                    vertices[i].tcount = 0;
                    ++remainingVertices;
                }
            }

            // Write References
            this.refs.Resize(tstart);
            Ref[] refs = this.refs.Data;
            for (uint i = 0; i < triangleCount; i++)
            {
                uint v0 = triangles[i].V0;
                uint v1 = triangles[i].V1;
                uint v2 = triangles[i].V2;
                uint start0 = vertices[v0].tstart;
                uint count0 = vertices[v0].tcount;
                uint start1 = vertices[v1].tstart;
                uint count1 = vertices[v1].tcount;
                uint start2 = vertices[v2].tstart;
                uint count2 = vertices[v2].tcount;

                refs[start0 + count0].Set(i, 0);
                refs[start1 + count1].Set(i, 1);
                refs[start2 + count2].Set(i, 2);

                ++vertices[v0].tcount;
                ++vertices[v1].tcount;
                ++vertices[v2].tcount;
            }
        }

        #endregion

        #region Compact Mesh

        /// <summary>
        /// Finally compact mesh before exiting.
        /// </summary>
        private void CompactMesh()
        {
            uint dst = 0;
            Vertex[] vertices = this.vertices.Data;
            uint vertexCount = this.vertices.Length;
            for (uint i = 0; i < vertexCount; i++)
            {
                vertices[i].tcount = 0;
            }

            Vector3[]? vertNormals = this.vertNormals?.Data;
            Vector3[]? vertTangents = this.vertTangents?.Data;
            Vector2[]?[]? vertUV2D = this.vertUV2D?.Data;
            Vector3[]?[]? vertUV3D = this.vertUV3D?.Data;
            Vector4[]?[]? vertUV4D = this.vertUV4D?.Data;
            Vector4[]? vertColors = this.vertColors?.Data;
            BoneWeight[]? vertBoneWeights = this.vertBoneWeights?.Data;

            Triangle[] triangles = this.triangles.Data;
            uint triangleCount = this.triangles.Length;
            for (uint i = 0; i < triangleCount; i++)
            {
                Triangle triangle = triangles[i];
                if (!triangle.Deleted)
                {
                    if (triangle.VA0 != triangle.V0)
                    {
                        uint iDest = triangle.VA0;
                        uint iSrc = triangle.V0;
                        vertices[iDest].p = vertices[iSrc].p;
                        if (vertBoneWeights != null)
                        {
                            vertBoneWeights[iDest] = vertBoneWeights[iSrc];
                        }
                        triangle.V0 = triangle.VA0;
                    }
                    if (triangle.VA1 != triangle.V1)
                    {
                        uint iDest = triangle.VA1;
                        uint iSrc = triangle.V1;
                        vertices[iDest].p = vertices[iSrc].p;
                        if (vertBoneWeights != null)
                        {
                            vertBoneWeights[iDest] = vertBoneWeights[iSrc];
                        }
                        triangle.V1 = triangle.VA1;
                    }
                    if (triangle.VA2 != triangle.V2)
                    {
                        uint iDest = triangle.VA2;
                        uint iSrc = triangle.V2;
                        vertices[iDest].p = vertices[iSrc].p;
                        if (vertBoneWeights != null)
                        {
                            vertBoneWeights[iDest] = vertBoneWeights[iSrc];
                        }
                        triangle.V2 = triangle.VA2;
                    }

                    triangles[dst++] = triangle;

                    vertices[triangle.V0].tcount = 1;
                    vertices[triangle.V1].tcount = 1;
                    vertices[triangle.V2].tcount = 1;
                }
            }

            triangleCount = dst;
            this.triangles.Resize(triangleCount);
            triangles = this.triangles.Data;

            dst = 0;
            for (uint i = 0; i < vertexCount; i++)
            {
                Vertex vert = vertices[i];
                if (vert.tcount > 0)
                {
                    vert.tstart = dst;
                    vertices[i] = vert;

                    if (dst != i)
                    {
                        vertices[dst].p = vert.p;
                        if (vertNormals != null)
                        {
                            vertNormals[dst] = vertNormals[i];
                        }

                        if (vertTangents != null)
                        {
                            vertTangents[dst] = vertTangents[i];
                        }

                        if (vertUV2D != null)
                        {
                            for (int j = 0; j < Mesh.UVChannelCount; j++)
                            {
                                Vector2[]? vertUV = vertUV2D[j];
                                if (vertUV != null)
                                {
                                    vertUV[dst] = vertUV[i];
                                }
                            }
                        }
                        if (vertUV3D != null)
                        {
                            for (int j = 0; j < Mesh.UVChannelCount; j++)
                            {
                                Vector3[]? vertUV = vertUV3D[j];
                                if (vertUV != null)
                                {
                                    vertUV[dst] = vertUV[i];
                                }
                            }
                        }
                        if (vertUV4D != null)
                        {
                            for (int j = 0; j < Mesh.UVChannelCount; j++)
                            {
                                Vector4[]? vertUV = vertUV4D[j];
                                if (vertUV != null)
                                {
                                    vertUV[dst] = vertUV[i];
                                }
                            }
                        }
                        if (vertColors != null)
                        {
                            vertColors[dst] = vertColors[i];
                        }

                        if (vertBoneWeights != null)
                        {
                            vertBoneWeights[dst] = vertBoneWeights[i];
                        }
                    }
                    ++dst;
                }
            }

            for (uint i = 0; i < triangleCount; i++)
            {
                Triangle triangle = triangles[i];
                triangle.V0 = vertices[triangle.V0].tstart;
                triangle.V1 = vertices[triangle.V1].tstart;
                triangle.V2 = vertices[triangle.V2].tstart;
                triangles[i] = triangle;
            }

            vertexCount = dst;
            this.vertices.Resize(vertexCount);
            this.vertNormals?.Resize(vertexCount, true);
            this.vertTangents?.Resize(vertexCount, true);
            this.vertUV2D?.Resize(vertexCount, true);
            this.vertUV3D?.Resize(vertexCount, true);
            this.vertUV4D?.Resize(vertexCount, true);
            this.vertColors?.Resize(vertexCount, true);
            this.vertBoneWeights?.Resize(vertexCount, true);
        }

        #endregion

        #endregion

        #region Public Methods

        #region Initialize

        /// <summary>
        /// Initializes the algorithm with the original mesh.
        /// </summary>
        /// <param name="mesh">The mesh.</param>
        public override void Initialize(Mesh mesh)
        {
            ArgumentNullException.ThrowIfNull(mesh);

            int meshSubMeshCount = mesh.SubMeshCount;
            uint meshTriangleCount = mesh.TriangleCount;
            Vector3D[] meshVertices = mesh.Vertices;
            Vector3[]? meshNormals = mesh.Normals;
            Vector3[]? meshTangents = mesh.Tangents;
            Vector4[]? meshColors = mesh.Colors;
            BoneWeight[]? meshBoneWeights = mesh.BoneWeights;
            subMeshCount = meshSubMeshCount;

            vertices.Resize((uint)meshVertices.LongLength);
            Vertex[] vertArr = vertices.Data;
            for (uint i = 0; i < meshVertices.LongLength; i++)
            {
                vertArr[i] = new Vertex(meshVertices[i]);
            }

            triangles.Resize(meshTriangleCount);
            Triangle[] trisArr = triangles.Data;
            int triangleIndex = 0;
            for (int subMeshIndex = 0; subMeshIndex < meshSubMeshCount; subMeshIndex++)
            {
                uint[] subMeshIndices = mesh.GetIndices(subMeshIndex);
                int subMeshTriangleCount = subMeshIndices.Length / 3;
                for (int i = 0; i < subMeshTriangleCount; i++)
                {
                    int offset = i * 3;
                    uint v0 = subMeshIndices[offset];
                    uint v1 = subMeshIndices[offset + 1];
                    uint v2 = subMeshIndices[offset + 2];
                    trisArr[triangleIndex++] = new Triangle(v0, v1, v2, subMeshIndex);
                }
            }

            vertNormals = InitializeVertexAttribute(meshNormals, "normals");
            vertTangents = InitializeVertexAttribute(meshTangents, "tangents");
            vertColors = InitializeVertexAttribute(meshColors, "colors");
            vertBoneWeights = InitializeVertexAttribute(meshBoneWeights, "boneWeights");

            for (int i = 0; i < Mesh.UVChannelCount; i++)
            {
                int uvDim = mesh.GetUVDimension(i);
                string uvAttributeName = $"uv{i}";
                if (uvDim == 2)
                {
                    vertUV2D ??= new UVChannels<Vector2>();

                    Vector2[]? uvs = mesh.GetUVs2D(i);
                    vertUV2D[i] = InitializeVertexAttribute(uvs, uvAttributeName);
                }
                else if (uvDim == 3)
                {
                    vertUV3D ??= new UVChannels<Vector3>();

                    Vector3[]? uvs = mesh.GetUVs3D(i);
                    vertUV3D[i] = InitializeVertexAttribute(uvs, uvAttributeName);
                }
                else if (uvDim == 4)
                {
                    vertUV4D ??= new UVChannels<Vector4>();

                    Vector4[]? uvs = mesh.GetUVs4D(i);
                    vertUV4D[i] = InitializeVertexAttribute(uvs, uvAttributeName);
                }
            }
        }

        #endregion

        #region Decimate Mesh

        /// <summary>
        /// Decimates the mesh.
        /// </summary>
        /// <param name="targetTrisCount">The target triangle count.</param>
        public override void DecimateMesh(uint targetTrisCount)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(targetTrisCount);

            uint deletedTris = 0;
            ResizableArray<bool> deleted0 = new(20);
            ResizableArray<bool> deleted1 = new(20);
            Triangle[] triangles = this.triangles.Data;
            uint triangleCount = this.triangles.Length;
            uint startTrisCount = triangleCount;

            uint maxVertexCount = MaxVertexCount;
            if (maxVertexCount <= 0)
            {
                maxVertexCount = int.MaxValue;
            }

            for (int iteration = 0; iteration < maxIterationCount; iteration++)
            {
                ReportStatus(iteration, startTrisCount, startTrisCount - deletedTris, targetTrisCount);
                if (startTrisCount - deletedTris <= targetTrisCount && remainingVertices < maxVertexCount)
                {
                    break;
                }

                // Update mesh once in a while
                if (iteration % 5 == 0)
                {
                    UpdateMesh(iteration);
                    triangles = this.triangles.Data;
                    triangleCount = this.triangles.Length;
                }

                // Clear dirty flag
                for (int i = 0; i < triangleCount; i++)
                {
                    triangles[i].Dirty = false;
                }

                // All triangles with edges below the threshold will be removed
                //
                // The following numbers works well for most models.
                // If it does not, try to adjust the 3 parameters
                double threshold = 0.000000001 * Math.Pow(iteration + 3, agressiveness);

                if (Verbose && iteration % 5 == 0)
                {
                    Logger.Info($"iteration {iteration} - triangles {startTrisCount - deletedTris} threshold {threshold}");
                }

                // Remove vertices & mark deleted triangles
                RemoveVertexPass(startTrisCount, targetTrisCount, threshold, deleted0, deleted1, ref deletedTris);
            }

            CompactMesh();
        }

        #endregion

        #region Decimate Mesh Lossless

        /// <summary>
        /// Decimates the mesh without losing any quality.
        /// </summary>
        public override void DecimateMeshLossless()
        {
            uint deletedTris = 0;
            ResizableArray<bool> deleted0 = new(0);
            ResizableArray<bool> deleted1 = new(0);
            Triangle[] triangles;
            uint triangleCount = this.triangles.Length;
            uint startTrisCount = triangleCount;

            ReportStatus(0, startTrisCount, startTrisCount, unchecked((uint)-1));
            for (int iteration = 0; iteration < 9999; iteration++)
            {
                // Update mesh constantly
                UpdateMesh(iteration);
                triangles = this.triangles.Data;
                triangleCount = this.triangles.Length;

                ReportStatus(iteration, startTrisCount, triangleCount, unchecked((uint)-1));

                // Clear dirty flag
                for (int i = 0; i < triangleCount; i++)
                {
                    triangles[i].Dirty = false;
                }

                // All triangles with edges below the threshold will be removed
                //
                // The following numbers works well for most models.
                // If it does not, try to adjust the 3 parameters
                double threshold = DoubleEpsilon;

                if (Verbose)
                {
                    Logger.Info($"Lossless iteration {iteration}");
                }

                // Remove vertices & mark deleted triangles
                RemoveVertexPass(startTrisCount, 0, threshold, deleted0, deleted1, ref deletedTris);

                if (deletedTris <= 0)
                {
                    break;
                }

                deletedTris = 0;
            }

            CompactMesh();
        }

        #endregion

        #region To Mesh

        /// <summary>
        /// Returns the resulting mesh.
        /// </summary>
        /// <returns>The resulting mesh.</returns>
        public override Mesh ToMesh()
        {
            uint vertexCount = this.vertices.Length;
            uint triangleCount = triangles.Length;
            Vector3D[] vertices = new Vector3D[vertexCount];
            uint[][] indices = new uint[subMeshCount][];

            Vertex[] vertArr = this.vertices.Data;
            for (int i = 0; i < vertexCount; i++)
            {
                vertices[i] = vertArr[i].p;
            }

            // First get the sub-mesh offsets
            Triangle[] triArr = triangles.Data;
            uint[] subMeshOffsets = new uint[subMeshCount];
            int lastSubMeshOffset = -1;
            for (uint i = 0; i < triangleCount; i++)
            {
                Triangle triangle = triArr[i];
                if (triangle.SubMeshIndex != lastSubMeshOffset)
                {
                    for (int j = lastSubMeshOffset + 1; j < triangle.SubMeshIndex; j++)
                    {
                        subMeshOffsets[j] = i;
                    }
                    subMeshOffsets[triangle.SubMeshIndex] = i;
                    lastSubMeshOffset = triangle.SubMeshIndex;
                }
            }
            for (int i = lastSubMeshOffset + 1; i < subMeshCount; i++)
            {
                subMeshOffsets[i] = triangleCount;
            }

            // Then setup the sub-meshes
            for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
            {
                uint startOffset = subMeshOffsets[subMeshIndex];
                if (startOffset < triangleCount)
                {
                    uint endOffset = subMeshIndex + 1 < subMeshCount ? subMeshOffsets[subMeshIndex + 1] : triangleCount;
                    uint subMeshTriangleCount = endOffset - startOffset;
                    if (subMeshTriangleCount < 0)
                    {
                        subMeshTriangleCount = 0;
                    }

                    uint[] subMeshIndices = new uint[subMeshTriangleCount * 3];

                    for (uint triangleIndex = startOffset; triangleIndex < endOffset; triangleIndex++)
                    {
                        Triangle triangle = triArr[triangleIndex];
                        uint offset = (triangleIndex - startOffset) * 3;
                        subMeshIndices[offset] = triangle.V0;
                        subMeshIndices[offset + 1] = triangle.V1;
                        subMeshIndices[offset + 2] = triangle.V2;
                    }

                    indices[subMeshIndex] = subMeshIndices;
                }
                else
                {
                    // This mesh doesn't have any triangles left
                    indices[subMeshIndex] = [];
                }
            }

            Mesh newMesh = new(vertices, indices);

            if (vertNormals != null)
            {
                newMesh.Normals = vertNormals.Data;
            }
            if (vertTangents != null)
            {
                newMesh.Tangents = vertTangents.Data;
            }
            if (vertColors != null)
            {
                newMesh.Colors = vertColors.Data;
            }
            if (vertBoneWeights != null)
            {
                newMesh.BoneWeights = vertBoneWeights.Data;
            }

            if (vertUV2D != null)
            {
                for (int i = 0; i < Mesh.UVChannelCount; i++)
                {
                    if (vertUV2D[i] != null)
                    {
                        Vector2[]? uvSet = vertUV2D[i]?.Data;
                        newMesh.SetUVs(i, uvSet);
                    }
                }
            }

            if (vertUV3D != null)
            {
                for (int i = 0; i < Mesh.UVChannelCount; i++)
                {
                    if (vertUV3D[i] != null)
                    {
                        Vector3[]? uvSet = vertUV3D[i]?.Data;
                        newMesh.SetUVs(i, uvSet);
                    }
                }
            }

            if (vertUV4D != null)
            {
                for (int i = 0; i < Mesh.UVChannelCount; i++)
                {
                    if (vertUV4D[i] != null)
                    {
                        Vector4[]? uvSet = vertUV4D[i]?.Data;
                        newMesh.SetUVs(i, uvSet);
                    }
                }
            }

            return newMesh;
        }

        #endregion

        #endregion
    }
}