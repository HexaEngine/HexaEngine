namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents different shader backends.
    /// </summary>
    public enum ShaderBackend
    {
        /// <summary>
        /// Shader backend for SPIR-V.
        /// </summary>
        SpirV,

        /// <summary>
        /// Shader backend for HLSL.
        /// </summary>
        HLSL,

        /// <summary>
        /// Shader backend for GLSL.
        /// </summary>
        GLSL,

        /// <summary>
        /// Shader backend for Metal.
        /// </summary>
        Metal
    }
}