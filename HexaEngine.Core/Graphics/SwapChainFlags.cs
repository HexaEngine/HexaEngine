namespace HexaEngine.Core.Graphics
{
    [Flags]
    public enum SwapChainFlags
    {
        None = 0x0,
        Nonprerotated = 0x1,
        AllowModeSwitch = 0x2,
        GdiCompatible = 0x4,
        RestrictedContent = 0x8,
        RestrictSharedResourceDriver = 0x10,
        DisplayOnly = 0x20,
        FrameLatencyWaitableObject = 0x40,
        ForegroundLayer = 0x80,
        FullscreenVideo = 0x100,
        YuvVideo = 0x200,
        HWProtected = 0x400,
        AllowTearing = 0x800,
        RestrictedToAllHolographicDisplays = 0x1000
    }
}