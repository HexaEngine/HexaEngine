namespace HexaEngine.Core.Graphics.Shaders
{
    /// <summary>
    /// Specifies the source language of a shader.
    /// </summary>
    public enum SourceLanguage
    {
        /// <summary>
        /// The shader is written in SPIR-V language.
        /// </summary>
        SpirV,

        /// <summary>
        /// The shader is written in High-Level Shader Language (HLSL).
        /// </summary>
        HLSL,

        /// <summary>
        /// The shader is written in OpenGL Shading Language (GLSL).
        /// </summary>
        GLSL,

        /// <summary>
        /// The shader is written in Metal Shading Language (MSL).
        /// </summary>
        MSL,
    }
}