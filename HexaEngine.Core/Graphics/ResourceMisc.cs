namespace HexaEngine.Core.Graphics
{
    [Flags]
    public enum ResourceMiscFlag : int
    {
        GenerateMips = unchecked((int)1),
        Shared = unchecked((int)2),
        TextureCube = unchecked((int)4),
        DrawIndirectArguments = unchecked((int)16),
        BufferAllowRawViews = unchecked((int)32),
        BufferStructured = unchecked((int)64),
        ResourceClamp = unchecked((int)128),
        SharedKeyedMutex = unchecked((int)256),
        GdiCompatible = unchecked((int)512),
        SharedNTHandle = unchecked((int)2048),
        RestrictedContent = unchecked((int)4096),
        RestrictSharedResource = unchecked((int)8192),
        RestrictSharedResourceDriver = unchecked((int)16384),
        Guarded = unchecked((int)32768),
        TilePool = unchecked((int)131072),
        Tiled = unchecked((int)262144),
        HardwareProtected = unchecked((int)524288),
        SharedDisplayable = unchecked((int)1048576),
        SharedExclusiveWriter = unchecked((int)2097152),
        None = unchecked((int)0)
    }
}