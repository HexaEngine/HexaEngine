#nullable disable

namespace HexaEngine.Editor
{
    using HexaEngine.Core.Graphics;

    public unsafe class DebugDrawCommand
    {
        public PrimitiveTopology Topology;
        public DebugDrawVert* Vertices;
        public ushort* Indices;
        public uint nIndices;
        public uint nVertices;
        public nint TextureId;
    }

    public unsafe class DebugDrawCommandQueue
    {
        private readonly Dictionary<string, DebugDrawCommand> cache = new();
        private readonly List<string> clearqueue = new();
        public readonly List<DebugDrawCommand> Commands = new();
        public uint VertexCount;
        public uint IndexCount;

        public bool Draw(string id, PrimitiveTopology topology, uint nIndex, uint nVertex, out DebugDrawCommand command)
        {
            VertexCount += nVertex;
            IndexCount += nIndex;

            if (!cache.TryGetValue(id, out command))
            {
                command = new();
                command.Topology = topology;
                command.nVertices = nVertex;
                command.nIndices = nIndex;
                cache.Add(id, command);
                Commands.Add(command);
                return true;
            }

            Commands.Add(command);
            clearqueue.Remove(id);

            if (command.nIndices != nIndex || command.nVertices != nVertex)
            {
                command.nIndices = nIndex;
                command.nVertices = nVertex;
                Free(command.Vertices);
                Free(command.Indices);
                return true;
            }

            return false;
        }

        public void ClearUnused()
        {
            for (int i = 0; i < clearqueue.Count; i++)
            {
                var id = clearqueue[i];
                var cmd = cache[id];
                cache.Remove(id);

                Free(cmd.Vertices);
                Free(cmd.Indices);
            }
        }

        public void Clear()
        {
            Commands.Clear();
            clearqueue.Clear();
            clearqueue.AddRange(cache.Keys);
            VertexCount = 0;
            IndexCount = 0;
        }
    }
}