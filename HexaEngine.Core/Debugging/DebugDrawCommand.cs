#nullable disable

namespace HexaEngine.Editor
{
    using HexaEngine.Core.Graphics;

    /// <summary>
    /// Represents a command for debugging drawing, used for rendering primitives.
    /// </summary>
    public unsafe class DebugDrawCommand
    {
        /// <summary>
        /// Gets or sets the primitive topology used for rendering.
        /// </summary>
        public PrimitiveTopology Topology;

        /// <summary>
        /// Gets or sets a pointer to the array of vertices used for rendering.
        /// </summary>
        public DebugDrawVert* Vertices;

        /// <summary>
        /// Gets or sets a pointer to the array of indices used for rendering.
        /// </summary>
        public ushort* Indices;

        /// <summary>
        /// Gets or sets the number of indices in the index array.
        /// </summary>
        public uint nIndices;

        /// <summary>
        /// Gets or sets the number of vertices in the vertex array.
        /// </summary>
        public uint nVertices;

        /// <summary>
        /// Gets or sets a native integer representing a texture ID, if applicable.
        /// </summary>
        public nint TextureId;

        /// <summary>
        /// Gets or sets a value indicating whether depth testing should be enabled for rendering.
        /// </summary>
        public bool EnableDepth;
    }

    /// <summary>
    /// Represents a queue of debugging drawing commands for rendering primitives.
    /// </summary>
    public unsafe class DebugDrawCommandQueue
    {
        private readonly Dictionary<string, DebugDrawCommand> cache = new();
        private readonly List<string> clearQueue = new();

        /// <summary>
        /// Gets a list of debug drawing commands to be executed.
        /// </summary>
        public readonly List<DebugDrawCommand> Commands = new();

        /// <summary>
        /// Gets the total count of vertices used in the commands in the queue.
        /// </summary>
        public uint VertexCount;

        /// <summary>
        /// Gets the total count of indices used in the commands in the queue.
        /// </summary>
        public uint IndexCount;

        /// <summary>
        /// Adds a debug drawing command to the queue or updates an existing command with the specified ID.
        /// </summary>
        /// <param name="id">The unique identifier for the command.</param>
        /// <param name="topology">The primitive topology used for rendering.</param>
        /// <param name="nIndex">The number of indices in the command.</param>
        /// <param name="nVertex">The number of vertices in the command.</param>
        /// <param name="command">The resulting debug drawing command.</param>
        /// <returns>True if a new command was created or an existing command was updated; otherwise, false.</returns>
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
            clearQueue.Remove(id);

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

        /// <summary>
        /// Clears unused debug drawing commands and frees associated resources.
        /// </summary>
        public void ClearUnused()
        {
            for (int i = 0; i < clearQueue.Count; i++)
            {
                var id = clearQueue[i];
                var cmd = cache[id];
                cache.Remove(id);

                Free(cmd.Vertices);
                Free(cmd.Indices);
            }
        }

        /// <summary>
        /// Clears the debug drawing queue, removing all commands and resetting counters.
        /// </summary>
        public void Clear()
        {
            Commands.Clear();
            clearQueue.Clear();
            clearQueue.AddRange(cache.Keys);
            VertexCount = 0;
            IndexCount = 0;
        }
    }
}