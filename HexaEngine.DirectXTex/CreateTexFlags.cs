namespace HexaEngine.DirectXTex
{
    [Flags]
    public enum CreateTexFlags : uint
    {
        Default = 0,
        ForceSRGB = 0x1,
        IgnoreSRGB = 0x2,
    }
}