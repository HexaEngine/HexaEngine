namespace HexaEngine.DirectXTex
{
    [Flags]
    public enum TexFrFlags
    {
        ROTATE0 = 0x0,
        ROTATE90 = 0x1,
        ROTATE180 = 0x2,
        ROTATE270 = 0x3,
        FLIP_HORIZONTAL = 0x08,
        FLIP_VERTICAL = 0x10,
    }
}