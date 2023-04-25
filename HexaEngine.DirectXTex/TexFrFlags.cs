namespace HexaEngine.DirectXTex
{
    [Flags]
    public enum TexFrFlags
    {
        Rotate0 = 0x0,
        Rotate90 = 0x1,
        Rotate180 = 0x2,
        Rotate270 = 0x3,
        FlipHorizontal = 0x08,
        FlipVertical = 0x10,
    }
}