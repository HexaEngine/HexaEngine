#nullable disable

namespace HexaEngine.Editor
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Unsafes;
    using System.Numerics;

    /// <summary>
    /// Represents a queue of debugging drawing commands for rendering primitives.
    /// </summary>
    public unsafe class DebugDrawCommandList
    {
        private UnsafeList<DebugDrawVert> vertices = new(vertexBufferSize);
        private UnsafeList<uint> indices = new(indexBufferSize);

        private UnsafeList<DebugDrawCommand> queue = [];

        private uint nVerticesCmd;
        private uint nIndicesCmd;

        private uint nVerticesTotal;
        private uint nIndicesTotal;

        private const int vertexBufferSize = 5000;
        private const int indexBufferSize = 10000;

        public DebugDrawCommandList(DebugDrawCommandListType type)
        {
            Type = type;
        }

        /// <summary>
        /// Gets the total count of vertices used in the commands in the queue.
        /// </summary>
        public uint VertexCount => nVerticesTotal;

        /// <summary>
        /// Gets the total count of indices used in the commands in the queue.
        /// </summary>
        public uint IndexCount => nIndicesTotal;

        public DebugDrawVert* Vertices => vertices.Data;

        public uint* Indices => indices.Data;

        public UnsafeList<DebugDrawCommand> Commands => queue;

        public DebugDrawCommandListType Type { get; }

        public void ReserveGeometry(uint nVertices, uint nIndices)
        {
            vertices.Resize(vertices.Size + nVertices);
            indices.Resize(indices.Size + nIndices);
            nVerticesCmd += nVertices;
            nIndicesCmd += nIndices;
        }

        public void ReserveVerts(uint nVertices)
        {
            vertices.Resize(vertices.Size + nVertices);
            nVerticesCmd += nVertices;
        }

        public void ReserveIndices(uint nIndices)
        {
            indices.Resize(indices.Size + nIndices);
            nIndicesCmd += nIndices;
        }

        public void AddVertex(DebugDrawVert v)
        {
            vertices.PushBack(v);
            nVerticesCmd++;
        }

        public void AddVertexRange(DebugDrawVert[] v)
        {
            vertices.AppendRange(v);
            nVerticesCmd += (uint)v.Length;
        }

        public void AddIndexRange(uint[] i)
        {
            indices.AppendRange(i);
            nIndicesCmd += (uint)i.Length;
        }

        public void AddIndex(uint i)
        {
            indices.PushBack(i);
            nIndicesCmd++;
        }

        public void AddFace(uint i0, uint i1, uint i2)
        {
            indices.PushBack(i0);
            indices.PushBack(i1);
            indices.PushBack(i2);
            nIndicesCmd += 3;
        }

        public void RecordCmd(PrimitiveTopology topology)
        {
            RecordCmd(topology, 0, false);
        }

        public void RecordCmd(PrimitiveTopology topology, nint texId)
        {
            RecordCmd(topology, texId, false);
        }

        public void RecordCmd(PrimitiveTopology topology, nint texId, bool enableDepth)
        {
            DebugDrawCommand cmd = new(topology, nVerticesCmd, nIndicesCmd, nVerticesTotal, nIndicesTotal, texId, enableDepth);
            queue.PushBack(cmd);
            nVerticesTotal += nVerticesCmd;
            nIndicesTotal += nIndicesCmd;
            nIndicesCmd = 0;
            nVerticesCmd = 0;
        }

        public void Compact()
        {
            if (nVerticesTotal == 0)
                return;

            if (vertices.Count + vertexBufferSize < vertices.Capacity)
            {
                vertices.ShrinkToFit();
            }

            if (indices.Count + indexBufferSize < indices.Capacity)
            {
                indices.ShrinkToFit();
            }

            if (queue.Count < 2)
            {
                return;
            }

            DebugDrawCommand last = queue.First();

            for (int i = 1; i < queue.Count; i++)
            {
                var cmd = queue[i];

                if (last.CanMerge(cmd))
                {
                    queue.RemoveAt(i);
                    i--;

                    var vOffset = last.VertexCount;
                    for (uint j = 0; j < cmd.IndexCount; j++)
                    {
                        indices[cmd.IndexOffset + j] += vOffset;
                    }

                    last.VertexCount += cmd.VertexCount;
                    last.IndexCount += cmd.IndexCount;

                    cmd = last;
                    queue[i] = cmd;
                }

                last = cmd;
            }
        }

        public void Clear()
        {
            queue.Clear();
            vertices.Clear();
            indices.Clear();
            nVerticesTotal = 0;
            nVerticesCmd = 0;
            nIndicesTotal = 0;
            nIndicesCmd = 0;
        }
    }
}