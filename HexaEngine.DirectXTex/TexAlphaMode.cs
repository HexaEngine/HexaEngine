namespace HexaEngine.DirectXTex
{
    /// <summary>
    /// Matches DDS_ALPHA_MODE, encoded in MISC_FLAGS2
    /// </summary>
    public enum TexAlphaMode
    {
        UNKNOWN = 0,
        STRAIGHT = 1,
        PREMULTIPLIED = 2,
        OPAQUE = 3,
        CUSTOM = 4,
    }
}