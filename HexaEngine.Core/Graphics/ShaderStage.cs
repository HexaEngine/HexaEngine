namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents the various shader stages.
    /// </summary>
    public enum ShaderStage
    {
        /// <summary>
        /// Vertex shader stage.
        /// </summary>
        Vertex,

        /// <summary>
        /// Hull (tessellation control) shader stage.
        /// </summary>
        Hull,

        /// <summary>
        /// Domain (tessellation evaluation) shader stage.
        /// </summary>
        Domain,

        /// <summary>
        /// Geometry shader stage.
        /// </summary>
        Geometry,

        /// <summary>
        /// Pixel (fragment) shader stage.
        /// </summary>
        Pixel,

        /// <summary>
        /// Compute shader stage.
        /// </summary>
        Compute
    }
}