#nullable disable

namespace HexaEngine.Core.Debugging
{
    using HexaEngine.Core.Graphics;

    /// <summary>
    /// Represents a command for debugging drawing, used for rendering primitives.
    /// </summary>
    public unsafe struct DebugDrawCommand
    {
        /// <summary>
        /// Gets or sets the primitive topology used for rendering.
        /// </summary>
        public PrimitiveTopology Topology;

        /// <summary>
        /// Gets or sets the number of vertices.
        /// </summary>
        public uint VertexCount;

        /// <summary>
        /// Gets or sets the number of indices.
        /// </summary>
        public uint IndexCount;

        /// <summary>
        /// Gets or sets the vertex offset in the vertex buffer (globally).
        /// </summary>
        public uint VertexOffset;

        /// <summary>
        /// Gets or sets the index offset in the index buffer (globally).
        /// </summary>
        public uint IndexOffset;

        /// <summary>
        /// Gets or sets a native integer representing a texture ID, if applicable.
        /// </summary>
        public nint TextureId;

        /// <summary>
        /// Gets or sets a value indicating whether depth testing should be enabled for rendering.
        /// </summary>
        public bool EnableDepth;

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugDrawCommand"/> struct.
        /// </summary>
        /// <param name="topology">The primitive topology used for rendering.</param>
        /// <param name="vertexCount">The number of vertices.</param>
        /// <param name="indexCount">The number of indices.</param>
        /// <param name="vertexOffset">The vertex offset in the vertex buffer (globally).</param>
        /// <param name="indexOffset">The index offset in the index buffer (globally).</param>
        /// <param name="textureId">A native integer representing a texture ID, if applicable.</param>
        /// <param name="enableDepth">A value indicating whether depth testing should be enabled for rendering.</param>
        public DebugDrawCommand(PrimitiveTopology topology, uint vertexCount, uint indexCount, uint vertexOffset, uint indexOffset, nint textureId, bool enableDepth)
        {
            Topology = topology;
            VertexCount = vertexCount;
            IndexCount = indexCount;
            VertexOffset = vertexOffset;
            IndexOffset = indexOffset;
            TextureId = textureId;
            EnableDepth = enableDepth;
        }

        /// <summary>
        /// Determines whether this <see cref="DebugDrawCommand"/> instance can be merged the specified other <see cref="DebugDrawCommand"/> instance.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>
        ///   <c>true</c> if this instance can be merged the specified other; otherwise, <c>false</c>.
        /// </returns>
        public readonly bool CanMerge(DebugDrawCommand other)
        {
            // Adjacent topologies are not mergeable!
            if (Topology != PrimitiveTopology.PointList &&
                Topology != PrimitiveTopology.LineList &&
                Topology != PrimitiveTopology.TriangleList)
                return false;

            if (other.Topology != PrimitiveTopology.PointList &&
                other.Topology != PrimitiveTopology.LineList &&
                other.Topology != PrimitiveTopology.TriangleList)
                return false;

            if (other.IndexCount > 8000 || other.VertexCount > 5000)
            {
                return false;
            }

            return Topology == other.Topology && TextureId == other.TextureId && EnableDepth == other.EnableDepth;
        }
    }
}