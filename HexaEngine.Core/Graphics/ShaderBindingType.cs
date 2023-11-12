namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Specifies the type of shader binding.
    /// </summary>
    public enum ShaderBindingType
    {
        /// <summary>
        /// Constant buffer binding (e.g., cX).
        /// </summary>
        ConstantBuffer,

        /// <summary>
        /// Shader resource binding (e.g., tX).
        /// </summary>
        ShaderResource,

        /// <summary>
        /// Sampler state binding (e.g., sX).
        /// </summary>
        SamplerState,

        /// <summary>
        /// Unordered access binding (e.g., uX).
        /// </summary>
        UnorderedAccess
    }
}