namespace HexaEngine.Core.IO.Materials
{
    /// <summary>
    /// Enumeration representing different types of material shaders.
    /// </summary>
    public enum MaterialShaderType
    {
        /// <summary>
        /// Unknown shader type.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Vertex shader.
        /// </summary>
        VertexShader,

        /// <summary>
        /// Hull (Tessellation Control) shader.
        /// </summary>
        HullShader,

        /// <summary>
        /// Domain (Tessellation Evaluation) shader.
        /// </summary>
        DomainShader,

        /// <summary>
        /// Geometry shader.
        /// </summary>
        GeometryShader,

        /// <summary>
        /// Pixel (Fragment) shader.
        /// </summary>
        PixelShader,

        /// <summary>
        /// Compute shader.
        /// </summary>
        ComputeShader,

        /// <summary>
        /// Vertex shader from a file.
        /// </summary>
        VertexShaderFile,

        /// <summary>
        /// Hull (Tessellation Control) shader from a file.
        /// </summary>
        HullShaderFile,

        /// <summary>
        /// Domain (Tessellation Evaluation) shader from a file.
        /// </summary>
        DomainShaderFile,

        /// <summary>
        /// Geometry shader from a file.
        /// </summary>
        GeometryShaderFile,

        /// <summary>
        /// Pixel (Fragment) shader from a file.
        /// </summary>
        PixelShaderFile,

        /// <summary>
        /// Compute shader from a file.
        /// </summary>
        ComputeShaderFile
    }
}