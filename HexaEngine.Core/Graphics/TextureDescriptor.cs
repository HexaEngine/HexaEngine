namespace HexaEngine.Core.Graphics
{
    public record struct TextureDescriptor
    {
        public TextureDimension Dimension;
        public Format Format;
        public int Width;
        public int Height;
        public int DepthOrArraySize;
        public int MipLevels;
        public Usage Usage;
        public BindFlags Bind;
        public CpuAccessFlags CpuAccess;
        public ResourceMiscFlag Misc;
        public int SampleCount;
        public SamplerDescription Sampler;
    }
}