namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Mathematics;

    public struct SwapChainFullscreenDescription
    {
        public Rational RefreshRate;
        public ModeScanlineOrder ScanlineOrdering;
        public ModeScaling Scaling;
        public bool Windowed;
    }
}