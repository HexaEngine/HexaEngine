namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Flags that describe miscellaneous resource options.
    /// </summary>
    [Flags]
    public enum ResourceMiscFlag : int
    {
        /// <summary>
        /// Generate mipmaps for the resource.
        /// </summary>
        GenerateMips = unchecked(1),

        /// <summary>
        /// Resource is shared.
        /// </summary>
        Shared = unchecked(2),

        /// <summary>
        /// Resource is a cube texture.
        /// </summary>
        TextureCube = unchecked(4),

        /// <summary>
        /// Resource is used for indirect draw or dispatch arguments.
        /// </summary>
        DrawIndirectArguments = unchecked(16),

        /// <summary>
        /// Allow raw views of a buffer resource.
        /// </summary>
        BufferAllowRawViews = unchecked(32),

        /// <summary>
        /// Resource is structured buffer.
        /// </summary>
        BufferStructured = unchecked(64),

        /// <summary>
        /// Resource is clampable.
        /// </summary>
        ResourceClamp = unchecked(128),

        /// <summary>
        /// Resource is shared using a keyed mutex.
        /// </summary>
        SharedKeyedMutex = unchecked(256),

        /// <summary>
        /// Resource is compatible with GDI.
        /// </summary>
        GdiCompatible = unchecked(512),

        /// <summary>
        /// Resource is shared using NT handle.
        /// </summary>
        SharedNTHandle = unchecked(2048),

        /// <summary>
        /// Resource contains restricted content.
        /// </summary>
        RestrictedContent = unchecked(4096),

        /// <summary>
        /// Restrict access to the resource.
        /// </summary>
        RestrictSharedResource = unchecked(8192),

        /// <summary>
        /// Restrict access to the resource for a driver.
        /// </summary>
        RestrictSharedResourceDriver = unchecked(16384),

        /// <summary>
        /// Resource is guarded.
        /// </summary>
        Guarded = unchecked(32768),

        /// <summary>
        /// Resource is in a tile pool.
        /// </summary>
        TilePool = unchecked(131072),

        /// <summary>
        /// Resource is tiled.
        /// </summary>
        Tiled = unchecked(262144),

        /// <summary>
        /// Resource is hardware protected.
        /// </summary>
        HardwareProtected = unchecked(524288),

        /// <summary>
        /// Resource is shared and displayable.
        /// </summary>
        SharedDisplayable = unchecked(1048576),

        /// <summary>
        /// Resource is shared with exclusive writer access.
        /// </summary>
        SharedExclusiveWriter = unchecked(2097152),

        /// <summary>
        /// No specific flags.
        /// </summary>
        None = unchecked(0)
    }
}