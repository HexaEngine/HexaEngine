namespace HexaEngine.Core.Graphics.Textures
{
    /// <summary>
    /// Subset here matches D3D10_RESOURCE_MISC_FLAG and D3D11_RESOURCE_MISC_FLAG
    /// </summary>
    [Flags]
    public enum TexMiscFlags : uint
    {
        /// <summary>
        /// Generate mipmaps.
        /// </summary>
        GenerateMips = unchecked(1),

        /// <summary>
        /// Resource can be shared.
        /// </summary>
        Shared = unchecked(2),

        /// <summary>
        /// Texture is a cube map.
        /// </summary>
        TextureCube = unchecked(4),

        /// <summary>
        /// Buffer contains draw-indirect arguments.
        /// </summary>
        DrawIndirectArguments = unchecked(16),

        /// <summary>
        /// Allow raw views on the buffer.
        /// </summary>
        BufferAllowRawViews = unchecked(32),

        /// <summary>
        /// Buffer is structured.
        /// </summary>
        BufferStructured = unchecked(64),

        /// <summary>
        /// Resource clamp.
        /// </summary>
        ResourceClamp = unchecked(128),

        /// <summary>
        /// Resource can be shared using a keyed mutex.
        /// </summary>
        SharedKeyedMutex = unchecked(256),

        /// <summary>
        /// Resource is compatible with GDI.
        /// </summary>
        GdiCompatible = unchecked(512),

        /// <summary>
        /// Resource has a shared NT handle.
        /// </summary>
        SharedNTHandle = unchecked(2048),

        /// <summary>
        /// Restricted content.
        /// </summary>
        RestrictedContent = unchecked(4096),

        /// <summary>
        /// Restrict shared resource.
        /// </summary>
        RestrictSharedResource = unchecked(8192),

        /// <summary>
        /// Restrict shared resource to the driver.
        /// </summary>
        RestrictSharedResourceDriver = unchecked(16384),

        /// <summary>
        /// Guarded resource.
        /// </summary>
        Guarded = unchecked(32768),

        /// <summary>
        /// Resource is part of a tile pool.
        /// </summary>
        TilePool = unchecked(131072),

        /// <summary>
        /// Tiled resource.
        /// </summary>
        Tiled = unchecked(262144),

        /// <summary>
        /// Hardware-protected resource.
        /// </summary>
        HardwareProtected = unchecked(524288),

        /// <summary>
        /// Resource can be shared and displayed.
        /// </summary>
        SharedDisplayable = unchecked(1048576),

        /// <summary>
        /// Resource is shared with exclusive writer access.
        /// </summary>
        SharedExclusiveWriter = unchecked(2097152),

        /// <summary>
        /// No miscellaneous flags.
        /// </summary>
        None = unchecked(0)
    }
}