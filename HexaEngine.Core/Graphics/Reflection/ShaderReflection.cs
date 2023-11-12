namespace HexaEngine.Core.Graphics.Reflection
{
    /// <summary>
    /// Represents the reflection information obtained from a shader.
    /// </summary>
    public struct ShaderReflection
    {
        /// <summary>
        /// The version of the shader.
        /// </summary>
        public uint Version;

        /// <summary>
        /// A pointer to the creator of the shader.
        /// </summary>
        public unsafe byte* Creator;

        /// <summary>
        /// Flags indicating properties of the shader.
        /// </summary>
        public uint Flags;

        /// <summary>
        /// The number of constant buffers used by the shader.
        /// </summary>
        public uint ConstantBuffers;

        /// <summary>
        /// The number of bound resources.
        /// </summary>
        public uint BoundResources;

        /// <summary>
        /// The number of input parameters.
        /// </summary>
        public uint InputParameters;

        /// <summary>
        /// The number of output parameters.
        /// </summary>
        public uint OutputParameters;

        // ... (continue with comments for other fields)

        /// <summary>
        /// The topology of the output from the geometry shader.
        /// </summary>
        public PrimitiveTopology GSOutputTopology;

        /// <summary>
        /// The maximum output vertex count from the geometry shader.
        /// </summary>
        public uint GSMaxOutputVertexCount;

        /// <summary>
        /// The input primitive type for the geometry shader.
        /// </summary>
        public Primitive InputPrimitive;

        /// <summary>
        /// The number of patch constant parameters.
        /// </summary>
        public uint PatchConstantParameters;

        /// <summary>
        /// The instance count for the control point of the geometry shader.
        /// </summary>
        public uint CGSInstanceCount;

        /// <summary>
        /// The number of control points for the tessellation shader.
        /// </summary>
        public uint CControlPoints;

        /// <summary>
        /// The output primitive type for the hull shader.
        /// </summary>
        public TessellatorOutputPrimitive HSOutputPrimitive;

        /// <summary>
        /// The tessellation partitioning mode for the hull shader.
        /// </summary>
        public TessellatorPartitioning HSPartitioning;

        /// <summary>
        /// The tessellation domain for the hull shader.
        /// </summary>
        public TessellatorDomain TessellatorDomain;

        /// <summary>
        /// The number of barrier instructions.
        /// </summary>
        public uint CBarrierInstructions;

        /// <summary>
        /// The number of interlocked instructions.
        /// </summary>
        public uint CInterlockedInstructions;

        /// <summary>
        /// The number of texture store instructions.
        /// </summary>
        public uint CTextureStoreInstructions;
    }
}