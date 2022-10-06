namespace HexaEngine.DirectXTex
{
    /// <summary>
    /// Subset here matches D3D10_RESOURCE_MISC_FLAG and D3D11_RESOURCE_MISC_FLAG
    /// </summary>
    [Flags]
    public enum TexMiscFlags : uint
    {
        TEXTURECUBE = 0x4,
    }
}