namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Specifies the graphics backend used by the application.
    /// </summary>
    public enum GraphicsBackend
    {
        /// <summary>
        /// Automatically select the most appropriate graphics backend.
        /// </summary>
        Auto,

        /// <summary>
        /// DirectX 12 (D3D12) graphics backend.
        /// </summary>
        D3D12,

        /// <summary>
        /// DirectX 11 (D3D11) graphics backend.
        /// </summary>
        D3D11,

        /// <summary>
        /// DirectX 11 (D3D11) on top of DirectX 12 (D3D12) graphics backend.
        /// </summary>
        D3D11On12,

        /// <summary>
        /// Vulkan graphics backend.
        /// </summary>
        Vulkan,

        /// <summary>
        /// OpenGL graphics backend.
        /// </summary>
        OpenGL,

        /// <summary>
        /// Metal graphics backend (used on Apple platforms).
        /// </summary>
        Metal
    }
}