namespace HexaEngine.Core.Graphics
{
    public struct SwapChainDescription
    {
        public uint Width;
        public uint Height;
        public Format Format;
        public bool Stereo;
        public SampleDescription SampleDesc;
        public uint BufferUsage;
        public uint BufferCount;
        public Scaling Scaling;
        public SwapEffect SwapEffect;
        public SwapChainAlphaMode AlphaMode;
        public SwapChainFlags Flags;
    }
}