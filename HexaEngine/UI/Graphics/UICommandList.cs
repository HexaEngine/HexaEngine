namespace HexaEngine.UI.Graphics
{
    using HexaEngine.Core.Unsafes;
    using System.Numerics;

    public unsafe class UICommandList
    {
        private UnsafeList<UIVertex> vertices = new();
        private UnsafeList<uint> indices = new();
        private List<UIDrawCommand> commands = new();
        private readonly Stack<ClipRectangle> clipRectStack = new();

        private ClipRectangle clipRect;
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

        public UnsafeList<UIVertex> VtxBuffer => vertices;

        public UnsafeList<uint> IdxBuffer => indices;

        public List<UIDrawCommand> CmdBuffer => commands;

        public UIVertex* VtxWritePtr => vertices.Back;

        public uint* IdxWritePtr => indices.Back;

        public Matrix3x2 Transform { get => transform; set => transform = value; }

        public ClipRectangle ClipRect => clipRect;

        public void PushClipRect(ClipRectangle clipRect)
        {
            clipRectStack.Push(this.clipRect);
            this.clipRect = clipRect;
        }

        public void PopClipRect()
        {
            clipRect = clipRectStack.Pop();
        }

        public uint AddVertex(UIVertex vertex)
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

        public UIVertex* ReserveVertices(uint count)
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

        public void RecordDraw(UICommandType commandType = UICommandType.DrawPrimitive, Brush? brush = null, nint textureId0 = 0, nint textureId1 = 0)
        {
            UIDrawCommand cmd = new(vertices.Data, indices.Data, vertexCountSinceLast, indexCountSinceLast, vertexCountOffset, indexCountOffset, clipRect, commandType, brush, textureId0, textureId1);
            commands.Add(cmd);

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

        public void ExecuteCommandList(UICommandList commandList)
        {
            for (int i = 0; i < commandList.commands.Count; i++)
            {
                var cmd = commandList.commands[i];
                cmd.VertexOffset += vertexCountOffset;
                cmd.IndexOffset += indexCountOffset;
                cmd.ClipRect = clipRect;
                commands.Add(cmd);
            }

            PrimReserve(commandList.TotalIdxCount, commandList.TotalVtxCount);

            vertices.AppendRange(commandList.vertices.Data, commandList.vertices.Size);
            indices.AppendRange(commandList.indices.Data, commandList.indices.Size);

            if (transform != Matrix3x2.Identity)
            {
                for (uint i = 0; i < commandList.vertices.Size; i++)
                {
                    vertices.Data[vertexCountOffset + i].Position = Vector2.Transform(vertices[vertexCountOffset + i].Position, transform);
                }
            }

            vertexCountOffset = vertices.Size;
            indexCountOffset = indices.Size;
        }

        public void BeginDraw()
        {
            vertices.Clear();
            indices.Clear();
            commands.Clear();
            clipRectStack.Clear();

            indexCountOffset = 0;
            vertexCountOffset = 0;
        }

        public void EndDraw()
        {
            if (commands.Count < 2)
            {
                return;
            }

            var lastCmd = commands[0];
            var lastCmdIdx = 0;
            for (int i = 1; i < commands.Count; i++)
            {
                var cmd = commands[i];
                if (cmd.Type == lastCmd.Type && cmd.TextureId0 == lastCmd.TextureId0 && cmd.TextureId1 == lastCmd.TextureId1 && cmd.Brush == lastCmd.Brush)
                {
                    if (lastCmd.VertexOffset + lastCmd.VertexCount == cmd.VertexOffset)
                    {
                        for (uint j = 0; j < cmd.IndexCount; j++)
                        {
                            indices[j + cmd.IndexOffset] += lastCmd.VertexCount;
                        }

                        lastCmd.VertexCount += cmd.VertexCount;
                        lastCmd.IndexCount += cmd.IndexCount;

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

        public void Dispose()
        {
            vertices.Release();
            indices.Release();
        }
    }
}