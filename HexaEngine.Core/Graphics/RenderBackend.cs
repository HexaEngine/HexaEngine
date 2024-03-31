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
        Auto = 0,

        /// <summary>
        /// DirectX 12 (D3D12) graphics backend.
        /// </summary>
        D3D12 = 1,

        /// <summary>
        /// DirectX 11 (D3D11) graphics backend.
        /// </summary>
        D3D11 = 2,

        /// <summary>
        /// DirectX 11 (D3D11) on top of DirectX 12 (D3D12) graphics backend.
        /// </summary>
        D3D11On12 = 4,

        /// <summary>
        /// Vulkan graphics backend.
        /// </summary>
        Vulkan = 8,

        /// <summary>
        /// OpenGL graphics backend.
        /// </summary>
        OpenGL = 16,

        /// <summary>
        /// Metal graphics backend (used on Apple platforms).
        /// </summary>
        Metal = 32,

        /// <summary>
        /// A dummy backend for Unit-Testing.
        /// </summary>
        Dummy = 64,
    }
}