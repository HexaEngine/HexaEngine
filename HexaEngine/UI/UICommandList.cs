namespace HexaEngine.UI
{
    using HexaEngine.Core.Unsafes;
    using System.Numerics;

    public unsafe class UICommandList
    {
        private readonly UnsafeList<RmGuiVertex> vertices = new();
        private UnsafeList<uint> indices = new();
        private UnsafeList<UIDrawCommand> commands = new();

        private uint vertexCountSinceLast;
        private uint indexCountSinceLast;
        private uint vertexCountOffset;
        private uint indexCountOffset;
        private Matrix3x2 transform = Matrix3x2.Identity;

        public UICommandList()
        {
        }

        public int TotalVtxCount => (int)vertices.Size;

        public int TotalIdxCount => (int)indices.Size;

        public UnsafeList<RmGuiVertex> VtxBuffer => vertices;

        public UnsafeList<uint> IdxBuffer => indices;

        public UnsafeList<UIDrawCommand> CmdBuffer => commands;

        public RmGuiVertex* VtxWritePtr => vertices.Back;

        public uint* IdxWritePtr => indices.Back;

        public Matrix3x2 Transform { get => transform; set => transform = value; }

        public uint AddVertex(RmGuiVertex vertex)
        {
            uint idx = vertexCountSinceLast;
            vertices.Add(vertex);
            vertexCountSinceLast++;
            return idx;
        }

        public void AddIndex(uint index)
        {
            indices.Add(index);
            indexCountSinceLast++;
        }

        public void AddFace(uint i0, uint i1, uint i2)
        {
            indices.Add(i0);
            indices.Add(i1);
            indices.Add(i2);
            indexCountSinceLast += 3;
        }

        public void AddFace(int i0, int i1, int i2)
        {
            indices.Add((uint)i0);
            indices.Add((uint)i1);
            indices.Add((uint)i2);
            indexCountSinceLast += 3;
        }

        public RmGuiVertex* ReserveVertices(uint count)
        {
            var s = vertices.Size;
            vertices.Resize(vertices.Size + count);
            vertexCountSinceLast += count;
            return vertices.Data + s;
        }

        public uint* ReserveIndices(uint count)
        {
            var s = indices.Size;
            indices.Resize(indices.Size + count);
            indexCountSinceLast += count;
            return indices.Data + s;
        }

        public void Reserve(uint idxCount, uint vtxCount)
        {
            indices.Resize(indices.Size + idxCount);
            indexCountSinceLast += idxCount;
            vertices.Resize(vertices.Size + vtxCount);
            vertexCountSinceLast += vtxCount;
        }

        public void PrimReserve(int idxCount, int vtxCount)
        {
            indices.Reserve(indices.Size + (uint)idxCount);
            vertices.Reserve(vertices.Size + (uint)vtxCount);
        }

        public void Ex(int idxCount, int vtxCount)
        {
            indices.Resize(indices.Size + (uint)idxCount);
            indexCountSinceLast += (uint)idxCount;
            vertices.Resize(vertices.Size + (uint)vtxCount);
            vertexCountSinceLast += (uint)vtxCount;
        }

        public void RecordDraw()
        {
            UIDrawCommand cmd = new(vertices.Data, indices.Data, vertexCountSinceLast, indexCountSinceLast, vertexCountOffset, indexCountOffset);
            commands.PushBack(cmd);

            if (transform != Matrix3x2.Identity)
            {
                for (uint i = 0; i < vertexCountSinceLast; i++)
                {
                    vertices.Data[vertexCountOffset + i].Position = Vector2.Transform(vertices[vertexCountOffset + i].Position, transform);
                }
            }

            vertexCountOffset = vertices.Size;
            indexCountOffset = indices.Size;
            vertexCountSinceLast = 0;
            indexCountSinceLast = 0;
        }

        public void RecordDraw(nint textureId)
        {
            UIDrawCommand cmd = new(vertices.Data, indices.Data, vertexCountSinceLast, indexCountSinceLast, vertexCountOffset, indexCountOffset, textureId);
            commands.PushBack(cmd);

            if (transform != Matrix3x2.Identity)
            {
                for (uint i = 0; i < vertexCountSinceLast; i++)
                {
                    vertices.Data[vertexCountOffset + i].Position = Vector2.Transform(vertices[vertexCountOffset + i].Position, transform);
                }
            }

            vertexCountOffset = vertices.Size;
            indexCountOffset = indices.Size;
            vertexCountSinceLast = 0;
            indexCountSinceLast = 0;
        }

        public void BeginDraw()
        {
            vertices.Clear();
            indices.Clear();
            commands.Clear();

            indexCountOffset = 0;
            vertexCountOffset = 0;
        }

        public void EndDraw()
        {
            if (commands.Size == 1)
            {
                return;
            }
            var lastCmd = commands[0];
            var lastCmdIdx = 0;
            for (int i = 1; i < commands.Size; i++)
            {
                var cmd = commands[i];
                if (cmd.TextureId == lastCmd.TextureId)
                {
                    if (lastCmd.VertexOffset + lastCmd.VertexCount == cmd.VertexOffset)
                    {
                        lastCmd.VertexCount += cmd.VertexCount;
                        lastCmd.IndexCount += cmd.IndexCount;

                        for (uint j = 0; j < cmd.IndexCount; j++)
                        {
                            indices[j + cmd.IndexOffset] += cmd.VertexOffset;
                        }

                        commands[lastCmdIdx] = lastCmd;
                        commands.RemoveAt(i);
                        i--;

                        continue;
                    }
                }

                lastCmd = cmd;
                lastCmdIdx = i;
            }
        }
    }
}