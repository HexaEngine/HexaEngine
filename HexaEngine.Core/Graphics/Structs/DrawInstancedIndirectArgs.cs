namespace HexaEngine.Core.Graphics.Structs
{
    /// <summary>
    /// Represents the arguments for drawing instanced geometry using indirect rendering.
    /// </summary>
    public struct DrawInstancedIndirectArgs
    {
        /// <summary>
        /// Gets or sets the number of vertices per instance.
        /// </summary>
        public uint VertexCountPerInstance;

        /// <summary>
        /// Gets or sets the number of instances to draw.
        /// </summary>
        public uint InstanceCount;

        /// <summary>
        /// Gets or sets the location of the first vertex to read from the vertex buffer.
        /// </summary>
        public uint StartVertexLocation;

        /// <summary>
        /// Gets or sets the start instance location.
        /// </summary>
        public uint StartInstanceLocation;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawInstancedIndirectArgs"/> struct.
        /// </summary>
        /// <param name="vertexCountPerInstance">The number of vertices per instance.</param>
        /// <param name="instanceCount">The number of instances to draw.</param>
        /// <param name="startVertexLocation">The location of the first vertex to read from the vertex buffer.</param>
        /// <param name="startInstanceLocation">The start instance location.</param>
        public DrawInstancedIndirectArgs(uint vertexCountPerInstance, uint instanceCount, uint startVertexLocation, uint startInstanceLocation)
        {
            VertexCountPerInstance = vertexCountPerInstance;
            InstanceCount = instanceCount;
            StartVertexLocation = startVertexLocation;
            StartInstanceLocation = startInstanceLocation;
        }
    }
}