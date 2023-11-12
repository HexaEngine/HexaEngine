namespace HexaEngine.Core.Graphics.Structs
{
    /// <summary>
    /// Represents the arguments for drawing indexed instanced geometry using indirect rendering.
    /// </summary>
    public struct DrawIndexedInstancedIndirectArgs
    {
        /// <summary>
        /// Gets or sets the number of indices per instance.
        /// </summary>
        public uint IndexCountPerInstance;

        /// <summary>
        /// Gets or sets the number of instances to draw.
        /// </summary>
        public uint InstanceCount;

        /// <summary>
        /// Gets or sets the location of the first index to read from the index buffer.
        /// </summary>
        public uint StartIndexLocation;

        /// <summary>
        /// Gets or sets the base vertex location.
        /// </summary>
        public int BaseVertexLocation;

        /// <summary>
        /// Gets or sets the start instance location.
        /// </summary>
        public uint StartInstanceLocation;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawIndexedInstancedIndirectArgs"/> struct.
        /// </summary>
        /// <param name="indexCountPerInstance">The number of indices per instance.</param>
        /// <param name="instanceCount">The number of instances to draw.</param>
        /// <param name="startIndexLocation">The location of the first index to read from the index buffer.</param>
        /// <param name="baseVertexLocation">The base vertex location.</param>
        /// <param name="startInstanceLocation">The start instance location.</param>
        public DrawIndexedInstancedIndirectArgs(uint indexCountPerInstance, uint instanceCount, uint startIndexLocation, int baseVertexLocation, uint startInstanceLocation)
        {
            IndexCountPerInstance = indexCountPerInstance;
            InstanceCount = instanceCount;
            StartIndexLocation = startIndexLocation;
            BaseVertexLocation = baseVertexLocation;
            StartInstanceLocation = startInstanceLocation;
        }
    }
}