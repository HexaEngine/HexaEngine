namespace HexaEngine.Core.Graphics.Reflection
{
    /// <summary>
    /// Describes the binding information for a shader input in HLSL.
    /// </summary>
    public struct ShaderInputBindDescription
    {
        /// <summary>
        /// The name of the shader input.
        /// </summary>
        public string Name;

        /// <summary>
        /// The type of the shader input.
        /// </summary>
        public ShaderInputType Type;

        /// <summary>
        /// The bind point for the shader input.
        /// </summary>
        public uint BindPoint;

        /// <summary>
        /// The bind count for the shader input.
        /// </summary>
        public uint BindCount;

        /// <summary>
        /// Flags associated with the shader input.
        /// </summary>
        public uint UFlags;

        /// <summary>
        /// The return type of the shader input resource.
        /// </summary>
        public ResourceReturnType ReturnType;

        /// <summary>
        /// The dimension of the shader input resource.
        /// </summary>
        public SrvDimension Dimension;

        /// <summary>
        /// The number of samples for the shader input resource.
        /// </summary>
        public uint NumSamples;
    }
}